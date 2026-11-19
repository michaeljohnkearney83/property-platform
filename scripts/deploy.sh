#!/usr/bin/env bash
# Deploys one environment. Run by .github/workflows/deploy-env.yml after `az login`.
#
# Required environment variables:
#   ENVIRONMENT              dev | staging | prod
#   IMAGE_TAG                commit SHA of the images already pushed to GHCR
#   GHCR_REPO                e.g. ghcr.io/owner/property-platform
#   GHCR_USERNAME, GHCR_TOKEN  credentials ACR uses to import from GHCR
#   POSTGRES_ADMIN_PASSWORD, AUTH_AUTHORITY, AUTH_AUDIENCE  (read by the .bicepparam files)
# Optional:
#   LOCATION                 default uksouth
set -euo pipefail

: "${ENVIRONMENT:?}" "${IMAGE_TAG:?}" "${GHCR_REPO:?}" "${GHCR_USERNAME:?}" "${GHCR_TOKEN:?}"
LOCATION="${LOCATION:-uksouth}"
RG="rg-propertyplatform-${ENVIRONMENT}"
PARAMS="infra/env/${ENVIRONMENT}.bicepparam"
TAG="${IMAGE_TAG}"

az group create -n "$RG" -l "$LOCATION" --tags product=property-platform environment="$ENVIRONMENT" -o none

# 1) First run only: create everything with placeholder images so the registry exists.
if [ -z "$(az acr list -g "$RG" --query '[0].name' -o tsv)" ]; then
  echo "Bootstrap deployment (no registry yet)"
  IMAGE_TAG="" az deployment group create -g "$RG" -f infra/main.bicep -p "$PARAMS" -n "bootstrap-${TAG}" -o none
fi

ACR=$(az acr list -g "$RG" --query '[0].name' -o tsv)
LOGIN=$(az acr show -n "$ACR" --query loginServer -o tsv)

# 2) Copy the exact images CI built and tested from GHCR into this environment's ACR (server-side).
for img in api web migrations; do
  echo "Importing ${img}:${TAG}"
  az acr import -n "$ACR" --force \
    --source "${GHCR_REPO}/${img}:${TAG}" \
    --image "propertyplatform-${img}:${TAG}" \
    --username "$GHCR_USERNAME" --password "$GHCR_TOKEN" -o none
done

# 3) Run database migrations BEFORE the new API revision starts (expand/contract migrations only).
JOB=$(az containerapp job list -g "$RG" --query "[?ends_with(name,'-migrate')].name | [0]" -o tsv)
IDENTITY=$(az identity list -g "$RG" --query '[0].id' -o tsv)
az containerapp job update -n "$JOB" -g "$RG" --image "${LOGIN}/propertyplatform-migrations:${TAG}" \
  --registry-server "$LOGIN" --registry-identity "$IDENTITY" -o none
EXEC=$(az containerapp job start -n "$JOB" -g "$RG" --query name -o tsv)
echo "Migration execution: $EXEC"
STATUS=""
for _ in $(seq 1 90); do
  STATUS=$(az containerapp job execution show -n "$JOB" -g "$RG" --job-execution-name "$EXEC" --query properties.status -o tsv)
  echo "  status: $STATUS"
  [ "$STATUS" = "Succeeded" ] && break
  if [ "$STATUS" = "Failed" ] || [ "$STATUS" = "Stopped" ]; then echo "::error::Migrations failed"; exit 1; fi
  sleep 10
done
[ "$STATUS" = "Succeeded" ] || { echo "::error::Migrations timed out"; exit 1; }

# 4) Deploy infrastructure + new app revisions with this commit's images.
IMAGE_TAG="$TAG" az deployment group create -g "$RG" -f infra/main.bicep -p "$PARAMS" -n "release-${TAG}" -o none

# 5) Smoke test.
API=$(az deployment group show -g "$RG" -n "release-${TAG}" --query properties.outputs.apiUrl.value -o tsv)
WEB=$(az deployment group show -g "$RG" -n "release-${TAG}" --query properties.outputs.webUrl.value -o tsv)
curl --fail --retry 10 --retry-delay 10 --retry-all-errors -sS "$API/health/ready"; echo
curl --fail --retry 10 --retry-delay 10 --retry-all-errors -sS "$WEB/api/health"; echo

echo "Deployed ${ENVIRONMENT}: ${WEB}"
if [ -n "${GITHUB_OUTPUT:-}" ]; then echo "web_url=${WEB}" >> "$GITHUB_OUTPUT"; fi

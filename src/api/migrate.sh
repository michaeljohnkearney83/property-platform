#!/bin/sh
# Applies every module's migration bundle in name order. Fails fast.
set -eu
: "${ConnectionStrings__Platform:?ConnectionStrings__Platform must be set}"
for bundle in /bundles/*; do
  echo "Applying $(basename "$bundle")"
  "$bundle" --connection "$ConnectionStrings__Platform"
done
echo "All migrations applied."

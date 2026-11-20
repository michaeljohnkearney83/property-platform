# Property Platform

[![Deploy](https://github.com/michaeljohnkearney83/property-platform/actions/workflows/deploy.yml/badge.svg?branch=main)](https://github.com/michaeljohnkearney83/property-platform/actions/workflows/deploy.yml)
[![CodeQL](https://github.com/michaeljohnkearney83/property-platform/actions/workflows/codeql.yml/badge.svg?branch=main)](https://github.com/michaeljohnkearney83/property-platform/actions/workflows/codeql.yml)
[![Release](https://img.shields.io/github/v/release/michaeljohnkearney83/property-platform?include_prereleases&sort=semver)](https://github.com/michaeljohnkearney83/property-platform/releases)
![.NET](https://img.shields.io/badge/.NET-10%20LTS-512BD4?logo=dotnet)
![Next.js](https://img.shields.io/badge/Next.js-15-000000?logo=nextdotjs)
![Node](https://img.shields.io/badge/Node-24%20LTS-339933?logo=nodedotjs)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Azure](https://img.shields.io/badge/hosted%20on-Azure-0078D4)

Multi-tenant SaaS for residential property management: portfolio, landlords, tenants, tenancies, rent and client accounting, compliance, maintenance, documents and reporting.

<!-- toc -->

**Contents**

- [Repository layout](#repository-layout)
- [Prerequisites](#prerequisites)
- [Run locally](#run-locally)
   - [Local authentication](#local-authentication)
- [Tests](#tests)
- [Multi-tenancy (organisations)](#multi-tenancy-organisations)
- [Compliance](#compliance)
- [Adding a module](#adding-a-module)
- [CI/CD (GitHub Actions)](#cicd-github-actions)
   - [CI: .github/workflows/ci.yml](#ci-githubworkflowsciyml)
   - [Branching and release flow (trunk-based)](#branching-and-release-flow-trunk-based)
   - [Release: .github/workflows/deploy.yml](#release-githubworkflowsdeployyml)
- [Versioning and releases](#versioning-and-releases)
- [Azure environments](#azure-environments)
   - [Setup checklist](#setup-checklist)
   - [Manual infrastructure deploy](#manual-infrastructure-deploy)
- [Known gaps and next steps](#known-gaps-and-next-steps)

<!-- /toc -->

| Layer | Technology | Version |
|---|---|---|
| Backend | ASP.NET Core minimal APIs, EF Core + Npgsql, modular monolith | .NET 10 (LTS), EF Core 10 |
| Frontend | Next.js (App Router, TypeScript, Tailwind) | Next.js 15, React 19, Node 24 (LTS) |
| Database | PostgreSQL with row-level security (RLS) per organisation, pgvector | PostgreSQL 16 |
| Hosting | Azure (UK South): Container Apps, PostgreSQL Flexible Server, Blob Storage, Key Vault, Front Door + WAF, Application Insights |
| IaC | Bicep (`infra/`) |
| Source control | GitHub |
| CI/CD | GitHub Actions (`.github/workflows/`), images in GitHub Container Registry |
| Agile / work tracking | Azure DevOps Boards (only) |
| Identity | Microsoft Entra External ID (customers) / Entra ID (staff SSO) |

## Repository layout

```
src/api/                  .NET solution (PropertyPlatform.sln)
  Host/                   ASP.NET Core entry point, auth, org resolution, health checks
  SharedKernel/           IModule, organisation context, RLS connection interceptor
  Modules/<Name>/         One project per business module (own schema, own endpoints)
  Tests/                  xUnit: ledger rules, architecture rules, compliance guardrails
  Dockerfile              API image
  Dockerfile.migrations   EF Core migration bundles (run as a Container Apps Job)
src/web/                  Next.js app (staff UI; portals to follow)
infra/main.bicep          One environment = one resource group
infra/modules/            monitoring, network, postgres, keyvault, registry, storage, containerapps, frontdoor
infra/env/*.bicepparam    dev / uat / prod parameters
.github/workflows/ci.yml          Build, test, lint, Bicep, Docker images (every PR; called by deploy.yml)
.github/workflows/deploy.yml      On push to main: CI -> publish to GHCR -> dev -> uat (approval) -> prod (releases only, approval)
.github/workflows/deploy-env.yml  Deploys one environment (OIDC login to Azure)
.github/workflows/codeql.yml      CodeQL security scanning (C#, TypeScript, workflows)
.github/workflows/pr-title.yml    Enforces Conventional Commit PR titles (they drive versioning)
release-please-config.json        Release automation config (version.txt, CHANGELOG.md, version bumps)
.github/dependabot.yml            Weekly dependency update PRs (NuGet, npm, Actions, Docker)
scripts/deploy.sh                 The release steps for one environment
scripts/readme-toc.py             Regenerates this README's Contents menu (CI checks it is current)
db/init/                  Local Postgres bootstrap (roles, extensions)
docs/adr/                 Architecture decision records
docs/compliance/          UK lettings and UK GDPR rules with permanent IDs (read by people and AI tools)
.claude/skills/compliance/  Claude Code skill: which compliance rules apply to a change + checklist
.github/pull_request_template.md  PR checklist, including the compliance section
```

## Prerequisites

- .NET SDK 10
- Node.js 24
- Docker Desktop
- Azure CLI with Bicep (`az bicep install`), only needed for infrastructure work

## Run locally

1. Start Postgres (with pgvector) and Azurite:

   ```bash
   docker compose up -d
   ```

2. Apply database migrations:

   ```bash
   cd src/api && dotnet tool restore && dotnet ef database update -p Modules/Portfolio -s Host -c PortfolioDbContext
   ```

3. Run the API on http://localhost:5080. The OpenAPI document is at `/openapi/v1.json`.

   ```bash
   dotnet run --project src/api/Host
   ```

4. Run the web app on http://localhost:3000:

   ```bash
   cd src/web && cp .env.example .env.local && npm install && npm run dev
   ```

To run everything in containers instead, use `docker compose --profile app up -d --build`.

### Local authentication

In `Development` with no `Auth:Authority` configured, the API uses a **dev-only auth handler**. Every request is signed in as an Admin of the demo organisation `00000000-0000-0000-0000-000000000001`. You can override the organisation with an `X-Org-Id` header to test isolation. This handler cannot be enabled outside Development: the API refuses to start without `Auth:Authority`.

Create a property:

```bash
curl -X POST http://localhost:5080/api/portfolio/properties -H "Content-Type: application/json" -d '{"reference":"P-001","addressLine1":"1 High Street","town":"Leeds","postcode":"ls1 1aa","type":"Flat","bedrooms":2}'
```

## Tests

```bash
dotnet test src/api/PropertyPlatform.sln
```

```bash
cd src/web && npm run lint && npm run typecheck && npm run build
```

## Multi-tenancy (organisations)

- The SaaS customer is an **Organisation**. "Tenant" always means a renter.
- Every org-owned table has an `OrgId` column. Access is restricted twice:
  1. EF Core global query filters (`HasQueryFilter`).
  2. Postgres RLS policies created in migrations. `OrgRlsConnectionInterceptor` sets `app.org_id` on each connection.
- The app must connect as a **non-superuser**, because superusers bypass RLS.

## Compliance

The platform implements UK lettings law (gas safety, EICR, EPC, deposits, Right to Rent, licensing, the Renters' Rights Act) and must meet UK GDPR. The rules are written down in [`docs/compliance/`](docs/compliance/README.md), one file per area, each rule with a permanent ID (`GAS-01`, `DP-02`...), a source link and a status. Every rule starts as **Draft** until a person has verified it; this is not legal advice.

The rules are built in so that AI-generated code follows them too:

| Layer | What it does |
|---|---|
| `CLAUDE.md` and the `compliance` skill | Tell AI assistants which rules apply to a change and to never invent law |
| Rules as data | Legal periods and dates come from the rules catalogue, with effective dates, not hard-coded values |
| `ComplianceArchitectureTests` | Fails the build if an org-owned entity lacks a query filter or RLS (`ORG-01`, `ORG-02`) or personal data isn't marked `[PersonalData]` (`DP-02`) |
| PR template | A compliance checklist on every PR |

See [ADR 0008](docs/adr/0008-compliance-guardrails-for-ai-assisted-development.md).

## Adding a module

1. `dotnet new classlib -n PropertyPlatform.Modules.<Name> -o src/api/Modules/<Name> -f net10.0`, then reference `SharedKernel` and add the project to the solution and Host.
2. Implement `IModule` and add it to `Host/ModuleCatalog.cs`.
3. Give it its own `DbContext` with `HasDefaultSchema("<name>")`, an org query filter, an RLS policy in its first migration, `[PersonalData]` on personal-data properties, and an `IDesignTimeDbContextFactory`.
4. Add a `dotnet ef migrations bundle` line to `Dockerfile.migrations`.

`ArchitectureTests` fails the build if a module references another module or is missing from the catalogue. `ComplianceArchitectureTests` fails it if step 3's query filter, RLS policy or `[PersonalData]` annotations are missing.

## CI/CD (GitHub Actions)

Azure DevOps is used **only for Boards** (agile work tracking). All build, test and deployment work runs in GitHub Actions.

### CI: `.github/workflows/ci.yml`

CI runs on every push to `feature/**`, `bugfix/**` and `hotfix/**` branches, on every pull request, and on demand (**Actions -> CI -> Run workflow**). `deploy.yml` also calls it on every push to `main`.

| Job | What it checks |
|---|---|
| API (.NET) | `dotnet format` check, Release build with warnings as errors, tests (results uploaded as an artifact), vulnerable NuGet packages |
| Web (Next.js) | `npm ci`, lint, typecheck, production build, `npm audit` (high and above) |
| Infrastructure (Bicep) | Bicep build and lint, and all three `.bicepparam` files compile |
| Container image (api / migrations / web) | Builds each Docker image. On `main`, it pushes them to `ghcr.io/<owner>/<repo>/<image>:<sha>` |

Actions minutes are free for public repositories. Dependabot opens grouped update PRs, and CI checks each one. CodeQL (`codeql.yml`) scans C#, TypeScript and the workflows on every PR, on every push to `main`, and weekly. ADR 0005 lists the delivery best practices this setup follows.

### Branching and release flow (trunk-based)

```
feature/AB#123-short-name --PR--> main --> DEV --[approve]--> UAT --[release + approve]--> PROD
   (CI on every push + PR)  (checks, up to date) auto   "dev is happy"      only versioned releases
```

- **Branches are not environments.** Only `main` is deployed. There are three hosted environments (dev, uat, prod), each in its own Azure resource group with its own URL and database. Each merge produces one build, and that same build is promoted from dev to uat. Released versions then go on to prod.
- **Work branches:** branch off `main` as `feature/AB#<id>-short-name`, `bugfix/...` or `hotfix/...`. Keep them short-lived (days, not weeks).
- **Merging:** open a PR with a Conventional Commit title, e.g. `feat: add tenancies (AB#123)`, and write `Fixes AB#<id>` in its body to close the Boards item on merge. The PR can only merge when all checks pass **and** the branch contains the latest `main`. Use **Update branch** on the PR to merge `main` into it first. PRs are squash-merged, and the branch is then deleted.
- **Getting to UAT:** every merge deploys to **dev** automatically. The run then pauses with a **Review deployments** button. Approve `uat` when you're happy with dev; builds you don't approve never reach UAT.
- **Getting to prod:** only a **release** goes to prod. Merge the `chore(main): release x.y.z` PR, which deploys through dev and UAT like any merge, then approve `prod`. Ordinary merges stop at UAT.
- **Hotfix:** `hotfix/...` -> PR titled `fix: ...` -> merge. Then merge the release PR (a patch release) and approve `uat` and `prod`.
- **Unfinished features** go behind feature flags, not long-lived branches, so `main` can always be released.

### Release: `.github/workflows/deploy.yml`

On every push to `main`, the release runs these steps in order:

1. **CI**, which builds and pushes the images **once**.
2. **Release:** release-please updates the release PR, or creates the release if this push *is* the release PR.
3. **dev:** automatic.
4. **uat:** waits for a required reviewer to approve (the `uat` environment).
5. **prod:** runs **only if step 2 created a release**, then waits for a required reviewer to approve (the `prod` environment).

Every environment gets the exact images that CI tested, so the release build approved in UAT is the one that reaches prod. Deployment is switched off until you set the repository variable `DEPLOY_ENABLED=true`. Until then, `main` is still tested and its images are still published.

Each environment runs `scripts/deploy.sh`:
1. Creates the resource group. On the first run only, it also deploys the infrastructure with placeholder images so the container registry exists.
2. Imports the commit's images from GHCR into that environment's Azure Container Registry (`az acr import`, which runs server-side).
3. Runs EF Core migrations as a Container Apps Job inside the VNet. The database has no public endpoint.
4. Deploys Bicep with the new image tag, which creates new API and web revisions.
5. Smoke-tests `/health/ready` and `/api/health`.

Migrations run before the new code goes live, so they must be **expand/contract**: backwards-compatible with the previous release.

## Versioning and releases

The product follows **Semantic Versioning** (`MAJOR.MINOR.PATCH`). The current version is in [`version.txt`](version.txt) and the history is in [`CHANGELOG.md`](CHANGELOG.md).

| Where | Shows |
|---|---|
| API `GET /version` | `{ "version": "0.1.0", "commit": "5c24518", "environment": "Production" }` |
| OpenAPI document | `info.version` = `0.1.0+5c24518` |
| Web app footer | `Property Platform v0.1.0 (5c24518)` |
| Container images | `ghcr.io/<owner>/<repo>/<image>:<sha>` for every build, plus `:<version>` and `:latest` for releases |

**How a version is chosen.** [release-please](https://github.com/googleapis/release-please) reads the squash-merge commits on `main`, which are the PR titles. Every PR title must therefore be a [Conventional Commit](https://www.conventionalcommits.org/), and the **PR title** check enforces this:

| PR title | Version bump |
|---|---|
| `feat: add tenancy renewals` | minor (0.1.0 -> 0.2.0) |
| `fix: round rent to the penny` / `perf:` / `deps:` | patch (0.1.0 -> 0.1.1) |
| `feat!: ...`, or `BREAKING CHANGE:` in the body | major (minor while below 1.0) |
| `docs:` `refactor:` `ci:` `build:` `chore:` `test:` | no release on its own |

**How a release happens.**
1. After each merge, release-please updates a PR called **`chore(main): release x.y.z`**. It bumps `version.txt`, `Directory.Build.props` and `package.json`, and writes `CHANGELOG.md`.
2. When you're ready to release, merge that PR. This creates the `vX.Y.Z` git tag and the GitHub Release, and adds `:x.y.z` and `:latest` tags to the images.
3. That merge deploys to dev, then UAT after approval, and is the only kind of merge that continues to **prod**, after a second approval.

Ordinary merges deploy to dev and UAT only. They show the last released version plus their commit SHA, so you always know exactly what is running. Prod only ever runs a clean `x.y.z` release.

**One-time setup for releases** (status is tracked in the [setup checklist](#setup-checklist)):
- GitHub Actions is allowed to create PRs (**Settings -> Actions -> General -> Workflow permissions**).
- Create a fine-grained personal access token with access to this repo only:
  - Contents: read and write
  - Issues: read and write (release-please labels its release PRs)
  - Pull requests: read and write
  - Metadata: read-only (added automatically)
- Save it as the repository secret **`RELEASE_PLEASE_TOKEN`**.
- The token expires. Set a calendar reminder to regenerate it and update the secret before the expiry date.

Without that token, release PRs are opened by the default `GITHUB_TOKEN`. GitHub doesn't run workflows for those PRs, so the required checks never report and branch protection blocks the merge.

## Azure environments

| Environment | Resource group | Notes |
|---|---|---|
| dev | `rg-propertyplatform-dev` | Every merge, automatically. Scales to zero, Burstable Postgres, no Front Door |
| uat | `rg-propertyplatform-uat` | User acceptance testing. Front Door with WAF in Detection mode; approval required to deploy |
| prod | `rg-propertyplatform-prod` | Releases only, with approval. Zone-redundant Postgres HA, geo-redundant backups, GZRS storage, WAF in Prevention mode, 2+ replicas |

### Setup checklist

Ticked items are done. Unticked items are still needed before the deploy stages can run.

**GitHub: repository** (**Settings**)

- [x] Repo is public, with squash merge only, the **Update branch** button enabled, and branches deleted after merge.
- [x] **`main` protection:**
  - A PR is required, with **0 approvals**. This is a solo-developer setup; raise it to 1 when a second developer joins.
  - Required checks: `API (.NET)`, `Web (Next.js)`, `Infrastructure (Bicep)`, `Container image (api|migrations|web)`, `Analyze (csharp|javascript-typescript|actions)` and `Conventional PR title`.
  - The branch must be up to date with `main` before merging.
  - Linear history, all conversations resolved, and no force pushes or deletions.
  - The rules apply to admins too.
- [x] **Actions -> General -> Actions permissions:** "Allow *owner*, and select non-*owner*, actions and reusable workflows".
  - Actions created by GitHub are allowed. Marketplace "verified creators" are **not** allowed automatically.
  - Allow-list:
    ```
    docker/login-action@*, docker/setup-buildx-action@*, docker/build-push-action@*,
    azure/login@*, googleapis/release-please-action@*, amannn/action-semantic-pull-request@*
    ```
  - **Require actions to be pinned to a full-length commit SHA** is on. When you add a new third-party action, add it to this list and pin it to a SHA.
- [x] **Actions -> General -> Workflow permissions:** "Read repository contents and packages permissions" is the default, and "Allow GitHub Actions to create and approve pull requests" is on (release-please needs it).
- [x] **Secrets -> Actions:** `RELEASE_PLEASE_TOKEN` is added and tested (it opened release PR #20, which created v0.1.1).
- [ ] **Code security:** turn on **Secret scanning** and **Push protection**. Both are free for public repos. Dependabot alerts and CodeQL are already active.
- [ ] **Environments:** create `dev`, `uat` and `prod` (**Settings -> Environments**).
  - Variables (each): `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AUTH_AUTHORITY`, `AUTH_AUDIENCE`
  - Secret (each): `POSTGRES_ADMIN_PASSWORD`
  - On **`uat`** and **`prod`**, add **Required reviewers** (yourself), and set **Deployment branches and tags** to `main` only. Leave **Prevent self-review** off while you're the only developer.
- [ ] **Variables -> Actions:** add the repository variable `DEPLOY_ENABLED` = `true`, once Azure is ready.

**Azure: one identity per environment.** No passwords are stored; sign-in uses OIDC.

- [ ] In Microsoft Entra ID, create an app registration per environment, e.g. `gh-propertyplatform-dev`.
- [ ] On each one, add a **federated credential**:
  - Scenario: *GitHub Actions deploying Azure resources*
  - Entity type: *Environment*
  - Environment name: `dev`, `uat` or `prod`, matching the app registration
- [ ] Grant each identity **Owner** on its subscription. Owner is needed because the templates create role assignments. Alternatively, grant Contributor plus Role Based Access Control Administrator.
- [ ] Create the Entra External ID tenant and the API app registration, which provide `AUTH_AUTHORITY` and `AUTH_AUDIENCE`.

**Azure DevOps** (Boards only)

- [ ] Create a project with the **Agile** process.
- [ ] Install the **Azure Boards** GitHub app on this repo. Writing `AB#<id>` in a commit or PR then links it to the work item, and `Fixes AB#<id>` closes the item when the PR merges.

### Manual infrastructure deploy

```bash
POSTGRES_ADMIN_PASSWORD='...' AUTH_AUTHORITY='...' AUTH_AUDIENCE='...' az deployment group create -g rg-propertyplatform-dev -f infra/main.bicep -p infra/env/dev.bicepparam
```

## Known gaps and next steps

- Entra External ID sign-in in Next.js (Auth.js). Then send the access token from `src/web/src/lib/api.ts`.
- Postgres Entra authentication with a managed identity instead of a password. Create a least-privilege `app` role.
- Private endpoints for Key Vault and Storage, and Front Door Private Link to Container Apps.
- An OpenAPI-generated TypeScript client.
- Hangfire worker for scheduled jobs (rent runs, compliance reminders).
- Compliance module: requirements catalogue, compliance items per property and tenancy, expiry dashboard (`GAS-*`, `ELEC-*`, `EPC-*` first).
- Verify the Draft rules in `docs/compliance/` with a lettings compliance adviser.
- Logging redaction driven by `[PersonalData]` (`DP-03`), and subject access export and erasure (`DP-07`, `DP-08`).
- Ledger persistence (append-only journal tables) and endpoints.

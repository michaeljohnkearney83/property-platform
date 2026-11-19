# Property Platform

Multi-tenant SaaS for residential property management: portfolio, landlords, tenants, tenancies, rent and client accounting, compliance, maintenance, documents and reporting.

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core minimal APIs, EF Core 10 + Npgsql, modular monolith |
| Frontend | Next.js 15 (App Router, TypeScript, Tailwind) |
| Database | PostgreSQL 16 with row-level security (RLS) per organisation, pgvector |
| Hosting | Azure (UK South): Container Apps, PostgreSQL Flexible Server, Blob Storage, Key Vault, Front Door + WAF, Application Insights |
| IaC | Bicep (`infra/`) |
| Source control | GitHub |
| Agile + CI/CD | Azure DevOps Boards + Azure Pipelines (`pipelines/`) |
| Identity | Microsoft Entra External ID (customers) / Entra ID (staff SSO) |

## Repository layout

```
src/api/                  .NET solution (PropertyPlatform.sln)
  Host/                   ASP.NET Core entry point, auth, org resolution, health checks
  SharedKernel/           IModule, organisation context, RLS connection interceptor
  Modules/<Name>/         One project per business module (own schema, own endpoints)
  Tests/                  xUnit: ledger rules + architecture rules
  Dockerfile              API image
  Dockerfile.migrations   EF Core migration bundles (run as a Container Apps Job)
src/web/                  Next.js app (staff UI; portals to follow)
infra/main.bicep          One environment = one resource group
infra/modules/            monitoring, network, postgres, keyvault, registry, storage, containerapps, frontdoor
infra/env/*.bicepparam    dev / staging / prod parameters
pipelines/                Azure Pipelines YAML (PR validation + CI/CD)
db/init/                  Local Postgres bootstrap (roles, extensions)
docs/adr/                 Architecture decision records
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

## Adding a module

1. `dotnet new classlib -n PropertyPlatform.Modules.<Name> -o src/api/Modules/<Name> -f net10.0`, then reference `SharedKernel` and add the project to the solution and Host.
2. Implement `IModule` and add it to `Host/ModuleCatalog.cs`.
3. Give it its own `DbContext` with `HasDefaultSchema("<name>")`, an org query filter, an RLS policy in its first migration, and an `IDesignTimeDbContextFactory`.
4. Add a `dotnet ef migrations bundle` line to `Dockerfile.migrations`.

`ArchitectureTests` fails the build if a module references another module or is missing from the catalogue.

## Azure environments

| Environment | Resource group | Notes |
|---|---|---|
| dev | `rg-propertyplatform-dev` | Scale to zero, Burstable Postgres, no Front Door |
| staging | `rg-propertyplatform-staging` | Front Door with WAF in Detection mode |
| prod | `rg-propertyplatform-prod` | Zone-redundant Postgres HA, geo-redundant backups, GZRS storage, WAF in Prevention mode, 2+ replicas |

### One-time Azure DevOps setup

1. **Project:** create it with the **Agile** process. Install the **Azure Boards** and **Azure Pipelines** GitHub apps on the repo. Then mention `AB#<id>` in commits and PRs to link work items.
2. **Service connections:** for each environment, create `sc-propertyplatform-<env>` (Azure Resource Manager, workload identity federation). Grant it **Owner** on its subscription or resource group; Owner is needed because the templates create role assignments.
3. **Variable groups:** for each environment, create `propertyplatform-<env>` containing:
   - `POSTGRES_ADMIN_PASSWORD` (secret)
   - `AUTH_AUTHORITY`
   - `AUTH_AUDIENCE`
4. **Environments:** create `propertyplatform-dev`, `propertyplatform-staging` and `propertyplatform-prod`. Add an **Approvals** check on prod.
5. **Pipelines:** create two pipelines from existing YAML in the GitHub repo:
   - `pipelines/pr-validation.yml`
   - `pipelines/azure-pipelines.yml`
6. **GitHub branch protection** on `main`: require a PR, require the PR-validation check, and require 1 review.

### What a release does (`pipelines/templates/deploy-env.yml`)

1. Creates the resource group. On the first run only, it also deploys the infrastructure with placeholder images so the registry exists.
2. Pushes the images that were built once in the Build stage.
3. Runs EF Core migrations as a Container Apps Job inside the VNet. The database has no public endpoint.
4. Deploys Bicep with the new image tag, which creates new API and web revisions.
5. Smoke-tests `/health/ready` and `/api/health`.

Migrations run before the new code goes live, so they must be **expand/contract**: backwards-compatible with the previous release.

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
- Ledger persistence (append-only journal tables) and endpoints.

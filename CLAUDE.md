# CLAUDE.md

Guidance for AI assistants working in this repository. For setup and commands, read README.md.

## Architecture rules (do not break)

- **Modular monolith.** Each module lives in `src/api/Modules/<Name>`, owns its own Postgres schema, and implements `IModule`. Modules never reference each other; `ArchitectureTests` enforces this. Share contracts through `SharedKernel` only.
- **Terminology.** The SaaS customer is an **Organisation** (`OrgId`). "Tenant" always means a renter. Never name anything `TenantId` for multi-tenancy.
- **Every org-owned entity** implements `IOrgOwned`, has an EF `HasQueryFilter` on `OrgId`, and gets an RLS policy (`ENABLE` + `FORCE ROW LEVEL SECURITY`) in its migration. See `Modules/Portfolio/Migrations/*_InitialPortfolio.cs`.
- **Money** is stored as `long` pence and never as `double`/`decimal` floats in the domain. Ledger journals are immutable: correct them with `JournalEntry.Reverse`. Client money and agency money never move in the same journal.
- **Migrations** must be expand/contract (backwards-compatible with the previous release), because they run before the new API revision.
- **Dev auth** (`DevAuthHandler`) is Development-only. Never weaken the guard in `Program.cs`.
- **Secrets** belong in Key Vault or pipeline secret variables, never in appsettings or bicepparam files.

## Conventions

- The build treats warnings as errors (`src/api/Directory.Build.props`). Fix the warning rather than suppressing it, except in generated migrations.
- Use minimal-API endpoints under `/api/<module>/...`, with DTOs as records.
- Bicep files must be ASCII only. Non-ASCII characters crash `az bicep` on Windows (cp1252).
- When a change affects how to run, configure or deploy the project, update README.md and this file in the same change.
- Record significant decisions as a new ADR in `docs/adr/`.

## CI and deployment

- **All CI/CD is GitHub Actions.** `ci.yml` runs on PRs and is called by `deploy.yml` on pushes to `main`. `deploy.yml` publishes images to GHCR once, then deploys dev -> staging -> prod through `deploy-env.yml` and `scripts/deploy.sh`.
- **Azure DevOps is used for Boards only.** Don't add Azure Pipelines YAML. Link work items with `AB#<id>` in commits and PRs.
- **Azure sign-in** is OIDC per GitHub Environment. Never add Azure credentials or publish profiles as secrets.
- **Branching:** trunk-based. Work on `feature/AB#<id>-...`, `bugfix/...` or `hotfix/...` branches and open PRs into `main`, which is protected: checks must pass, the branch must be up to date, and merges are squash-only. Never push directly to `main`, and never create `dev`, `uat` or `release` branches; environments are pipeline stages, not branches.
- **Commits:** don't add Claude co-author or "Generated with Claude Code" lines.

## Verify before finishing

```bash
dotnet build src/api/PropertyPlatform.sln && dotnet test src/api/PropertyPlatform.sln
```

```bash
cd src/web && npm run lint && npm run typecheck && npm run build
```

```bash
az bicep build -f infra/main.bicep --stdout > /dev/null && az bicep lint -f infra/main.bicep
```

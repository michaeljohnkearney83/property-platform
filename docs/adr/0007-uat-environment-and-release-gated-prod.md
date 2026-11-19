# ADR 0007: UAT environment and release-gated production

- **Status:** Accepted (amends ADRs 0004 and 0006)
- **Date:** 2026-11-19

## Context

Every merge to `main` flowed dev -> staging -> prod, with one approval before prod. In practice:
- the business calls the pre-production environment **UAT**;
- we want a deliberate "dev looks good" step before testers see a build;
- production, which handles client money and compliance data, should only ever run named, versioned releases with release notes.

## Decision

- Rename the `staging` environment to **`uat`** everywhere: GitHub Environment, Bicep `environmentName`, resource group `rg-propertyplatform-uat`, and `ASPNETCORE_ENVIRONMENT=UAT`. Nothing had been deployed, so there was nothing to migrate.
- **dev:** every merge to `main` deploys automatically.
- **uat:** requires approval through the `uat` GitHub Environment's required reviewers.
- **prod:** deploys **only when the push is a release-please release** (`release_created == true`), and then requires approval through the `prod` environment.
- Promotion is by build, not by branch. The images built once per merge are what dev, uat and prod run.

## Consequences

- To ship to production you merge the release PR. Releasing and deploying to prod are now the same deliberate act.
- A hotfix needs a `fix:` PR **and** a patch release, then two approvals. That is a little slower, but every production change is versioned and in the changelog.
- UAT can run builds newer than the last release, so it shows `x.y.z+sha`; prod always shows a clean `x.y.z`.

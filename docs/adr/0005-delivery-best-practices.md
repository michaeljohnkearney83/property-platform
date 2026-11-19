# ADR 0005: Delivery best practices

- **Status:** Accepted
- **Date:** 2026-11-19

## Context

We want a secure, low-maintenance delivery setup for a small team: GitHub for code and CI/CD, and Azure DevOps for Boards only (ADR 0004).

## Decision

| Practice | How it is implemented |
|---|---|
| Trunk-based development | Short-lived branches, PRs into a protected `main`, 1 review, and required CI checks |
| Build once, deploy many | `deploy.yml` builds images once and pushes them to GHCR, tagged with the commit SHA. Each environment imports that exact image |
| No long-lived cloud secrets | GitHub OIDC with federated credentials and one Entra identity per environment |
| Least privilege | `permissions: contents: read` by default; extra permissions are granted per job only |
| Gated production | The `prod` GitHub Environment has required reviewers and only allows deployments from `main` |
| Safe database changes | Migrations run as a job before the new revision and must be expand/contract |
| Infrastructure as code | Bicep only; nothing is created by hand in the portal |
| Supply-chain hygiene | Dependabot for packages and actions, NuGet and npm vulnerability gates in CI, CodeQL code scanning, and secret scanning with push protection |
| Traceability | `AB#<id>` in commits and PRs links code to Azure Boards work items |

## Consequences

- The repo settings in the README must be applied once: environments, reviewers, branch protection, `DEPLOY_ENABLED`, and secret scanning.
- All actions, including first-party `actions/*` and `github/*`, are pinned to full commit SHAs with the version in a comment. The repo setting "Require actions to be pinned to a full-length commit SHA" enforces this, and Actions are restricted to GitHub-authored ones plus an allow-list of third-party ones. Dependabot keeps the SHAs updated.
- Later hardening, not yet done: sign images with build attestations, and add Bicep `what-if` to PRs from branches in this repo.

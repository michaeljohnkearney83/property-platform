# ADR 0004: GitHub Actions for all CI/CD; Azure DevOps for Boards only

- **Status:** Accepted (amends ADR 0001)
- **Date:** 2026-11-19

## Context

The code lives in a public GitHub repo. ADR 0001 planned to use Azure Pipelines for build and deployment. That would have meant two CI systems, duplicated build steps, results split between two UIs, and Azure service connections to manage.

## Decision

- **GitHub Actions** runs everything that touches code:
  - CI on every PR (`ci.yml`).
  - On every push to `main`, `deploy.yml` builds and publishes the images once to GitHub Container Registry, then promotes the same images through dev, staging and prod.
- **Azure sign-in** uses OIDC federated credentials per GitHub Environment, so no Azure secrets are stored. Prod requires reviewer approval through the environment's protection rules.
- **Azure DevOps** is used **only for Boards** (Agile process), linked to GitHub with the Azure Boards app and `AB#<id>` mentions.
- **Dependabot** handles dependency updates.

## Consequences

- One pipeline system and one place to see build and deploy status, next to the code and PRs.
- Build once, deploy many: each environment's ACR imports the exact GHCR image that CI tested.
- Release approvals and history live in GitHub Environments rather than Azure DevOps. That is enough for a small team. If formal release management is needed later, revisit this.
- GitHub becomes critical infrastructure for releasing. If GitHub is down, `scripts/deploy.sh` can be run manually after `az login`.

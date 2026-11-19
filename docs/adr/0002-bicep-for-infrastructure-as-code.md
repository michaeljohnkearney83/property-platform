# ADR 0002: Bicep for infrastructure as code

- **Status:** Accepted
- **Date:** 2026-11-19

## Context

All infrastructure is on Azure. The options were Bicep and Terraform.

## Decision

Use **Bicep**.

- It is Azure-native, so new resource types and API versions are available on day one.
- There is no state file to host, lock, secure or repair.
- It is simpler for a small .NET team, and `what-if` previews can run in GitHub Actions using the same OIDC sign-in as deployments.
- Microsoft supports it, and it costs nothing extra.

## Consequences

- Resources outside Azure (GitHub settings, external DNS, SaaS tools) are not managed by our IaC. If that becomes important, revisit Terraform.
- `what-if` output is noisier than `terraform plan`, so reviewers should read it with care.
- Bicep files must be ASCII-only (the Azure CLI on Windows fails on other characters).

# ADR 0003: Multi-tenancy with a shared schema and Postgres row-level security

- **Status:** Accepted
- **Date:** 2026-11-19

## Context

Many agencies (organisations) share one platform. A data leak between organisations would be a critical incident: it involves client money, personal data and GDPR.

## Decision

- Use a shared database and schema. Every org-owned row carries `OrgId`.
- **Defence in depth:**
  1. EF Core global query filters.
  2. Postgres RLS policies with `FORCE ROW LEVEL SECURITY`, keyed on the session setting `app.org_id`. The setting is applied by `OrgRlsConnectionInterceptor` on every connection open.
- The organisation comes from the `org_id` claim in the access token.
- The app connects as a non-superuser.
- A dedicated database per organisation can be offered later as an enterprise tier.

## Consequences

- Each new table needs an RLS policy in its migration. Reviewers must check for it.
- Cross-organisation reporting for platform staff needs a separate, audited path; RLS stays in force for the app.

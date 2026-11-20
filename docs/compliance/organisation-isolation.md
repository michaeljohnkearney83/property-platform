# Organisation isolation

- **Status:** Draft. Not yet verified against the source.
- **Applies to:** the platform (us, as a SaaS vendor and data processor)
- **Last reviewed:** 2026-11-20

Agencies' data must never be visible to another agency. This is both a contractual promise and a UK GDPR security requirement (Article 32). See [ADR 0003](../adr/0003-multi-tenancy-with-postgres-rls.md).

## Rules

| ID | Rule | When | Blocks? | Source |
|---|---|---|---|---|
| ORG-01 | Every org-owned entity implements `IOrgOwned` and has an EF `HasQueryFilter` on `OrgId`. | Every entity | Build | ADR 0003 |
| ORG-02 | Every org-owned table has `ENABLE` and `FORCE ROW LEVEL SECURITY` and a `CREATE POLICY ... USING (... app.org_id ...)` with a matching `WITH CHECK`, in a migration. | Every table | Build | ADR 0003 |
| ORG-03 | The app connects to Postgres as a non-superuser role (superusers bypass RLS). | Every environment | Deploy | ADR 0003 |
| ORG-04 | Never use `IgnoreQueryFilters()` outside a reviewed, documented platform-admin path. | Always | Review | ADR 0003 |

## How the app enforces it

- `ORG-01`, `ORG-02`: `ComplianceArchitectureTests` reads every module's EF model and migrations and fails the build.
- `ORG-03`: local `db/init/00-roles.sql`; a least-privilege `app` role in Azure is a known gap (README "Known gaps").
- `ORG-04`: PR review, prompted by the PR template.

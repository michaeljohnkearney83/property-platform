# Security and accessibility

- **Status:** Draft. Not yet verified against the source.
- **Applies to:** the platform (us, as a SaaS vendor and data processor)
- **Last reviewed:** 2026-11-20

## Rules

| ID | Rule | When | Blocks? | Source |
|---|---|---|---|---|
| SEC-01 | No secrets in source, appsettings or bicepparam files; use Key Vault or pipeline secrets. | Always | Review; GitHub push protection | Cyber Essentials; CLAUDE.md |
| SEC-02 | Azure sign-in from CI uses OIDC only; no stored Azure credentials. | Always | Review | CLAUDE.md |
| SEC-03 | All GitHub Actions are SHA-pinned and allow-listed; dependencies are kept current by Dependabot. | Always | Repo settings | Supply-chain security |
| SEC-04 | CodeQL findings of high severity are fixed before merge. | Every PR | Required check | Secure development |
| SEC-05 | Public traffic goes through Front Door with WAF (prod). | Prod | Infra | Cyber Essentials (boundary firewall) |
| SEC-06 | Aim for Cyber Essentials before the first agency customer, then Cyber Essentials Plus; ISO 27001 or SOC 2 when larger customers ask. | Business milestone | - | NCSC Cyber Essentials |
| ACC-01 | Tenant and landlord portals meet WCAG 2.2 AA. | Before portals launch | Review | Equality Act 2010; WCAG 2.2 |
| ACC-02 | Every form field has a label, every image has alt text, and the UI works by keyboard alone. | Every UI change | Lint / review | WCAG 2.2 |

## How the app enforces it

- `SEC-01` to `SEC-04`: repo settings, CI and CodeQL (see README "Setup checklist").
- `ACC-*`: Next.js lint for now. Planned: automated accessibility checks (e.g. axe) in CI.

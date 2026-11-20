---
name: compliance
description: Use whenever a change touches properties, tenancies, tenants, landlords, applicants, deposits, rent, money, documents, certificates, notices, personal data, logging of request data, AI features over people's data, or anything in docs/compliance. Loads the UK lettings and UK GDPR rules this platform must follow and the checklist to apply before finishing.
---

# Compliance guardrails

This platform handles client money, tenants' personal data and UK lettings law. Rules live in `docs/compliance/`; read the files relevant to the change **before** writing code.

## 1. Find the rules that apply

| The change touches... | Read |
|---|---|
| Any new entity or table | `docs/compliance/organisation-isolation.md`, `docs/compliance/data-protection.md` |
| People's details, logging, exports, AI over personal data | `docs/compliance/data-protection.md` |
| Gas, electrics, EPC, alarms, licences, legionella | the matching file in `docs/compliance/` |
| Tenancy creation, notices, rent changes | `deposits-and-fees.md`, `right-to-rent.md`, `renters-rights-act.md` |
| Ledger, deposits, payments | `client-money.md`, `deposits-and-fees.md` |
| UI for tenants or landlords | `security-and-accessibility.md` |

## 2. Hard rules (never break; tests enforce the first three)

- **ORG-01 / ORG-02:** every `IOrgOwned` entity has a `HasQueryFilter` on `OrgId`, and its table gets `ENABLE` + `FORCE ROW LEVEL SECURITY` and a `CREATE POLICY ... USING ... WITH CHECK` in the migration. Copy the pattern in `src/api/Modules/Portfolio/Migrations/*_InitialPortfolio.cs`.
- **DP-02:** every persisted property holding personal data has `[PersonalData(PersonalDataCategory.X)]` (namespace `PropertyPlatform.SharedKernel.Compliance`). If a name only looks personal, use `[NotPersonalData("reason")]`.
- **CM-01 / CM-02:** client money and agency money never share a journal; journals are immutable, corrected with `JournalEntry.Reverse`.
- **DP-03:** never log, trace or put in exception messages any `[PersonalData]` value. Log IDs.
- **No invented law.** Only implement rules that exist in `docs/compliance/` with an ID. If a requirement isn't there, stop and say so; propose a new rule entry with a source link for a person to verify. Never guess legal periods, amounts or dates.
- **Rules are data.** Legal periods, caps and effective dates come from the requirements catalogue or a named constant tied to a rule ID, never an unexplained literal.
- **Draft rules don't make claims.** UI must not say "compliant" based on a rule whose status is Draft.

## 3. Checklist before finishing

- [ ] Named the rule IDs this change implements or affects (put them in the PR description and in test names).
- [ ] New entities: query filter, RLS migration, `[PersonalData]` annotations. `ComplianceArchitectureTests` passes.
- [ ] Tests cover the rule's boundary (e.g. day 30 vs day 31, 5 vs 6 weeks' rent at GBP 50,000).
- [ ] No personal data in logs, errors or AI prompts.
- [ ] If a rule's meaning or interpretation changed: updated its `docs/compliance/` file (new ID if the meaning changed) and, if significant, added an ADR.
- [ ] Filled in the Compliance section of the PR template.

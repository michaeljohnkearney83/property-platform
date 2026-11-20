# Data protection (UK GDPR)

- **Status:** Draft. Not yet verified against the source.
- **Applies to:** the platform (us, as a SaaS vendor and data processor)
- **Last reviewed:** 2026-11-20

Agencies are the **controllers** of their landlords', tenants' and applicants' personal data. We are their **processor**, and the controller for our own account and billing data.

## Rules

| ID | Rule | When | Blocks? | Source |
|---|---|---|---|---|
| DP-01 | Personal data is only reachable inside its organisation (see `ORG-*`). | Always | Build | UK GDPR Art. 5(1)(f), 32 |
| DP-02 | Every persisted property holding personal data is marked `[PersonalData(category)]`, or `[NotPersonalData("reason")]` if its name only looks like it. | Every entity | Build | UK GDPR Art. 30 (records of processing) |
| DP-03 | Never write `[PersonalData]` values to logs, traces, metrics, exception messages or AI prompts sent outside UK/EU processing agreements. Log IDs instead. | Always | Review | UK GDPR Art. 5(1)(c), 32 |
| DP-04 | Data is stored and processed in UK South. New sub-processors (e.g. an AI API) need a DPA and are listed below before use. | Always | Review | UK GDPR Ch. V, Art. 28 |
| DP-05 | `SpecialCategory` data (health, ethnicity, etc.) needs a DPIA and an ADR before it is stored. | Before storing | Review | UK GDPR Art. 9, 35 |
| DP-06 | Every entity with personal data has a retention period in the schedule below before it ships. | Before release | Review | UK GDPR Art. 5(1)(e) |
| DP-07 | Subject access: the platform can export all `[PersonalData]` for a person within one month of the controller's request. | On request | - | UK GDPR Art. 15 |
| DP-08 | Erasure: the platform can erase or anonymise a person's `[PersonalData]` where the controller asks and no legal retention applies. Ledger journals are immutable, so ledger entries reference a party ID, never personal data. | On request | - | UK GDPR Art. 17 |
| DP-09 | AI features that make or support decisions about people (e.g. tenant referencing) need a DPIA and human review of the outcome. | Before release | Review | UK GDPR Art. 22, 35 |

## Retention schedule

| Data | Retention | Basis | Status |
|---|---|---|---|
| Property address (`Property.AddressLine1`, `Postcode`) | While managed + TBC | Agency's legitimate interest; contract | To be confirmed |
| Ledger and financial records | TBC (HMRC commonly 6 years) | Legal obligation | To be confirmed |
| Right to Rent check copies | Tenancy + 1 year (Home Office guidance) | Legal obligation | To be confirmed |
| Gas safety records | At least 2 years | Gas Safety Regs 1998 reg. 36 | To be confirmed |

## Sub-processors

| Sub-processor | Purpose | Region | DPA |
|---|---|---|---|
| Microsoft Azure | Hosting, database, storage, monitoring | UK South | Microsoft Products and Services DPA |

## How the app enforces it

- `DP-02`: `ComplianceArchitectureTests.Persisted_personal_data_is_annotated` checks every persisted property whose name looks like personal data (name, email, phone, date of birth, NI number, passport, share code, bank details, address, postcode).
- `DP-03`: PR review for now. Planned: a logging redaction policy driven by `[PersonalData]`.
- `DP-07`, `DP-08`: not implemented yet. The `[PersonalData]` annotations are what the export and erasure jobs will read.

## Notes for implementers

- A let property's address is the tenant's home, so it is personal data (`PersonalDataCategory.Address`).
- DTOs and API responses don't need annotations, but never log them whole.
- Don't send personal data to an AI model unless the provider is listed above as a sub-processor and the feature has passed `DP-09` where relevant.

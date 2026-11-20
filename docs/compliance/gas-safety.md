# Gas safety

- **Status:** Draft. Not yet verified against the source by a person or a lettings compliance adviser.
- **Jurisdiction:** England
- **Last reviewed:** 2026-11-20

## Rules

| ID | Rule | When / how often | Blocks? | Source |
|---|---|---|---|---|
| GAS-01 | Every gas appliance, fitting and flue provided by the landlord has a safety check by a Gas Safe registered engineer. | At least every 12 months | New tenancy start if missing or expired | [Gas Safety (Installation and Use) Regulations 1998, reg. 36](https://www.legislation.gov.uk/uksi/1998/2451/regulation/36) |
| GAS-02 | Give a copy of the record to existing tenants. | Within 28 days of the check | - | reg. 36 |
| GAS-03 | Give a copy to new tenants. | Before they move in | New tenancy start | reg. 36 |
| GAS-04 | Keep each record. | At least 2 years | - | reg. 36 |

## How the app enforces it

Not implemented yet. Planned for the Compliance module: a `GasSafetyCheck` compliance item per property with gas, its expiry date (issue date + 12 months), reminders at 60/30/7 days, and tracking of when the record was given to each tenant.

## Notes for implementers

- A property without gas has no `GAS-*` obligations. Record "no gas supply" explicitly; don't infer it from a missing certificate.
- A check done up to 2 months before the deadline can keep the original anniversary date (reg. 36(3)). Model this as a rule, not a hard-coded 12 months.

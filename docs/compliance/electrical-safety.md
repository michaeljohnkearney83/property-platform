# Electrical safety (EICR)

- **Status:** Draft. Not yet verified against the source by a person or a lettings compliance adviser.
- **Jurisdiction:** England
- **Last reviewed:** 2026-11-20

## Rules

| ID | Rule | When / how often | Blocks? | Source |
|---|---|---|---|---|
| ELEC-01 | The electrical installation is inspected and tested by a qualified person. | At least every 5 years, or sooner if the report says so | New tenancy start if missing or expired | [Electrical Safety Standards in the Private Rented Sector (England) Regulations 2020](https://www.legislation.gov.uk/uksi/2020/312/contents) |
| ELEC-02 | Give a copy of the report to existing tenants. | Within 28 days of the inspection | - | 2020 Regs, reg. 3 |
| ELEC-03 | Give a copy to new tenants. | Before they move in | New tenancy start | 2020 Regs, reg. 3 |
| ELEC-04 | Give a copy to the local housing authority on request. | Within 7 days of the request | - | 2020 Regs, reg. 3 |
| ELEC-05 | Complete remedial or further investigative work the report requires, and confirm it in writing to the tenant and the local authority. | Within 28 days, or the shorter period in the report | - | 2020 Regs, reg. 3 |

## How the app enforces it

Not implemented yet. Planned for the Compliance module: an `Eicr` compliance item with next-inspection date taken from the report (not always 5 years), and a remedial-work task with its own deadline.

## Notes for implementers

- A report graded "unsatisfactory" (C1, C2 or FI codes) creates remedial work; it is not simply a valid certificate.
- Reading the next-inspection date and observation codes from an uploaded PDF is a good AI extraction feature, with a person confirming the values.

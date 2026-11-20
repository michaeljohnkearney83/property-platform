# Tenancy deposits and fees

- **Status:** Draft. Not yet verified against the source by a person or a lettings compliance adviser.
- **Jurisdiction:** England
- **Last reviewed:** 2026-11-20

## Rules

| ID | Rule | When / how often | Blocks? | Source |
|---|---|---|---|---|
| DEP-01 | Protect the deposit in a government-authorised scheme. | Within 30 days of receiving it | Flag on day 31 | [Housing Act 2004, ss. 212-215](https://www.legislation.gov.uk/ukpga/2004/34/part/6/chapter/4) |
| DEP-02 | Give the tenant (and any relevant person) the prescribed information. | Within 30 days of receiving the deposit | Flag on day 31 | Housing Act 2004 s. 213; Housing (Tenancy Deposits) (Prescribed Information) Order 2007 |
| DEP-03 | Cap the deposit at 5 weeks' rent (annual rent under GBP 50,000) or 6 weeks' rent (GBP 50,000 or more). | When taken | Tenancy creation | [Tenant Fees Act 2019](https://www.legislation.gov.uk/ukpga/2019/4/contents) |
| DEP-04 | Cap a holding deposit at 1 week's rent and handle its return within the Act's deadlines. | When taken | - | Tenant Fees Act 2019, Sch. 2 |
| DEP-05 | Only charge permitted payments (rent, deposits, defaults, agreed changes, early termination, utilities/council tax/TV licence/communications where applicable). | Always | Charge creation | Tenant Fees Act 2019, Sch. 1 |

## How the app enforces it

Not implemented yet. Planned for the Compliance module: deposit-received date on the tenancy, a 30-day countdown for `DEP-01` and `DEP-02`, validation in Tenancies for `DEP-03`, and an allow-list of charge types in the Ledger for `DEP-05`.

## Notes for implementers

- Deposit money is **client money** (see [client-money.md](client-money.md)). It never moves in the same journal as agency money.
- Store amounts in pence (`long`), and compute the cap from the weekly rent equivalent of the agreed rent. Write tests for monthly-to-weekly conversion; it is a common source of error.

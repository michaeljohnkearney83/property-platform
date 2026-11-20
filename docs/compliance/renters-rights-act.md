# Renters' Rights Act 2025

- **Status:** Draft. Not yet verified against the source by a person or a lettings compliance adviser.
- **Jurisdiction:** England
- **Last reviewed:** 2026-11-20

The Act reforms private renting in England in phases. **This file needs verifying against the Act, its commencement regulations and current government guidance before any rule is used.** Phase dates below are our understanding and are the most likely things to be wrong.

## Rules

| ID | Rule | When | Blocks? | Source |
|---|---|---|---|---|
| RRA-01 | New assured tenancies are periodic; fixed terms and assured shorthold tenancies are replaced. Existing ASTs convert. | From the main commencement date (our understanding: 1 May 2026) | Tenancy creation (no fixed-term ASTs after commencement) | Renters' Rights Act 2025 |
| RRA-02 | Section 21 "no-fault" possession is abolished; possession uses Section 8 grounds only. | From the main commencement date | Notice generation | Renters' Rights Act 2025 |
| RRA-03 | Rent increases only once a year through the statutory (section 13) process; no rent review clauses. | From the main commencement date | Rent change | Renters' Rights Act 2025 |
| RRA-04 | No rental bidding: don't ask for or accept more than the advertised rent; limits on rent in advance. | From the main commencement date | Tenancy creation | Renters' Rights Act 2025 |
| RRA-05 | No blanket bans on tenants with children or receiving benefits. | From the main commencement date | Listing / applicant screening | Renters' Rights Act 2025 |
| RRA-06 | Tenants can request to keep a pet; landlords must not unreasonably refuse and must respond within the set period. | From the main commencement date | - | Renters' Rights Act 2025 |
| RRA-07 | Give tenants the required government information (replacing the How to Rent guide for new tenancies) by the set deadline. | Per commencement regulations | New tenancy start | Renters' Rights Act 2025 and guidance |
| RRA-08 | Landlords register on the PRS Database and join the landlord Ombudsman once those are live. | Later phases (dates TBC) | Marketing, once live | Renters' Rights Act 2025 |
| RRA-09 | Decent Homes Standard and Awaab's Law (hazard response deadlines) extend to the private rented sector. | Later phases (dates TBC) | - | Renters' Rights Act 2025 |

## How the app enforces it

Not implemented yet. This Act touches Tenancies (tenancy types, notices, rent changes), Parties (applicants, pets) and Maintenance (hazard deadlines). Each rule needs verifying first and an ADR for the tenancy-model change (`RRA-01`).

## Notes for implementers

- Model tenancy type with effective dates: tenancies created before commencement are ASTs that convert; don't rewrite their history.
- Never generate a Section 21 notice for a tenancy in England after `RRA-02` takes effect.

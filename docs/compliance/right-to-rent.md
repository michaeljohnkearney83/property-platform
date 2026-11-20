# Right to Rent

- **Status:** Draft. Not yet verified against the source by a person or a lettings compliance adviser.
- **Jurisdiction:** England
- **Last reviewed:** 2026-11-20

## Rules

| ID | Rule | When / how often | Blocks? | Source |
|---|---|---|---|---|
| RTR-01 | Check that every adult who will live in the property as their main home has the right to rent. | Within 28 days before the tenancy starts | New tenancy start | [Immigration Act 2014, Part 3 Ch. 1](https://www.legislation.gov.uk/ukpga/2014/22/part/3/chapter/1) |
| RTR-02 | For time-limited permission, do a follow-up check. | Before permission expires (or 12 months after the first check, whichever is later) | - | Immigration Act 2014; Home Office code of practice |
| RTR-03 | Keep copies of the evidence. | For the tenancy and 1 year after it ends | - | Home Office code of practice |
| RTR-04 | Check all adults the same way, regardless of apparent nationality. | Always | - | Home Office code of practice on avoiding discrimination |

## How the app enforces it

Not implemented yet. Planned for the Compliance module: a Right to Rent check per adult occupier with result, method (document, share code, IDVT), date and follow-up date.

## Notes for implementers

- Share codes, document numbers and copies are `[PersonalData(PersonalDataCategory.Identity)]`. Store copies in Blob Storage via the Documents module, not in the database.
- Retention (`RTR-03`) is a legal requirement that overrides an erasure request until it expires (`DP-08`).

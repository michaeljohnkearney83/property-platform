# HMO and property licensing

- **Status:** Draft. Not yet verified against the source by a person or a lettings compliance adviser.
- **Jurisdiction:** England
- **Last reviewed:** 2026-11-20

## Rules

| ID | Rule | When / how often | Blocks? | Source |
|---|---|---|---|---|
| LIC-01 | A large HMO (5 or more people forming 2 or more households) needs a mandatory HMO licence. | Before letting; licences usually last up to 5 years | New tenancy start if unlicensed | [Housing Act 2004, Part 2](https://www.legislation.gov.uk/ukpga/2004/34/part/2) |
| LIC-02 | Smaller HMOs may need an additional licence, and any rented property may need a selective licence, depending on the council's schemes. | Varies by council | New tenancy start if required and missing | Housing Act 2004, Parts 2 and 3 |
| LIC-03 | Comply with licence conditions (e.g. maximum occupiers, minimum room sizes). | Throughout | Tenancy creation over the occupancy limit | Licence conditions |

## How the app enforces it

Not implemented yet. Planned for the Compliance module: a `PropertyLicence` item with type, council, licence number, expiry and maximum occupiers.

## Notes for implementers

- Whether a licence is needed depends on the council and the date. Don't try to compute it; let the agency record that a licence is required, and track its expiry and conditions.
- `PropertyType.Hmo` exists in Portfolio; an HMO without a licence record should show as "licence status unknown", not "compliant".

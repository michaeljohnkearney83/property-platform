# Energy performance (EPC and minimum standards)

- **Status:** Draft. Not yet verified against the source by a person or a lettings compliance adviser.
- **Jurisdiction:** England
- **Last reviewed:** 2026-11-20

## Rules

| ID | Rule | When / how often | Blocks? | Source |
|---|---|---|---|---|
| EPC-01 | A valid EPC is available for the property and given to prospective tenants. | Before marketing / letting; an EPC is valid 10 years | New tenancy start if missing | [Energy Performance of Buildings (England and Wales) Regulations 2012](https://www.legislation.gov.uk/uksi/2012/3118/contents) |
| EPC-02 | The property meets the minimum energy efficiency standard (currently band E) unless a valid exemption is registered on the PRS Exemptions Register. | New and existing tenancies | New tenancy start if below E without exemption | [Energy Efficiency (Private Rented Property) (England and Wales) Regulations 2015](https://www.legislation.gov.uk/uksi/2015/962/contents) |

## How the app enforces it

Not implemented yet. Planned for the Compliance module: an `Epc` compliance item with rating and expiry, and an `MeesExemption` item with its own expiry.

## Notes for implementers

- The government has consulted on raising the minimum to band C. Store the minimum band as catalogue data with effective dates (`EPC-02`), never as a constant.
- EPC data can be looked up from the public EPC register by address; consider an import rather than manual entry.

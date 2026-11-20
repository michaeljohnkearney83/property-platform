# Client money

- **Status:** Draft. Not yet verified against the source.
- **Applies to:** the platform (us, as a SaaS vendor and data processor)
- **Last reviewed:** 2026-11-20

The platform records agencies' client money (rent and deposits held for landlords and tenants). It doesn't hold money itself.

## Rules

| ID | Rule | When | Blocks? | Source |
|---|---|---|---|---|
| CM-01 | Client money and agency money never move in the same journal; client balances are reported separately from agency income. | Always | Build (ledger tests) | Agency CMP scheme rules; [Client Money Protection Schemes for Property Agents (Requirements) Regulations 2019](https://www.legislation.gov.uk/uksi/2019/386/contents) |
| CM-02 | Journals are immutable; corrections are reversals (`JournalEntry.Reverse`). | Always | Build (ledger tests) | Audit trail; CMP scheme rules |
| CM-03 | Money is stored as `long` pence. No floating-point money. | Always | Review | Accuracy |
| CM-04 | The platform supports client account reconciliation (ledger vs bank) for each client account. | Before the ledger ships | - | CMP scheme rules |
| CM-05 | If the platform ever moves money itself (collecting rent, paying landlords), stop and write an ADR first: that brings in FCA payment services rules and a regulated payment provider. | Before building it | Review | Payment Services Regulations 2017 |

## How the app enforces it

- `CM-01`, `CM-02`: `LedgerTests`.
- `CM-04`: not implemented yet.

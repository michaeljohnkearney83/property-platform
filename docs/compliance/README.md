# Compliance knowledge base

This folder is the single source of truth for the legal and regulatory rules the platform implements. It is written for people **and** AI coding assistants: Claude Code loads it through the `compliance` skill (`.claude/skills/compliance/`), and `CLAUDE.md` points here.

> **Not legal advice.** Every rule starts as **Draft**. A rule only becomes **Verified** after a person has checked it against the source (and, for lettings rules, with a lettings compliance adviser). Code may implement Draft rules, but the UI must not tell users they are compliant on the strength of a Draft rule.

## Scope

- **Jurisdiction:** England first. Wales (Renting Homes (Wales) Act 2016) and Scotland have different regimes and are out of scope until we add them as separate rule files.
- **Two kinds of rule:**
  - **Lettings rules:** what the product tracks and enforces for agencies (gas safety, deposits, Right to Rent...).
  - **Platform rules:** what we must do as a SaaS vendor (UK GDPR, security, client money, accessibility).

## Rule files

| Area | File | Rule IDs |
|---|---|---|
| Organisation isolation | [organisation-isolation.md](organisation-isolation.md) | `ORG-*` |
| Data protection (UK GDPR) | [data-protection.md](data-protection.md) | `DP-*` |
| Gas safety | [gas-safety.md](gas-safety.md) | `GAS-*` |
| Electrical safety (EICR) | [electrical-safety.md](electrical-safety.md) | `ELEC-*` |
| Energy performance (EPC, MEES) | [energy-performance.md](energy-performance.md) | `EPC-*` |
| Smoke and carbon monoxide alarms | [smoke-and-co-alarms.md](smoke-and-co-alarms.md) | `ALARM-*` |
| Tenancy deposits and fees | [deposits-and-fees.md](deposits-and-fees.md) | `DEP-*` |
| Right to Rent | [right-to-rent.md](right-to-rent.md) | `RTR-*` |
| HMO and property licensing | [licensing.md](licensing.md) | `LIC-*` |
| Legionella | [legionella.md](legionella.md) | `LEG-*` |
| Renters' Rights Act 2025 | [renters-rights-act.md](renters-rights-act.md) | `RRA-*` |
| Client money | [client-money.md](client-money.md) | `CM-*` |
| Security and accessibility | [security-and-accessibility.md](security-and-accessibility.md) | `SEC-*`, `ACC-*` |

## How rules are written

Each file has the same shape:

1. A header with **Status** (Draft / Verified), **Jurisdiction**, **Last reviewed**.
2. A **Rules** table: `ID | Rule | When / how often | Blocks? | Source`.
3. **How the app enforces it:** which module, which tests (tests name the rule ID), or "Not implemented yet".
4. **Notes for implementers:** edge cases and interpretations.

Rule IDs are permanent. Never reuse or renumber one; mark it **Withdrawn** with a date instead.

## How rules reach the code

- **Rules are data, not scattered `if`s.** Recurring requirements (type, applies to, frequency, effective dates, source, blocks tenancy start) go in the Compliance module's requirements catalogue when it is built. Code reads the catalogue; it doesn't hard-code legal periods.
- **Effective dates.** Law changes. A rule has `effective from` and, when replaced, `effective to`. Never edit an old rule's meaning in place: add a new rule ID.
- **Tests name the rule.** A test that enforces `GAS-01` has `GAS01` or `GAS-01` in its name or message, so a rule can be traced to code with a search.
- **Architecture tests** (`src/api/Tests/ComplianceArchitectureTests.cs`) enforce the cross-cutting platform rules on every build: `ORG-01`, `ORG-02` and `DP-02`.

## Changing a rule

1. Edit the rule file (new ID if the meaning changes) and cite the source.
2. Update the code and tests that reference the ID.
3. Tick the compliance section of the PR template.
4. For a significant interpretation, add an ADR in `docs/adr/`.

# ADR 0008: Compliance guardrails for AI-assisted development

- **Status:** Accepted
- **Date:** 2026-11-20

## Context

The platform handles client money, tenants' personal data and UK lettings law, and much of its code will be written with AI assistants. AI-generated code tends to work while silently ignoring rules it was never told about: it may skip row-level security on a new table, log personal data, or hard-code a legal period it guessed. Legal rules also change (the Renters' Rights Act phases in from 2026), so rules scattered through the code would be hard to find and update.

## Decision

- Write the rules down in `docs/compliance/`, one file per area, each rule with a permanent ID, source link, effective dates and a Draft/Verified status. This is the single source of truth for people and AI tools.
- Point AI assistants at it through `CLAUDE.md` and a Claude Code skill (`.claude/skills/compliance/`) that maps a kind of change to the rule files and a finishing checklist. The core instruction: never invent law; only implement rules that exist in the knowledge base.
- Treat legal periods, caps and dates as data (a requirements catalogue with effective dates in the Compliance module), not literals.
- Enforce the cross-cutting rules with tests that read the real EF models and migrations: `ComplianceArchitectureTests` checks org query filters and RLS (`ORG-01`, `ORG-02`) and `[PersonalData]` annotations (`DP-02`).
- Add a PR template with a compliance checklist.

## Consequences

- New entities need a `[PersonalData]` decision for any field whose name looks personal; false positives need `[NotPersonalData("reason")]`.
- The `[PersonalData]` annotations become the input for log redaction, subject access export and erasure, which still need building.
- The rule files are Draft until verified by a person, ideally a lettings compliance adviser. Until then the UI must not claim compliance.
- Instructions guide AI tools but don't bind them; the tests and PR review are the actual enforcement.

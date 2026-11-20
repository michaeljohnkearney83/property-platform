<!-- PR title must be a Conventional Commit, e.g. "feat: track gas safety certificates". It becomes the squash commit and drives the version. -->

## What and why

<!-- One or two sentences. Link work items with AB#<id>. -->

## Compliance

<!-- See docs/compliance/README.md. Tick what applies; delete nothing. -->

- [ ] **None of the below apply** (no personal data, money, legal rules, org-owned data or tenant-facing UI)
- [ ] **New or changed org-owned table:** query filter + RLS migration (`ORG-01`, `ORG-02`)
- [ ] **Personal data:** new fields annotated with `[PersonalData]`; nothing personal is logged (`DP-02`, `DP-03`)
- [ ] **New sub-processor or AI over personal data:** listed in `data-protection.md`; DPIA considered (`DP-04`, `DP-09`)
- [ ] **Money:** client and agency money kept apart; amounts in pence (`CM-*`)
- [ ] **Legal rule implemented or changed.** Rule IDs: <!-- e.g. GAS-01, DEP-01 -->
  - [ ] The rule exists in `docs/compliance/` with a source link
  - [ ] Tests cover the boundary and name the rule ID
- [ ] **Uses `IgnoreQueryFilters()`:** justified below (`ORG-04`)
- [ ] **Tenant- or landlord-facing UI:** keyboard and labels checked (`ACC-*`)

## How it was tested

<!-- Commands run, screenshots for UI. -->

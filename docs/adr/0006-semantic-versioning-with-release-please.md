# ADR 0006: Semantic versioning with release-please

- **Status:** Accepted
- **Date:** 2026-11-19

## Context

We deploy every merge to `main`, but we also need human-readable versions: for customers, for support ("which version are you on?"), for release notes, and for rolling back to a known release.

## Decision

- Use Semantic Versioning for the whole product. There is one version for the API, web app and images, because they ship together.
- The single source of the version is `version.txt` plus `.release-please-manifest.json`. release-please also bumps `<Version>` in `Directory.Build.props` and `package.json`.
- PR titles must be Conventional Commits, enforced by the `Conventional PR title` check. Squash merges turn each title into one commit on `main`.
- release-please keeps a release PR open. Merging it creates the tag, the GitHub Release and the `CHANGELOG.md` entry, and adds version tags to the GHCR images.
- Every running build reports its version and commit SHA: the API's `/version`, the OpenAPI `info.version`, and the web app footer.

## Consequences

- Releasing is a deliberate act (merging the release PR), while deployment stays continuous.
- Contributors must write good PR titles, because the changelog is generated from them.
- release-please needs a fine-grained PAT (`RELEASE_PLEASE_TOKEN`) so that CI runs on its PRs.

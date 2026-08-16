---
name: platform-wiki-governance
description: Maintain the PTN Assurance Platform canonical Wiki Brain when API Contract Checker work changes current behavior, architecture, security, package or migration truth, decisions, roadmap, verification evidence, or external research sources. Use at task completion for durable checker knowledge changes; never create a parallel wiki.
---

# Platform wiki governance

## Read

Open the repository root [Wiki Brain router](../../../../../docs/wiki-brain/00-Home.md).
Use its task routing and authority order: working code/package evidence, `01-Current`,
`02-Rules`, accepted `03-Decisions`, then architecture/operations; `90-Inbox` is scope
input rather than canonical truth. Read only the pages relevant to the task and verify
claims against code, migration, package content, tests, and registry evidence.

## Record

- Update the relevant `01-Current` page for verified behavior.
- Add or supersede an ADR only when a durable decision changed; include owner, date,
  consequences, and links required by the Wiki Brain governance.
- Update Roadmap and the first remaining step without turning an Inbox plan into truth.
- Record exact package versions and exact test/build/scanner evidence.
- Add new external research sources to the existing source registry when applicable.

## Preserve boundaries

- Keep `docs/wiki-brain` as the single canonical knowledge tree; do not create
  `docs/PTN-ASSURANCE-PLATFORM-WIKI.md` or another parallel wiki.
- Preserve the existing Current, Rules, Decisions, Architecture, Operations, Inbox, and
  Templates ownership model defined by `00-Home.md` and ADR-0001.
- Keep package README files only when the NuGet/public consumer contract requires them.
- Never record private remote URLs, tokens, unseal keys, passwords, connection strings,
  real Vault paths, or customer data.
- A proposal stays a proposal until accepted; dry-run evidence is not a public release.

## Verify

Run the repository's Wiki Brain validation and local Markdown-link checks. Compare any
source claims with code/package/registry evidence. Documentation checks do not replace
backend build, tests, package validation, consumer smoke, or `backend-verify`.

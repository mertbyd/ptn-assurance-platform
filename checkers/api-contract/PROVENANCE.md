# Provenance — CheckNexus API Contract Checker

This file records where this repository's history came from and what is
provable about it. It exists because the module tree was previously not under
version control, while `CheckNexus.ApiContracts*` packages had already been
published from it.

**Rule this file serves:** history is never manufactured. No commit date is
backdated, no development order is invented, and no published package is
associated with a commit that cannot be shown to have produced it.

## Import path taken: Phase 1 (real history import)

The upstream worktree was located and readable, so the real history was
transferred. Phase 1-B (single snapshot commit) was **not** needed.

| Fact | Value |
|---|---|
| Upstream worktree | `C:\Users\mertb\RiderProjects\ptn-api-contract-checker` |
| Upstream remote | `https://pitea.piton.com.tr/PITON/ptn-contract-checker-api.git` |
| Upstream HEAD branch | `replay/master` |
| Upstream HEAD commit | `fac76895e0d9a335519e2732077ef771befd8b3d` |
| Upstream HEAD subject | `#KBP-86 created: rebuilt the postman verification suite against the real api surface` |
| Upstream HEAD date | 2026-08-07 |
| Commits on HEAD | 52 |
| Distinct commits imported | 55 |
| Branches imported | 27 |
| Import date | 2026-08-12 |

Transfer method — no history rewriting of any kind:

```
git remote add upstream <upstream worktree>
git fetch --no-tags upstream "+refs/heads/*:refs/remotes/upstream/*"
git branch --no-track <real-name> upstream/<real-name>      # for each branch
```

Every imported commit keeps its original SHA, author, and author/commit dates.
Because the SHAs are unchanged, any imported commit can be re-verified against
the upstream worktree or the `pitea` remote.

## Imported branches

Branch names are verbatim upstream names; none were renamed or collapsed.

| Branch | HEAD SHA | Commits | Last commit | Subject |
|---|---|---|---|---|
| `KBP-0015` | `e254602394c59d83e93affdad73a951b7155ba60` | 53 | 2026-08-09 | #KBP-0015 feat: connected selected context login authorization |
| `KBP-61` | `438296028b15a78dd5e21f320a03a309f9a090b5` | 2 | 2026-08-06 | #KBP-61 feat: created the ABP solution skeleton with layers and test bases |
| `KBP-62` | `0df1b64b98c113403fbb87ede548dac3721ce1c6` | 4 | 2026-08-06 | #KBP-62 feat: created the local infrastructure and vault tooling |
| `KBP-63` | `96719aae2954a7db9e5fb749ff08165d07989934` | 6 | 2026-08-06 | #KBP-63 feat: established tenant aware identity and session flows |
| `KBP-64` | `a66e9f0f2f093a2adccad3095ec7f549a0eadb6f` | 8 | 2026-08-06 | #KBP-64 feat: established permissions settings localization and tenant seeding |
| `KBP-65` | `2642d2f35bfb061545290bb4058a5f9fc25a0dbd` | 10 | 2026-08-07 | #KBP-65 feat: established reusable lookup and repository foundation |
| `KBP-66` | `237ca97c45ae1e07378cca76e126f48a840fec79` | 12 | 2026-08-07 | #KBP-66 feat: seeded lookup families and generated initial schema |
| `KBP-67` | `6a38d86896966d0870eb243ab62b4852ec2fd3be` | 14 | 2026-08-07 | #KBP-67 feat: created manager-owned source aggregate behavior |
| `KBP-68` | `110a858225622f3556b16aec16c2f500cd37658b` | 16 | 2026-08-07 | #KBP-68 feat: secured source credentials and reachability checks |
| `KBP-69` | `fcfd6d53f7b96b2aa5ecb1cd882a562e36729081` | 18 | 2026-08-07 | #KBP-69 feat: created guarded resilient specification fetching |
| `KBP-70` | `ffd1f78f9e38fa95270cd5ac5b081fadc5171881` | 20 | 2026-08-07 | #KBP-70 feat: pinned openapi toolchain and created format reading |
| `KBP-71` | `16a1e859e15e1d00352ce8b81d2ad30285356281` | 22 | 2026-08-07 | #KBP-71 feat: persisted manager-owned deduplicated snapshots |
| `KBP-72` | `a9d56d257593fb9e4ea371b25c7d0f026fdaf731` | 24 | 2026-08-07 | #KBP-72 feat: exposed snapshot history through target-complete mappings |
| `KBP-73` | `3671758955e46ce10d12025cffdf003a23d7efe0` | 26 | 2026-08-07 | #KBP-73 feat: defined directional difference vocabulary |
| `KBP-74` | `0dae9c734873425fcb5a6d5a0a2c401189a2c6c2` | 28 | 2026-08-07 | #KBP-74 feat: compared OpenAPI operations against governed lookups |
| `KBP-75` | `eba22fbb686b57aec8e31dbc753abf938d26a0b2` | 30 | 2026-08-07 | #KBP-75 feat: compared schema and response contracts |
| `KBP-76` | `b8e9f4fb5739649c3680dc77faa802c877fd9102` | 32 | 2026-08-07 | #KBP-76 feat: classified scoped contract differences |
| `KBP-77` | `4c55d55c08d1f25ecc61677f122d24a29b92f39a` | 34 | 2026-08-07 | #KBP-77 feat: persisted manager-owned contract check runs |
| `KBP-78` | `36a755d245fcab1b4e85365cd92a4fb4683d6f69` | 36 | 2026-08-07 | #KBP-78 feat: queued manager-owned asynchronous contract checks |
| `KBP-79` | `e6dcd3ab38fdfb3b84b8d2cbef2fae211a2e1d5d` | 38 | 2026-08-07 | #KBP-79 feat: persisted manager-owned notification recipients |
| `KBP-80` | `67b487c76a40bc0450116cb705f5d631bd823f4d` | 40 | 2026-08-07 | #KBP-80 feat: queued tenant-aware contract report emails |
| `KBP-81` | `2e108c34625fecb86abfa4759ed4cf06d6539e3f` | 42 | 2026-08-07 | #KBP-81 feat: provisioned operators and tenant members |
| `KBP-82` | `89c13eedcb08ee19c3327566e35fbc4bcd89fc12` | 44 | 2026-08-07 | #KBP-82 feat: published manager-owned live contract check updates |
| `KBP-83` | `8bef022138fd552708e69e7b8edcc93b8c6aeb52` | 46 | 2026-08-07 | #KBP-83 feat: scheduled monitored contract checks |
| `KBP-84` | `e668729b121e161df2b909853e2f955fc8ff9068` | 48 | 2026-08-07 | #KBP-84 feat: enforced notification delivery preferences |
| `KBP-85` | `84691726d44681d245c024920e62d6af5d13804b` | 50 | 2026-08-07 | #KBP-85 fix: completed runtime wiring and verification |
| `replay/master` | `fac76895e0d9a335519e2732077ef771befd8b3d` | 52 | 2026-08-07 | #KBP-86 created: rebuilt the postman verification suite against the real api surface |

### Not imported, deliberately

- **`refs/codex/turn-diffs/checkpoints/**`** — 22 broken refs in the upstream
  worktree; Git itself reports them as unreadable (`ignoring broken ref`). They
  are editor checkpoint state, not development history.
- **Tag `backup/KBP-623`** (`d31eb20a9020d2e18527a0bcc17daffabeeee8ce`,
  2026-08-06, `#KBP-623 refactor: moved entity behaviour into validators
  configurations and managers`) — a real commit, but **not an ancestor of
  `replay/master`** and, as measured below, not the parent of this workspace
  tree. Importing it would imply a lineage that does not exist. It remains
  retrievable from the upstream worktree.
- Upstream tags generally (`--no-tags`), so that this repository's tag namespace
  carries only release claims that this file can justify.

## Workspace reconciliation

The workspace tree and the upstream HEAD tree are not identical. This module is
a repackaged subset of the upstream application: the application shell
(`docs/`, `postman/`, `vault/`, compose files, solution metadata) was dropped,
and the `CheckNexus.ApiContracts` package projects plus a large body of
untracked platform-era work (KBP-601…623 in the wiki numbering) were added.

That difference is recorded as **one separate commit** whose parent is the real
upstream HEAD. It is not blended into the imported commits, and nothing is
presented as though it had always been there.

Delta of that commit, measured against `fac7689` (629 paths):

| Change | Files |
|---|---|
| Added | 202 |
| Deleted | 334 |
| Modified | 92 |
| Renamed | 1 |

### Nearest-ancestor evidence

The parent was not assumed. The staged workspace tree
(`0f36a94d470bc26b201ac60353859790ce9a3149`) was diffed against every plausible
upstream candidate, counting files under `src/`, `test/` and `host/` that are
byte-identical to the workspace (653 workspace files in those paths):

| Candidate | Identical files | Differing paths |
|---|---|---|
| **`replay/master` (chosen)** | **373** | 500 |
| `KBP-85` | 373 | 500 |
| `KBP-84` | 365 | 505 |
| `KBP-83` | 361 | 495 |
| `KBP-82` | 344 | 512 |
| `KBP-81` | 339 | 488 |
| `backup/KBP-623` | 336 | 533 |

`replay/master` and `KBP-85` tie on identical content; `replay/master` was
chosen because it is the upstream HEAD and a descendant of `KBP-85` (`KBP-86`
touches only `postman/`). The divergent `backup/KBP-623` line matches the
workspace *less* well, which is why it is not treated as the parent.

### Files restored from upstream during reconciliation

`.gitignore` and `.gitattributes` were absent from the extracted module folder
and were restored from upstream HEAD unchanged. They are inherited repository
hygiene, not authored content. Without them the commit would have tracked build
output (`bin/`, `obj/`, 26 directories) and IDE state (`.idea/`). Both paths,
plus `artifacts/`, are ignored by the inherited rules — so the local `artifacts/`
package evidence is intentionally untracked build output.

## Published package provenance

Sixteen `0.2.0-alpha.2` packages were published to NuGet.org from this tree
before it was under version control. Every one of them was inspected.

**Result: 16/16 `.nuspec` files carry a repository URL and no `commit`
attribute.**

```xml
<repository type="git" url="https://pitea.piton.com.tr/PITON/ptn-contract-checker-api.git" />
```

No published package points at a wrong or non-existent commit, so there is
nothing to retract. This was not luck: `common.props` disables SourceLink unless
the build is a CI build.

```xml
<PtnSourceRepositoryMetadataAvailable Condition="'$(CI)' == 'true'">true</PtnSourceRepositoryMetadataAvailable>
<EnableSourceLink Condition="'$(PtnSourceRepositoryMetadataAvailable)' != 'true'">false</EnableSourceLink>
```

The published repository URL is the same upstream repository whose history this
file imports, so the imported lineage is consistent with what was published.

> **Correction to LEDGER-0001.** The ledger's "Bilinen borç" note states that the
> packages carry SourceLink metadata "fakat işaret edilen commit mevcut
> değildir" (but the pointed-to commit does not exist). No commit is pointed to
> at all. The debt was narrower than recorded.

## Version tags

See `Tags` below for what is tagged and what is deliberately left untagged; a
version is tagged only when this tree can be shown to have produced it.

| Version | Tagged | Basis |
|---|---|---|
| `0.1.0-alpha.5` | No | Tree that produced it is not identifiable in this repository |
| `0.2.0-alpha.1` | No | Superseded working tree; no byte-level proof available |
| `0.2.0-alpha.2` | See `git tag -l` | Tagged only if package-content verification passed; the tag message records the evidence |

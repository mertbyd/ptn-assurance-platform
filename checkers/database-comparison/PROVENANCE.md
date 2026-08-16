# Provenance — CheckNexus Database Comparison Checker

This file records where this repository's history came from and what is
provable about it. It exists because the module tree was previously not under
version control at all (no `.git`), while `CheckNexus.DatabaseComparison*`
packages had already been published from it.

**Rule this file serves:** history is never manufactured. No commit date is
backdated, no development order is invented, and no published package is
associated with a commit that cannot be shown to have produced it.

## Import path taken: Phase 1 (real history import)

The upstream worktree was located and readable, so the real history was
transferred. Phase 1-B (single snapshot commit) was **not** needed.

| Fact | Value |
|---|---|
| Upstream worktree | `C:\Users\mertb\Documents\Codex\2026-07-06\bi\ptn-database-comparison-api` |
| Upstream remote | `https://pitea.piton.com.tr/PITON/ptn-database-checker-api.git` |
| Upstream checked-out branch | `fix/KBP-54-report-detailseparator-prefix` (`fad549f`, 89 commits) |
| Most advanced real tip | `pitea/predev` = `c136f670559ec232e3ea89bd4b2b30671fd2abe9` |
| Tip subject | `Merge pull request '#KBP-54' (#24) from fix/KBP-54-report-detailseparator-prefix into predev` |
| Tip date | 2026-07-27 |
| Commits on tip | 90 |
| Distinct commits imported | 94 |
| Local branches imported | 16 |
| Remote-tracking refs retained | 17 |
| Import date | 2026-08-12 |

The upstream worktree is owned by a different Windows account
(`Mert/CodexSandboxOffline`), so Git refuses to read it under the current user
without an ownership exception. It was read with a **per-command** override
(`git -c safe.directory=<path> ...`) rather than a persistent global config
change, so nothing outside this operation was altered.

Transfer method — no history rewriting of any kind:

```
git init -b master
git remote add upstream <upstream worktree>
git -c safe.directory=<upstream> fetch --no-tags upstream "+refs/heads/*:refs/remotes/upstream/*"
git -c safe.directory=<upstream> fetch --no-tags upstream "+refs/remotes/pitea/*:refs/remotes/pitea/*"
git branch --no-track <real-name> upstream/<real-name>      # for each branch
```

Every imported commit keeps its original SHA, author, and author/commit dates.

## Imported branches

Branch names are verbatim upstream names, including the `fix/...` branches;
none were renamed or collapsed.

| Branch | HEAD SHA | Commits | Last commit | Subject |
|---|---|---|---|---|
| `KBP-43` | `705b873b6be5a13e15d372e0983481c1434ac350` | 2 | 2026-07-09 | #KBP-43 |
| `KBP-44` | `f577a1c311b7ccc46c2d03b9b52cd43be4e7132b` | 3 | 2026-07-09 | #KBP-44 |
| `KBP-45` | `16b109b05be5ed4da0f5b0e1cc3a62e4e132e275` | 5 | 2026-07-09 | #KBP-45 |
| `KBP-46` | `16bff971ca9637b563df81ff423b7d394d37bb80` | 11 | 2026-07-17 | #KBP-46 |
| `KBP-47` | `bf470031e94bed7285411d7f91365ff39fd30bba` | 17 | 2026-07-16 | #KBP-47 |
| `KBP-48` | `4767fef1bfdcaaccf5b49f31e6eb66282aacfbfb` | 19 | 2026-07-17 | #KBP-48 |
| `KBP-49` | `adfbb9f6e298d2edd20479d3dad6de2493357b1b` | 34 | 2026-07-17 | #KBP-49 |
| `KBP-50` | `9740b6c66ecf66ff98756850b53240dc2074f044` | 36 | 2026-07-16 | #KBP-50 |
| `KBP-51` | `6998de9958cfed2550eb5611639040dbb8249468` | 47 | 2026-07-20 | #KBP-51 |
| `KBP-52` | `692d27f034bd7b5df1e9517bc4f10b908e01d205` | 60 | 2026-07-23 | #KBP-52 |
| `KBP-54` | `dbdb0a18330767628294c16b7406c1faa3bdc6da` | 76 | 2026-07-24 | #KBP-54 feat: expose migration schemas in history and reports |
| `KBP-55` | `b365ab87cb5d44ae293ecb80248a8653a679ed11` | 80 | 2026-07-27 | #KBP-55 |
| `fix/KBP-52-index-comparison-matching` | `cdeb1b8201320804d64386ece71e713565086bbf` | 84 | 2026-07-27 | #KBP-52 |
| `fix/KBP-52-nuget-package-sources` | `4c64553174a2ffb6a6140fa3b4f52a9b2e76b0dd` | 74 | 2026-07-23 | #KBP-52 |
| `fix/KBP-54-report-detailseparator-prefix` | `fad549fda6f61452b20bea7af9d82e90b5e7d1af` | 89 | 2026-07-27 | #KBP-54 |
| `predev` | `418924014039ee8aa5edb7a47b1e6ba084a8b43a` | 75 | 2026-07-23 | Merge pull request 'fix/KBP-52-nuget-package-sources' (#20) from fix/KBP-52-nuget-package-sources into predev |

There is no `KBP-53`; the upstream numbering skips it. It was not invented here.

### Remote-tracking refs retained

The upstream worktree's `refs/remotes/pitea/*` carry commits that exist on **no
local branch** — notably `pitea/predev` (`c136f67`), `pitea/KBP-52`
(`e0c2185`) and `pitea/development` (`9f71f8b`). They were copied across as
remote-tracking refs so that real history is not lost, and deliberately **not**
converted into local branches, because upstream did not have them as local
branches either.

### Not imported, deliberately

- **`refs/codex/turn-diffs/checkpoints/**`** — 4 broken refs in the upstream
  worktree that Git reports as unreadable. Editor checkpoint state, not history.
- **Upstream tags** `FINAL`, `backup-kbp49-tip-20260712-131102`,
  `backup-kbp49-worktree-20260712-131102`, `bak-kbp46-oldmodel`,
  `bak-kbp47-oldmodel`, `bak-kbp48-oldmodel` — backup and working markers, not
  release claims. Fetched with `--no-tags` so this repository's tag namespace
  carries only release claims that this file can justify.

## Workspace reconciliation

The workspace tree and the upstream tip tree are not identical. This module is a
repackaged subset of the upstream application: the application shell (`vault/`,
`secrets/`, compose files, solution metadata) was dropped, and the
`CheckNexus.DatabaseComparison` package projects plus a large body of untracked
platform-era work (KBP-701…706 in the wiki numbering — cross-engine type map,
safety profile, catalog depth, assertion surface, diagnosis engine, fingerprint
and paged finding reads) were added.

That difference is recorded as **one separate commit** whose parent is the real
upstream tip. It is not blended into the imported commits.

Delta of that commit, measured against `c136f67` (599 paths):

| Change | Files |
|---|---|
| Added | 190 |
| Deleted | 229 |
| Modified | 175 |
| Renamed | 5 |

### Nearest-ancestor evidence

The parent was not assumed. The staged workspace tree
(`391941d3b6ca0821edaea3d4b8bbd88c89bef69b`) was diffed against every plausible
upstream candidate, counting files under `src/`, `test/` and `host/` that are
byte-identical to the workspace (662 workspace files in those paths):

| Candidate | Identical files | Differing paths |
|---|---|---|
| **`pitea/predev` (chosen)** | **300** | 572 |
| `fix/KBP-54-report-detailseparator-prefix` | 300 | 572 |
| `KBP-54` | 289 | 561 |
| `predev` | 284 | 564 |
| `KBP-55` | 284 | 583 |
| `KBP-52` | 284 | 564 |
| `pitea/KBP-52` | 283 | 565 |
| `pitea/development` | 227 | 574 |

`pitea/predev` and `fix/KBP-54-report-detailseparator-prefix` tie on identical
content. `pitea/predev` was chosen because it is the integrated mainline and
`fix/KBP-54-report-detailseparator-prefix` is a verified ancestor of it
(`git merge-base --is-ancestor` → 0), so it is the later of two equally close
candidates.

### Files restored from upstream during reconciliation

`.gitignore` and `.gitattributes` were absent from the extracted module folder
and were restored from the upstream tip unchanged. They are inherited repository
hygiene, not authored content. Without them the commit would have tracked build
output and IDE state. `bin/`, `obj/`, `.idea/` and `artifacts/` are ignored by
the inherited rules — so the local `artifacts/` package evidence is
intentionally untracked build output.

## Published package provenance

Eight `CheckNexus.DatabaseComparison*` packages at `0.2.0-alpha.2` were
published to NuGet.org from this tree before it was under version control. All
were inspected, together with the eight API-side packages.

**Result: 16/16 `.nuspec` files carry a repository URL and no `commit`
attribute.**

```xml
<repository type="git" url="https://pitea.piton.com.tr/PITON/ptn-database-checker-api.git" />
```

No published package points at a wrong or non-existent commit, so there is
nothing to retract. This was not luck: `common.props` disables SourceLink unless
the build is a CI build, so a non-CI local pack emits no commit claim.

The published repository URL is the same upstream repository whose history this
file imports, so the imported lineage is consistent with what was published.

> **Correction to LEDGER-0001.** The ledger's "Bilinen borç" note states that the
> packages carry SourceLink metadata "fakat işaret edilen commit mevcut
> değildir" (but the pointed-to commit does not exist). No commit is pointed to
> at all. The debt was narrower than recorded.

## Version tags

A version is tagged only when this tree can be shown to have produced it.

| Version | Tagged | Basis |
|---|---|---|
| `0.1.0-alpha.5` | No | Tree that produced it is not identifiable in this repository |
| `0.2.0-alpha.1` | No | Superseded working tree; no byte-level proof available |
| `0.2.0-alpha.2` | See `git tag -l` | Tagged only if package-content verification passed; the tag message records the evidence |

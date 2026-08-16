# AJAN DEVİR KİTİ — sıra, paralellik kuralı ve hazır promptlar

Hazırlandı: 2026-08-15 · KBP-100 kapandıktan sonra. Güncellendi: 2026-08-16.

Aşağıdaki blokları **olduğu gibi** ajana yapıştır. Her prompt kendi kendine yeterlidir;
ajan hiçbir bağlam miras almaz.

---

## 0. Sıra ve neden

| # | Task | Nerede çalışır | Git yazar mı | Ne zaman |
|---|---|---|---|---|
| **1** | **KBP-99** — Test Module borçları | ana checkout | ✅ evet | **hemen** |
| **2** | **KBP-97 kalanı** — wiki senkron | `docs/` wiki checkout | ✅ yalnız wiki deposu | **1 ile aynı anda** |
| **3** | **KBP-98** — Vault paketleme | ana checkout | ✅ evet | **1 bittikten sonra** |
| **4** | Canlı altyapı smoke | — | — | **önce senin kararın** (§5) |

### Paralellik kuralı — tek kural, ihlal etme

**Aynı repository index'inde aynı anda yalnız BİR ajan `git add`/`git commit` çalıştırabilir.**
İki ajan aynı checkout'ta commit atarsa birbirinin yarım dosyalarını stage'ler. Kök kaynak
deposu ile `docs/` wiki deposu ayrı index kullandığından bu kural onları birbirine kilitlemez.

> [!NOTE] Bu paragrafın eski paralellik gerekçesi geçersizleşti
> `docs/` kök kaynak deposunda hâlâ ignored'dır; ancak 2026-08-15'ten beri kendi `.git`
> deposuna sahiptir. Wiki commit'leri yalnız `docs/` deposunda yapılır. Kök kaynak index'iyle
> çakışmaz, fakat artık “hiç git komutu çalıştırmaz” denemez. Güncel sınır §6'dadır.

Bu yüzden kaynak işiyle wiki işi ayrı repository index'lerinde yürüyebilir. **3'ü 1 bitmeden başlatma.**

---

## 1. KBP-99 — Test Module borçları *(ilk sırada)*

`HarArtifactContainer` marker sınıfının kaldırılması bu görevdedir.

```
You are implementing ticket KBP-99 in an ABP Framework (.NET 10) repository. Work autonomously to completion.

MANDATORY FIRST STEP — standards chain. You inherit NO standards lock. Before your first edit:

1. Run:
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\mertb\.claude\skills\backend-standards-router\scripts\resolve-standards-route.ps1" -RepositoryPath "C:\Users\mertb\RiderProjects\ptn-assurance-platform" -ConcernPath "C:\Users\mertb\RiderProjects\ptn-assurance-platform\ptn-test-module\src" -TaskText "KBP-99 remove blob marker container, move host package versions to common props, scope the bridge naming drift test"

2. Read COMPLETELY, in returned order, every file in RequiredSkills then RequiredReferences. Do not skim.
3. Emit a Standards Context Lock checkpoint before your first edit.
4. Refresh it when you cross layers and before each commit.

THE TASK — read this file completely and follow it exactly. It is the specification:
C:\Users\mertb\RiderProjects\ptn-assurance-platform\docs\wiki-brain\90-Inbox\TASK-KBP-99-Test-Module-Borclari.md

It is in Turkish: §1 write-gate (which skill, then which live sibling file, per file type), §2 frozen decisions, §3 three slices with a file manifest, §4 cut zone, §5 ten prohibitions, §6 acceptance criteria, §7 finish procedure.

Also read as governing context:
- C:\Users\mertb\RiderProjects\ptn-assurance-platform\AGENTS.md
- C:\Users\mertb\RiderProjects\ptn-assurance-platform\ptn-test-module\AGENTS.md

STARTING STATE — verified 2026-08-15:
- Branch KBP-100 is green: 3 commits (ab2a9a5, deaf29d, 8c86b53), build 0 errors, 150 tests pass, 0 fail.
- Create your branch from it: git checkout KBP-100 && git checkout -b KBP-99
- Do NOT commit to KBP-95 or KBP-100. No force-push, no rebase, no amend of commits you did not create.
- ptn-test-module/AGENTS.md and README.md have pre-existing uncommitted edits that are NOT yours. Leave them alone; do not stage them.
- vault/ has unrelated uncommitted work. Do not touch vault/ at all.

HARD CONSTRAINTS:
- Commit subject grammar: #KBP-99 <type>: <past-tense English>. Use "created" not "added". Three commits, one per slice, named exactly as §3 specifies.
- Pass the message with a POSIX heredoc or git commit -F -. Do NOT use PowerShell here-string syntax (@'...'@) with the Bash tool — it leaks a literal @ into the subject. Verify each with: git log -1 --format='[%s]' — the output must start with [#KBP-99 and have no character before the #.
- Every slice closes green: dotnet build Ptn.TestModule.slnx -m:1 → 0 errors. Do not start the next slice after a failed gate.
- Run build/test from C:\Users\mertb\RiderProjects\ptn-assurance-platform\ptn-test-module with timeout >= 600000 ms and -m:1. On a file-lock error run dotnet build-server shutdown ONCE, never in a loop.
- Broken tests get FIXED, never deleted or Skip-ed.
- Before declaring done run: powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\mertb\.claude\skills\backend-verify\scripts\check-backend-diff.ps1"

KNOWN SCANNER FALSE POSITIVE — do not "fix" it: the scanner emits [ENTITY] findings for Ensure*/Normalize* methods in Domain/Managers/Catalog/TestScenarioManager.cs. That file is a Manager (public class TestScenarioManager : FoundationManager<TestScenario, Guid>) and those methods belong exactly where they are. This was verified on 2026-08-15. Report the count, do not refactor them.

REPORT BACK, separately and factually: files per slice; the three commit SHAs and subjects; build result with exact error/warning counts; test result with totals; the before/after resolved Serilog versions (prove with dotnet list package, do not assume); the drift test's old and new scope; every test you broke and how you fixed it; anything you could NOT verify with the exact blocker; every assumption. Never report "verified" for a gate that did not run or failed.
```

---

## 2. KBP-97 kalanı — wiki senkron *(1 ile aynı anda başlatılabilir)*

```
You are finishing ticket KBP-97 — synchronizing the Obsidian wiki with the actual state of the code. A previous agent completed part of this and was cut off. Work autonomously to completion.

THIS IS A DOCUMENTATION-ONLY TASK. You will not write one line of C#. Do not run dotnet build or dotnet test.

CRITICAL — TWO SEPARATE GIT REPOSITORIES. As of 2026-08-15 the wiki vault is its OWN git repository, nested inside the source repository's working directory:

- Parent source repo: C:\Users\mertb\RiderProjects\ptn-assurance-platform  (tracks C# only; its .gitignore line 25 is "docs/")
- Wiki vault repo:    C:\Users\mertb\RiderProjects\ptn-assurance-platform\docs   (branch main, tracks the whole vault)

Because the parent ignores docs/, the two never see each other. Rules:
- Work in and commit to the WIKI repo only. Every git command you run must have C:\...\ptn-assurance-platform\docs as its working directory. Verify before your first commit: git rev-parse --show-toplevel must print the docs path, NOT the parent path.
- Do NOT run any git command against the parent repository. Another agent may be committing C# there; you would collide with it.
- Do NOT create a branch in the wiki repo — commit directly to main. There is no branch workflow here.
- Edit files IN PLACE at C:\Users\mertb\RiderProjects\ptn-assurance-platform\docs\wiki-brain\ . Touch nothing outside docs\.
- Commit subject grammar here is plain, NOT the #KBP form: "docs: <past-tense English description>". Use "created" not "added". Group into coherent commits.
- A pre-repo backup also exists at ...\ptn-assurance-platform\tmp\wiki-backup-20260815-012036\wiki-brain\ (93 .md files, 2026-08-15 01:20), from before the repo existed.

ALREADY DONE by the previous agent — do NOT redo these six, but DO read them so your work is consistent with them:
01-Current/Platform-Truth.md, 01-Current/Checker-Packages-Truth.md, 05-Operations/Package-Release-Ledger.md, 04-Architecture/System-Context.md, 03-Decisions/ADR-0020-*.md, 03-Decisions/ADR-0018-*.md (a new section I was added narrowing the SchemaName prohibition to the location and report types).

YOUR SPECIFICATION: docs/wiki-brain/90-Inbox/PLAN-0005-Paralel-Is-Kumesi.md §2 lists 14 numbered corrections with exact page names and exact defects. Work every item not already covered by the six pages above.

Also read, because they govern how this wiki may change:
- docs/wiki-brain/00-Home.md
- docs/wiki-brain/03-Decisions/ADR-0001-Wiki-Brain-Governance.md
- docs/wiki-brain/05-Operations/Research-Index.md §5
- docs/wiki-brain/02-Rules/RULE-0008-*.md

ADDITIONAL ITEMS, all verified against source on 2026-08-15 — record each:

A. KBP-100 is COMPLETE. Branch KBP-100, three commits: ab2a9a5 (arazzo validation + x-checknexus-db compiler), deaf29d (publication evidence derived from the machine compiler), 8c86b53 (test coverage). Build 0 errors; 150 tests pass, 0 fail. It closes TM-05 and the machine side of TM-17, and it moved the five publication-gate evidence fields (CompiledDocument, CompiledHash, AssertionCount, IsSchemaValid, AreAssertionsDerivable, plus SourceDescriptionSpecSnapshotIds) from client-supplied to server-derived. PublishTestScenarioDto, its validator and TestScenarioPublishModel were deleted as a consequence.

B. KBP-95's headline acceptance criterion is NOT met — record it as NOT met, do not check it off. PLAN-0003 Blok 1's criterion and the Roadmap's "T1 dikey dilimi" item both require "a hand-written Arazzo scenario runs end to end GREEN with zero model calls". TestRunEndToEndTests.cs does not prove it: it enters at OracleDispatchService.JudgeAsync with a hardcoded HAR fixture, the runner never executes, oracles are NSubstitute mocks, and the fixture is red by construction (ExitCode = 1; the second test asserts OutcomeCode == Failed). The "zero model calls" half IS proven, structurally, by the third test reflecting over the constructor. Record why it is genuinely hard: a real green run needs Docker + redocly/cli, a live SUT and live checker endpoints — an infrastructure smoke test, not a unit test. Recommend it as a named follow-up.

C. Same gap for KBP-100: redocly/cli:2.14.0 was never actually executed (Docker not invoked, IArazzoDocumentLinter stubbed in tests), and neither checker's derivability surface was called for real. Record the pattern honestly: this module has strong unit coverage and zero live-infrastructure coverage.

D. Record the repository topology, on whichever page carries repository/architecture facts. Agents keep guessing wrong about this. The facts: the parent source repo tracks C# only; docs/, scripts/, AGENTS.md, CLAUDE.md and checkers/ are all in its .gitignore. checkers/ are two independent repositories with their own upstream history and release tags. docs/ became its own repository on 2026-08-15 (see item H). Each has its own commit grammar — #KBP-<no> in the source repo, plain "docs:" in the wiki repo.

E. Correct PLAN-0005's own defects: §1's conflict matrix lists KBP-97's repo as "docs/wiki-brain" and §2 prescribes a commit subject — both written as though the vault were tracked. Also §1 and §5 state "KBP-95 depends on KBP-100"; that inverted in practice, KBP-95 shipped first against hand-written Arazzo documents exactly as TASK-KBP-95 §0 permitted. Record what actually happened.

F. Ticket numbers KBP-94 and KBP-96 were never used (git log --all --grep returns zero for both). KBP-94's scope landed inside KBP-93; KBP-96's scope (TM-08 oracle dispatcher + TM-09 job) landed as slices 2 and 3 of KBP-95. TASK-KBP-93 §9 still assigns TM-08/TM-09 to KBP-96. Record this absorption explicitly and non-deletably — the numbering drift has already cost real time.

G. Three task documents now exist and belong in the task inventory/counters: TASK-KBP-100-Arazzo-Derleyicisi.md, TASK-KBP-99-Test-Module-Borclari.md, HANDOFF-Ajan-Devir-Sirasi.md.

H. The "should the wiki be version-controlled?" question is DECIDED and already implemented — record it, do not re-open it. On 2026-08-15 the vault became its own git repository at ...\ptn-assurance-platform\docs (branch main, first commit 839566a, 107 files). The parent source repo still ignores docs/, so the ".gitignore" intent is intact: the wiki stays out of the shipped product and out of code review, but now has full history and undo. Record this on the same page as item D, and if Inbox.md carries the open question, move it to the answered/closed section.

UNCHANGED RULES:
- Never edit a RESEARCH-* page; contradictions go in the index (Research-Index §5).
- Never silently rewrite an ADR's decision. A changed decision needs a NEW ADR. If an item starts to feel like reversing a decision rather than correcting a record, STOP and report it.
- Historical references to the deleted ADR-0011 stay.
- Refresh updated: to 2026-08-15 on every page you touch; keep id/decision_refs/rule_refs consistent.
- Verify claims against actual source (git log, real .csproj, real files). Code wins over Current pages.
- If a PLAN-0005 §2 item is already correct, or is wrong about what the code does, say so instead of making a pointless edit.

REPORT BACK: per-item outcome for the 14 plus A-H; pages edited with their new updated values; what you could not verify; any genuine decision conflict that needs a human. Explicitly confirm you issued zero git commands.
```

---

## 3. KBP-98 — Vault paketleme *(KBP-99 bittikten SONRA)*

> **Ağaçta yarım ve kısmen YANLIŞ iş var.** Ajanın ilk işi onu düzeltmek; devam ettirmek değil.
> Ayrıntı promptta.

```
You are implementing ticket KBP-98 — hardening the CheckNexus.Vault package release wiring. Work autonomously to completion.

MANDATORY FIRST STEP — standards chain. You inherit NO standards lock. Before your first edit:

1. Run:
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\mertb\.claude\skills\backend-standards-router\scripts\resolve-standards-route.ps1" -RepositoryPath "C:\Users\mertb\RiderProjects\ptn-assurance-platform" -ConcernPath "C:\Users\mertb\RiderProjects\ptn-assurance-platform\vault" -TaskText "KBP-98 vault package release hardening, sourcelink guard, packagevalidation baseline, version advance"

2. Read COMPLETELY every file in RequiredSkills then RequiredReferences. This is NuGet package-family work, so also load the nuget-family-release skill and follow its publish-authorization and immutable-version gates.
3. Emit a Standards Context Lock checkpoint before your first edit.

YOUR SPECIFICATION: docs/wiki-brain/90-Inbox/PLAN-0005-Paralel-Is-Kumesi.md §3 — four numbered items. Read it completely. Also read docs/wiki-brain/05-Operations/NuGet-Package-Release-Playbook.md (GUIDE-0003) and docs/wiki-brain/05-Operations/Package-Release-Ledger.md.

STARTING STATE — there is uncommitted work in vault/ that is PARTLY WRONG. Audit it before continuing; do not assume it is a correct starting point. What I verified on 2026-08-15:

Already applied in the working tree (uncommitted):
- vault/src/CheckNexus.Vault/CheckNexus.Vault.csproj: Version 0.1.0-alpha.5 -> 0.2.0-alpha.2; added RepositoryType/RepositoryUrl/PublishRepositoryUrl; added EnablePackageValidation=true, ContinuousIntegrationBuild, IncludeSymbols, SymbolPackageFormat=snupkg; added Microsoft.SourceLink.GitHub 10.0.203.
- vault/release-manifest.json (new, untracked): version 0.2.0-alpha.2, immutableVersions: [].
- vault/PACKAGE-README.md: one line changed.

Defects in that work, against PLAN-0005 §3:
1. SourceLink is added but UNGUARDED. §3.1 requires the checker common.props protection clause (PtnSourceRepositoryMetadataAvailable / EnableSourceControlManagerQueries / EnableSourceLink / EmbedUntrackedSources). Without it a non-CI dotnet pack stamps repository+commit metadata that may point at a commit that exists nowhere. LEDGER-0001 records this clause as the reason the 16 published checker packages do not have that defect.
2. EnablePackageValidation=true with NO PackageValidationBaselineVersion — contract breaks are not actually being looked for. §3.2.
3. Version was set to 0.2.0-alpha.2, but per PLAN-0005 §3.3 that version is ALREADY PUBLISHED and therefore immutable. Setting it again means a different payload under a published version — precisely what §3's prohibition forbids. The version must ADVANCE to the next prerelease, and 0.2.0-alpha.2 must be recorded in release-manifest.json's immutableVersions (which is currently the empty array). Follow the DB checker manifest's pattern.
4. §3.4's audit is not done: inspect the PUBLISHED 0.2.0-alpha.2 nupkg's .nuspec for a commit attribute. If it has one and that commit does not exist on origin, record it as a finding in LEDGER-0001. Do NOT unpublish or re-push — a published version is immutable.

PROHIBITIONS:
- NEVER re-push a published version with different content.
- Do NOT bind vault to the checker common.props — it is a separate version line. Copy the PATTERN only.
- Do NOT publish to nuget.org in this task. Pack, validate and inspect locally. Publishing requires explicit human authorization per the nuget-family-release skill; if you believe a push is warranted, STOP and ask.
- Do not touch ptn-test-module/, checkers/ or docs/.
- ptn-test-module/AGENTS.md and README.md have unrelated uncommitted edits. Do not stage them.

ACCEPTANCE (PLAN-0005 §3): a non-CI dotnet pack stamps NO repository/commit metadata; the baseline property is consistent between csproj and manifest; dotnet build has 0 warnings; 10/10 Vault tests pass.

COMMITS: subject grammar #KBP-98 <type>: <past-tense English>, "created" not "added". Pass the message with a POSIX heredoc or git commit -F - — do NOT use PowerShell here-string syntax (@'...'@) with the Bash tool, it leaks a literal @ into the subject. Verify each with git log -1 --format='[%s]'.

REPORT BACK: what you found wrong in the pre-existing uncommitted work and how you corrected it; the version you chose and why; proof that a non-CI pack stamps no commit metadata (show the actual .nuspec); the .nuspec audit result for the published 0.2.0-alpha.2; build and test results with totals; commit SHAs and subjects; everything you could not verify with the exact blocker; every assumption.
```

---

## 4. Sırada ne yok — bilerek

Bu kitte **canlı altyapı smoke testi** yok, çünkü önce senin bir kararın gerekiyor. Bkz. §5.

---

## 5. Sıradaki büyük iş — canlı altyapı smoke

Modülün **unit kapsaması güçlü, canlı altyapı kapsaması sıfır**. İki kabul kriteri aynı
sebeple kanıtsız:

- KBP-95: *"elle yazılmış Arazzo senaryosu uçtan uca **yeşil** koşuyor"* — runner hiç koşmadı
- KBP-100: `redocly/cli:2.14.0` **hiç çalıştırılmadı**, linter testlerde stub

### Ortam envanteri — Docker Desktop, 2026-08-15 doğrulandı

| Gereken | Durum |
|---|---|
| Docker engine | ✅ çalışıyor (26 GB kullanımda, 1 TB limit) |
| PostgreSQL | ✅ `postgres:17`, `17-alpine`, `15` |
| HashiCorp Vault | ✅ `hashicorp/vault:2.0.3`, `1.18` |
| Redis | ✅ `redis:7-alpine` |
| SMTP (bildirim akışı) | ✅ `axllent/mailpit:v1.30.0`, `mailhog` |
| **`redocly/cli:2.14.0`** | ❌ **YOK — çekilmemiş.** Runner ve lint kapısının ikisi de buna bağlı |
| İki checker host'u ayakta mı | ❓ bilinmiyor |
| Hedef SUT | ❓ bilinmiyor |

**İlk adım tek komut:** `docker pull redocly/cli:2.14.0`. Kod tarafı zaten pinli imajı ve
salt-okunur mount'u bekliyor; eksik olan yalnız imajın kendisi.

Kalan iki soru işareti cevaplanınca smoke task'ı yazılır. Kademeli öneri: **(1)** yalnız lint
turu — sadece Docker + imaj yeter, KBP-100'ün `redocly lint` yolunu gerçek konteynerle kanıtlar;
**(2)** derleme + koşum turu — SUT ve checker host'ları gerekir; **(3)** tam altı-an turu.

Adım (1) bugün yapılabilir ve KBP-100'ün en büyük kanıt boşluğunu kapatır.

## 6. Kapanmış kararlar — yeniden açma

**Wiki versiyonlaması — 2026-08-15'te çözüldü.** Vault artık kendi git deposu:
`...\ptn-assurance-platform\docs`, branch `main`, ilk commit `839566a`, 107 dosya. Ana kaynak
deposu `docs/`'u yok saymaya devam ediyor, yani `.gitignore` niyeti korundu: wiki üründe ve kod
incelemesinde değil, ama artık tam geçmişi ve geri alması var. `tmp/wiki-backup-20260815-012036`
depo öncesi yedektir; saklanabilir ama artık zorunlu değil.

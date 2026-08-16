---
id: ADR-0024
type: decision
status: accepted
title: Urun deposunun siniri ve wiki yayin yeri
created: 2026-08-16
updated: 2026-08-16
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0001
  - ADR-0002
  - ADR-0004
rule_refs:
  - RULE-0001
  - RULE-0003
---

# ADR-0024 — Ürün deposunun sınırı ve wiki yayın yeri

## Bağlam

`ptn-assurance-platform` deposu 2026-08-16'da `github.com/mertbyd/ptn-assurance-platform`
adresine **public** olarak push edildi. Push öncesinde depo üç ayrı türde içerik taşıyordu:

1. `ptn-test-module/` — gerçekten derlenen ve çalışan tek kaynak ağacı;
2. `vault/` — `CheckNexus.Vault` adapterinin kaynağı;
3. `docs/` — kanonik Obsidian wiki'si (kendi `.git` deposu, kök depo tarafından ignore).

Wiki'yi kök depoya gömme denemesi yapıldı ve geri alındı. Denemede iki mekanik sorun görüldü:
`docs/` nested bir Git deposu olduğu için normal `git add` içeriği değil **submodule gitlink**
üretiyor; ayrıca kök depo merge'ü wiki dosyalarını CRLF ile yeniden yazarak nested depoda 122
dosyalık sahte bir diff oluşturuyor.

Bağımsız olarak `vault/` kaynağının depoda durması da tutarsızdı: `ptn-test-module` bu adapteri
zaten **PackageReference** (`CheckNexus.Vault`) ile tüketiyor, hiçbir yerde ProjectReference
yok. Yani kaynak depoda dursa bile build'e girmiyordu.

## Karar

### A. Kök depo yalnız çalışan kaynağı izler

Kök depoda izlenen küme şudur ve genişletilmez:

| İzlenen | Ne |
|---|---|
| `ptn-test-module/` | `host/`, `src/`, `test/` ve host `Dockerfile`'ı |
| `.gitignore` · `NuGet.Config` · `README.md` | Depo iskeleti ve tek prose dosyası |

Bunun dışında hiçbir klasör kök depoya eklenmez. `docs/`, `vault/`, `scripts/`, `checkers/`,
`AGENTS.md` ve `CLAUDE.md` `.gitignore` ile dışarıda tutulur.

### B. `vault/` kaynağı depodan çıkarıldı

`vault/` 2026-08-16'da `23dd372` ile takipten çıkarıldı ve `.gitignore`'a eklendi. Kaynak
diskte durmaya devam eder; tüketim yalnız NuGet paketi üzerindendir. Adapterin kendi kaynak
sahipliği bu depo değildir.

Aynı gerekçe `checkers/` için zaten geçerlidir (ADR-0002): paket sınırında tüketilen bir şeyin
kaynağı tüketici deposunda ikinci kez yaşamaz.

### C. Wiki'nin yayın yeri GitHub Wiki'dir

`docs/` kanonik yazma yeri olarak kalır: Obsidian vault'u, kendi `main` dalı ve kendi geçmişi
olan ayrı bir Git deposudur. **Yayın** yeri ise bu deponun GitHub Wiki'sidir
(`<repo>.wiki.git`). Wiki, ürün kaynak commit'ine karıştırılmaz.

Kök depoda `git add -f docs` çalıştırılmaz ve `docs/` ignore kuralı kaldırılmaz.

### D. Geçmiş yeniden yazılmaz

`vault/` kaynağı ve kısa süreli wiki commit'i Git geçmişinde ve daha önce push edilmiş
dallarda durur. Depo public olduğu için bu commit'ler okunabilir kalır. `filter-repo` ile
geçmiş temizliği ve force-push **kararı alınmamıştır**; gerekirse ayrı bir ADR ister.

## Alternatifler

- **Wiki'yi `docs/` altında kök depoya gömmek.** Denendi ve geri alındı. Nested `.git`
  submodule gitlink üretiyor; içerik gömmek için wiki'nin kendi geçmişini devre dışı bırakmak
  gerekiyordu. Ayrıca mimari kararları, DBML şemasını ve task metinlerini public kaynak
  deposuna kalıcı olarak yazıyordu.
- **Wiki'yi `docs/` submodule'ü yapmak.** Wiki deposunun upstream'i yerel bir yoldur; submodule
  referansı başka hiçbir makinede çözülmez. ADR-0002'de checker'lar için aynı gerekçeyle
  reddedildi.
- **`vault/` kaynağını depoda tutmak.** Build'e girmiyor, ikinci bir kaynak sahipliği yanılsaması
  üretiyor ve paket sürümü ile kaynak arasında sessiz drift riski taşıyor. Reddedildi.
- **Depoyu private yapmak.** Görünürlük kararı bu ADR'nin konusu değildir; depo public
  bırakıldı ve sınır daraltılarak çözüldü.

## Sonuçlar ve riskler

| Risk | Önlem |
|---|---|
| Wiki'nin GitHub'da güncel kalmaması | Yayın `docs/` deposundan push'tur; kanonik kopya tek ve Obsidian tarafındadır |
| `vault/` kaynağının sessizce eskimesi | Kaynak sahipliği adapterin kendi deposundadır; bu depo yalnız yayımlanmış paket sürümünü tüketir |
| Geçmişteki içeriğin public kalması | Bilinçli kabul; §D'de kayıtlı, temizlik ayrı karar |
| Yeni bir klasörün refleksle depoya girmesi | §A'daki izlenen küme kapalı listedir; genişletme ADR ister |
| GitHub Wiki'de klasör/`[[wikilink]]` davranışının Obsidian'dan farklı olması | Kanonik okuma yeri Obsidian vault'udur; GitHub Wiki yayın kopyasıdır |

---
id: INBOX-0001
type: inbox
status: draft
title: Karara baglanmamis acik sorular
updated: 2026-08-13
decision_refs:
rule_refs: []
---

# Açık sorular

Bu sayfa kanonik bilgi kaynağı değildir. Bir madde karara bağlandığında ilgili
Current/Rule/ADR/Roadmap sayfasına taşınır ve **buradan silinir**.

> Bu klasördeki araştırma ve plan belgelerinin haritası için
> [[05-Operations/Research-Index|GUIDE-0005]] açılır.

## Açık

- **`SystemStandards.Abp.Authorization` + `SystemStandards.Authorization.Contracts` hangi sürümle
  yayımlanacak?** İkisi de nuget.org'da yok; ikisi de csproj'da hâlâ `1.0.0`. Aile 2.x'te
  (`Abp 2.0.2`, `Core`/`AspNetCore`/`Validation 2.0.0`). Yerel cache'te **farklı baytlı** bir
  `1.0.0` durduğu için aynı numarayla yayın yapılmamalı. Sürüm seçimi ve push ürün/paket
  sahibinindir — [[01-Current/Platform-Truth|CURRENT-0001]] blokaj 6.
  > Bu madde gün içinde bir ara "kapandı, teşhis yanlıştı" diye kapatılmıştı; **o kapatma
  > hatalıydı** ve geri alındı. `SystemStandards.Abp 2.0.2` bu namespace'leri içermiyor.
- **Kök `NuGet.Config` düz metin parola taşıyor ve depoda izleniyor.** Ortam değişkenine mi
  taşınacak, parola döndürülecek mi? — TASK-KBP-116 §6 S2.
- **RULE-0008 DMN satır kapsamı yayın şartı mı?** Kod gate'inde ölçülmüyor; kural mı gevşeyecek,
  altıncı gate mi eklenecek? — TASK-KBP-116 §6 S3.
- ~~**`SpecFingerprint`'i kim üretecek?**~~ **Kapandı (2026-08-16).** Soru hatalı kurulmuştu:
  kaynak checker'da zaten public — `ISpecSnapshotAppService.GetAsync` →
  `SpecSnapshotDetailDto.SpecContent.CanonicalHash`. Karar değil kod işi; TASK-KBP-117 Dilim 4.
- **`ProfileFingerprint`'i kim üretecek — yoksa kapı onu zorunlu olmaktan mı çıkaracak?**
  `ScenarioPublicationGateManager.cs:57` dolu ister; sunucuda üreten yol yok, KBP-116 ise
  bilinçle boş bırakıyor. `MaterialIntegrity` bugün elle değer verilmeden geçmiyor —
  [[01-Current/Platform-Truth|CURRENT-0001]] blokaj 9. **Öneri:** KBP-112 profil paketi kaynak
  portunu (`POST authoring/profile-packs`, `Authoring/profiles`) getirdiği için profil paketinin
  **kendi** kanonik hash'i artık türetilebilir; `ApplyRulesFingerprint` ile birebir aynı desen
  kurulur ve KBP-116'nın "başka bir belgenin hash'i konmaz" gerekçesi ihlal edilmez. Diğer iki
  yol: kapıdan düşürmek (ADR-0020 revizyonu) veya çağırana bırakmak (mühür zayıflar).
- **`abp.*` tablolarını kim uygulayacak?** Authenticator aynı veritabanına önce mi deploy edilir,
  yoksa ayrı bir migration bundle mı koşar? Test Module yabancı `DbContext` migrate etmez
  (RULE-0002) — TASK-KBP-117 Dilim 3.
- **TypeScript yazarlık ajanı ve eval harness'ı hangi ticket numarasını alacak?** `KBP-112`
  .NET'e gitti, `KBP-113`/`KBP-114` yakıldı, `KBP-115` canlı smoke, `KBP-116` backend kapanışı.
- `CheckNexus.Vault` public NuGet.org'a mı, şirket içi feed'e mi yayımlanacak?
- Test Module consumer graph'ında hedef ABP sürümü hangisi olacak?
- Notifications'ın ilk paket sınırı ve stable release kapısı nedir?
- Tek `DbMigrator` mı kullanılacak, yoksa consumer deploy pipeline migration bundle mı çalıştıracak?
- Test Module UI'ı ayrı bir uygulama mı, composition host içinde mi yaşayacak?

## 2026-08-13'te kapatılanlar

| Soru | Nereye taşındı |
|---|---|
| Test Orchestrator kalıcı modeli `TestPlan`/`TestRun`/`TestStep`/binding/evidence'ı nasıl ayıracak? | [[03-Decisions/ADR-0016-Kayit-Ve-Teshis-Veri-Modeli\|ADR-0016]] — **4 ana tablo + 5 lookup**; adım kaydı yok, kanıt HAR artefaktı (ADR-0015) |
| MCP için ilk izinli Application.Contracts use-case listesi nedir? | RULE-0005 + PLAN-0003 Blok 3 — 12 tool, an bazında profil |
| Test Module checker'larla nasıl konuşacak? | ADR-0015 §F — sorular için doğrudan çağrı, olgular için olay |
| Ürün içi sohbet ajanı hangi runtime ve model sağlayıcısıyla başlayacak? | [[03-Decisions/ADR-0023-TypeScript-Yazarlik-Ajani-Runtime-Ve-Model-Siniri\|ADR-0023]] — Node 24 LTS, provider-neutral port, ilk adapter OpenAI Responses API; yerel destek F1 ≥ 0.90 kapısına bağlı |
| Şema adları, bölümleme, BLOB sağlayıcısı, `HistoryId` formülü | ADR-0016 |
</content>

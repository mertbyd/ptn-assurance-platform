---
id: PLAN-0002
type: plan
status: draft
title: API Contract Checker eklenecek ozellikler — Test Module oracle ve MCP butcesi
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0005
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# API Contract Checker'a eklenecek özellikler

Bu liste [[90-Inbox/RESEARCH-0005-ApiContract-Oracle-Ve-Mcp-Butce-Mimarisi|RESEARCH-0005]] belgesinin
**tek, tekrarsız ve sıralı** iş özetidir. Araştırma gerekçesi orada, **yapılacak iş burada**.
PLAN-0001 (`DBC-xx`) Database Checker'ın listesidir; bu liste onun API tarafındaki kardeşidir ve
numaralandırması **ACC-xx**'tir.

**Kapsam:** Test Module'ün koşum / teşhis / bakım anlarında api-contract'tan isteyeceği yüzeyler ve
MCP maliyet-doğruluk kapıları.
**Kapsam dışı (bu tur):** spec karşılaştırma motorunun genişletilmesi, yeni fark türü ekleme, yeni
kaynak/format desteği. Karşılaştırma motoru bugünkü haliyle girdi kabul edilir.

**Boyut:** S ≈ 1–3 gün · M ≈ 1–2 hafta · L ≈ 2–4 hafta (tek geliştirici, test dahil).

## Kaynak gerçeği durum notu — 2026-08-12

Bu Inbox planı current truth değildir. Kaynakta doğrulanan durum:

| Madde | Durum | Kaynak kanıtı |
|---|---|---|
| ACC-01/02 | Tamamlandı | `ISpecSchemaResolver`; `ValueRetentionPolicyResolver` + `FindingValueRedactor` |
| ACC-05 | Tamamlandı | Request/response conformance AppService ve controller yüzeyleri |
| ACC-07/08 | Tamamlandı | Operation resolver ile bounded operation/schema/request-example/binding özetleri |
| ACC-09 | Kısmi | SHA-256 fingerprint ve New/Known/Resolved/Unknown sınıflaması mevcut; kalıcı suppression kaydı açık |
| ACC-10 | Tamamlandı | Sekiz bileşenli public `FindingAddressDto`, Mapperly projection ve exact README grameri |
| ACC-13/16 | Tamamlandı | Deterministik diagnosis manager/rule/probe zinciri ve bounded RFC 9457 raporu |
| ACC-19 | Tamamlandı | Public output ceilings, `TrimToBudget` ve açık truncation/result-ref davranışı |
| ACC-20 | Kısmi | Assertion derivability yüzeyi mevcut; planın tüm G1/G2 kapsamı tamamlandı sayılmıyor |
| ACC-23 | Tamamlandı | `ApiContractCheckerActivity` span ve güvenli attribute sahipliği |

Ek olarak Test Module Sınıf A talepleri ACX-01/02 source'ta kapandı: typed finding address ile
`SinceRunId`/bounded `Fingerprints` filtreleri mevcut. Kısmi satırlar kalan plan kapsamını korur.

---

## Blok 0 — Borç kapatma (özellik değil; bunlar bitmeden oracle yüzeyi kurulamaz)

| # | Ne | Neden | Dokunulan yer | Kaynak | Boyut |
|---|---|---|---|---|---|
| **ACC-01** | **Şema çözümleyici.** `ISpecSchemaResolver`: saklanan `SpecContent`'ten (operasyon, durum kodu, medya tipi) için doğrulanabilir şema düğümünü çözer; `$ref` çözümü, `allOf` düzleştirmesi, OAS 3.0 dialect uyarlaması; önbellek anahtarı `CanonicalHash`. **Doğrulayıcı kütüphane seçimi ADR ile** | `SpecSchemaPropertyModel` yalnız `Name/Type/Nullable/Required/EnumValues/ReferenceId` taşıyor; `format`, `maxLength`, `pattern`, `additionalProperties` ve iç içe `properties` yok. Diff için yeterli, **doğrulama için değil**. Ham metin `SpecContent.Content`'te durduğu için **migration gerekmez** | Yeni `Domain/Interface/Snapshots/ISpecSchemaResolver.cs`, `Domain/Managers/Snapshots/*`, `Microsoft.OpenApi 2.11.0` üstüne doğrulayıcı | RESEARCH-0005 §3.6 | **M** |
| **ACC-02** | **Değer saklama politikası.** `ValueRetentionMode`: `None` (varsayılan) / `Hashed` / `Masked` / `Full`; `Full` ayrı izin + TTL ister. `Finding.OldValue/NewValue` ve tüm yeni yüzeyler bu politikadan geçer | Bugün bulgu ham metin taşıyor. Hem gizlilik hem **prompt injection** sınırı; DB tarafındaki DBC-02'nin karşılığı | `Models/Runs/Finding.cs`, yeni `Constants/.../ValueRetentionModeCodes`, uygunluk ve teşhis DTO'ları | RESEARCH-0005 §3.4 | M |
| **ACC-03** | **Snapshot çevre olguları.** `ParsedSpecModel`'e `servers[]`, `info.version` ve yanıt header adları; probe hedefi ve snapshot tazeliği bunlardan hesaplanır | Probe hedefi ve `H-EN-03`/`H-EN-04` hipotezleri bu veriler olmadan kurulamaz | `Models/Snapshots/ParsedSpecModel.cs`, `SpecSnapshotModel`, ilgili normalizasyon manager'ı | RESEARCH-0005 §4.5 | S |
| **ACC-04** | **Giden HTTP emniyet profili.** Yalnız safe metot (`GET`/`HEAD`/`OPTIONS`), izinli host kümesi (`servers[]` + yapılandırılmış `ConnectionRef`), redirect politikası, per-istek timeout, toplam bütçe. `Microsoft.Extensions.Http.Resilience` üstüne kurulur | Probe'lar SSRF ve durum değiştirme yüzeyidir; sınır arayüzde olmalı, kod incelemesinde değil | `EntityFrameworkCore/Adapters/Sources/SpecFetcherClient.cs` deseni; yeni `Adapters/Diagnosis/*` | RESEARCH-0005 §4.5 | S |

---

## Blok 1 — Oracle yüzeyi (en yüksek iş değeri; Test Module'ün koşum anı)

| # | Ne | Neden | Dokunulan yer | Kaynak | Boyut |
|---|---|---|---|---|---|
| **ACC-05** | **Yanıt uygunluk API'si.** `IResponseConformanceAppService.AssertResponseAsync`: (snapshot, operasyon, gözlenen durum/medya tipi/header/gövde) → kapalı `ConformanceOutcomeCodes` + ihlal listesi. Serbest şema kabul edilmez; şema **yalnız** snapshot'tan çözülür. Sonuç **≤ 512 bayt** ve **değer taşımaz** (JSON Pointer + kural kodu + şema anahtar sözcüğü) | Senaryonun her API adımı bunu çağıracak. Alternatifi tam karşılaştırma (büyük ve yavaş) veya runner'ın kendi doğrulayıcısını yazması (paket sınırı ihlali + sürüm kayması) | Yeni `Services/Conformance/*`, `Managers/Conformance/*`, `Dtos/Conformance/*` + FluentValidation, `Permissions/Definitions/Conformance/*`, `Controllers/Conformance/*` | RESEARCH-0005 §3.2 | **L** |
| **ACC-06** | **Uygunluk politikası ve profilleri.** `ConformanceRuleCodes` (Schemathesis beşlisi + `additional-properties` + `security-requirement`), `ConformanceLevelCodes` (`Ignore`/`Info`/`Warn`/`Fail`), `ConformanceProfileCodes` (`Strict`/`Runtime` varsayılan/`Lenient`); kural bazında seviye çözümü | Uygunluk ikili değil politikadır. Profil olmadan ya gürültü testleri kırar ya kural tamamen kapanır. `LevelResolver` deseni | Domain.Shared `Constants/Conformance/*`, `Managers/Conformance/ConformancePolicyResolver` | RESEARCH-0005 §3.2 | M |
| **ACC-07** | **Operasyon çözümleyici + `OperationNotResolved` invariant'ı.** Gözlenen istek snapshot'ta **tek** operasyona çözülemiyorsa assertion **koşmaz**; path şablonu belirsizliği, `operationId` yokluğu ve çoklu server prefix'i açıkça ele alınır | ADR-0007'nin `KeyNotUnique` invariant'ının API karşılığı: "o operasyon" garantisi yoksa sessiz yanlış cevap verilmez | `Managers/Conformance/OperationResolver`, `SpecOperationModel` üzerinde | RESEARCH-0005 §3.3 | M |
| **ACC-08** | **Bilgi yüzeyi (yalnız yazım anı).** `FindOperationAsync` (method, path, zorunlu parametreler, yanıt şeması **özeti**) ve `DescribeSchemaAsync` (tek şema, 1 seviye). Tam OpenAPI gövdesi **asla** dönmez; ağır içerik `resource_link` | Ajan senaryoyu yazarken tam spec okumasın; token bütçesi burada belirlenir | `Services/Snapshots/*` genişletmesi; yeni özet DTO'lar | RESEARCH-0005 §5.7 | M |

> **Sınır kararı:** Checker SUT'a **yazmaz.** `IDiagnosisProbe` ve uygunluk yüzeyi yalnız safe metot
> tanır; test verisi seed/cleanup Test Module'ün `ITestDataSandbox` işidir (RULE-0004, ADR-0002).

---

## Blok 2 — Bulgu kalitesi (MCP "bakım anı"nın ön şartı)

| # | Ne | Neden | Dokunulan yer | Kaynak | Boyut |
|---|---|---|---|---|---|
| **ACC-09** | **Bulgu fingerprint'i.** `SHA256(kind \| direction \| normalized address \| normalized delta)` + `New`/`Known`/`Resolved` kovaları + susturma kaydı (fingerprint + gerekçe + kim + TTL) | `grep -ril "fingerprint" src` bugün **0 sonuç** veriyor. Fingerprint yoksa baseline, susturma, "bu yeni mi?" ayrımı ve `scenario.impacted` **mümkün değil**. DB tarafındaki DBC-09 ile aynı formül şekli | `Models/Runs/Finding.cs`, `ContractCheckFindings`, `Managers/Comparison/SpecDifferenceFactory.cs` | RESEARCH-0005 §1, §4.8 | M |
| **ACC-10** | **Etkilenen senaryo bağı.** Fingerprint → senaryo/adım eşlemesi için kararlı adres sözleşmesi (`FindingAddress` bileşenleri ↔ Arazzo adım referansı). Eşlemenin **sahibi composition host**; checker yalnız kararlı adresi ve fingerprint'i yayınlar | Bakım anının tüm kazancı bu eşlemeye dayanıyor; ama senaryo modeli checker'ın değil Test Module'ün nesnesidir (ADR-0002) | `Models/Runs/FindingAddress.cs` sözleşmesinin dokümantasyonu + DTO yüzeyi | RESEARCH-0005 §4.8 | S |

---

## Blok 3 — Dinamik teşhis motoru

| # | Ne | Neden | Dokunulan yer | Kaynak | Boyut |
|---|---|---|---|---|---|
| **ACC-11** | **Sinyal yakalama.** `HttpFailureSignal`: gözlenen yanıt metadatası + gönderilen isteğin metadatası (gövde hariç); kaynaklar: uygunluk sonucu `Fail`, HTTP durum hatası, taşıma hatası (DNS/TLS/timeout) | Teşhisin girdisi tek değer nesnesine indirgenmeden hipotez motoru kurulamaz | Yeni `Domain/Models/Diagnosis/HttpFailureSignal.cs` | RESEARCH-0005 §4.4 | S |
| **ACC-12** | **Kimlik çıkarıcılar.** `IFailureIdentityExtractor` uygulamaları: `ProblemDetails` (RFC 9457), `Challenge` (`WWW-Authenticate` + RFC 6750 hata kodları), `Allow` (405), `Transport`, `Assertion`. **Çıkarılan hiçbir ad snapshot'ta doğrulanmadan kullanılmaz**; doğrulanmazsa ad atılır ve `IdentityConfidence` düşer. Yapılandırılmamış gövde **ayrıştırılmaz** | HTTP zorunlu yapılandırılmış alanlar sunar (401→`WWW-Authenticate` MUST, 405→`Allow` MUST); bunlar PostgreSQL'in yapılandırılmış hata alanlarının karşılığıdır. Metin ayrıştırma lokalizasyon ve sürümle kırılır | Yeni `Domain/Interface/Diagnosis/IFailureIdentityExtractor.cs`, `Domain/Managers/Diagnosis/*` | RESEARCH-0005 §4.3 | **M** |
| **ACC-13** | **Teşhis çekirdeği.** 7 adımlı `DiagnosisManager` (yakala → kimlikle → yerelleştir → hipotez → kanıt → sırala → anlat), `IDiagnosisRule` / `IDiagnosisProbe` conventional DI kayıtları, `ProbeBudgetManager`, `HypothesisRankingManager` (saf), güven merdiveni `Confirmed/Likely/Possible/RuledOut`. **`RuledOut` gizlenmez, tek kök neden dayatılmaz** | Teşhis bir arama tablosu değil aramadır; kural `AppliesTo` yordamı olgulara bakar, durum kodu eşitliğine değil. DB tarafındaki `DiagnosisManager` ile **aynı şekil** | Yeni `Domain/Managers/Diagnosis/*`, `Domain/Interface/Diagnosis/*` | RESEARCH-0005 §4.1, §4.4 | **L** |
| **ACC-14** | **Probe ailesi (safe-only).** Olgu probe'ları (`SpecFact`, `SchemaViolationLocation`, `ContractDriftFact`, `ResponseHeaderFact`, `SnapshotFreshness` — **ağa çıkmaz**) ve safe ağ probe'ları (`OptionsAllow`, `HeadResource`, `ServerReachability`, `AuthMetadata`). Hedef yalnız `servers[]`/`ConnectionRef`'ten üretilir | `HeadResource`, DB tarafındaki `RowExists`'in karşılığıdır: "kaynak gerçekten oluştu mu" sorusunu DB erişimi olmadan cevaplar. Olgu probe'ları çoğu vakayı **sıfır ağ çağrısıyla** kapatır | Yeni `EntityFrameworkCore/Adapters/Diagnosis/*` (ağ) + `Domain/Managers/Diagnosis/Probes/*` (olgu) | RESEARCH-0005 §4.5 | M |
| **ACC-15** | **Hipotez kataloğu v1.** A sözleşme sapması (`H-CD-01..07`) · B istek şekli (`H-RQ-01..06`) · C kaynak/durum/sıra (`H-ST-01..06`) · D yetki (`H-AU-01..06`) · E ortam/dağıtım (`H-EN-01..05`) · F içerik pazarlığı (`H-NG-01..03`) · G throttling/zamanlama (`H-TH-01..04`) · H assertion (`H-AS-01..06`) | Her hipotez **ayrı sınıf**; yeni hipotez eklemek mevcut hiçbir dosyaya dokunmamalı. Genişletme sözleşmesi budur | `Domain/Managers/Diagnosis/Rules/*` | RESEARCH-0005 §4.6 | **L** |
| **ACC-16** | **Rapor yüzeyi.** RFC 9457 + `checknexus:` uzantı üyeleri, `type = urn:checknexus:problem:api-contract-diagnosis`, **≤ 4 KB** (`TrimToBudget` davranışı DB tarafındakiyle aynı), ABP `BusinessException`/`RemoteServiceErrorInfo` entegrasyonu, hipotez metinleri **lokalizasyon kaynağında**, OTel `error.type` / `http.response.status_code` | Paralel bir hata sistemi kurulmaz; ABP'nin lokalizasyon ve hata yüzeyi kullanılır | `Domain/Models/Diagnosis/DiagnosisReport.cs`, `Localization/ApiContractChecker/*`, `Controllers/Diagnosis/*` | RESEARCH-0005 §4.7 | S |
| **ACC-17** | **`SuggestedCheck` sözleşmesi.** `nextChecks` düz metin yerine tipli öneri: `CapabilityCode` + `OperationCode` + parametre çantası. api-contract paketi db-checker'a **bağımlılık almaz**; çözümü composition host yapar | Teşhisin en değerli anı cevabın öteki checker'da olduğu andır; ADR-0002/0008 sınırını bozmadan iki motoru birleştirir | Domain.Shared `Constants/Diagnosis/SuggestedCheckCodes`, rapor DTO'su | RESEARCH-0005 §4.8 | S |

---

## Blok 4 — MCP token bütçesi ve doğruluk kapıları

| # | Ne | Neden | Dokunulan yer | Kaynak | Boyut |
|---|---|---|---|---|---|
| **ACC-18** | **Statik katalog bütçesi kapısı.** Test: MCP yüzeyini ayağa kaldır → `tools/list` → her tool tanımını serileştir → tokenizer ile say. Eşikler: ≤ 12 tool, ≤ 4.000 token katalog, ≤ 400 token/tool, `inputSchema` ≤ 2 seviye, her tool'da `outputSchema`. Değerler `mcp-token-baseline.json` içinde; artış gerekçesiz geçemez. Yaklaşık ölçüm kullanılıyorsa **yaklaşık olduğu raporlanır** | Ölçümler tool tanımı maliyetinin %97'sinin `inputSchema` olduğunu, tool azaltmanın doğruluğu da artırdığını gösteriyor (Opus 4.5: %79,5 → %88,1). ADR-0008'in "sınırlı çıktı" borcu bugün **belgeli ama denetimsiz** | Composition host tarafı + checker'ın DTO yüzeyi; `accepted-deviations.json` deseniyle aynı disiplin | RESEARCH-0005 §5.2 | S |
| **ACC-19** | **Dinamik çıktı bütçesi.** Tavanlar: uygunluk 512 B · teşhis 4 KB · bulgu sayfası 32 KB · operasyon özeti 2 KB · ham gövde **hiç**. `verbosity` (`minimal` varsayılan) + `resultRef` handle ile tam gövde **yeniden çalıştırmadan** geri alınır; ağır içerik `resource_link` | Yayımlanmış bir API testi MCP sunucusunda ölçülen tasarruf: "normal" ~%65, "minimal" ~%95. `TrimToBudget` algoritması evde zaten yazılı | Uygunluk/teşhis DTO'ları; `DiagnosisReport.TrimToBudget` şekli | RESEARCH-0005 §5.3 | S |
| **ACC-20** | **Doğruluk kapıları G1–G2.** G1: Arazzo + `x-checknexus-*` şema doğrulaması, `operationId` çözümü, runtime expression bağlanabilirliği. **G2: her `successCriteria` sözleşmeden türetilebilir olmalı**; yanıt şemasında olmayan alana yazılmış assertion `AssertionNotInContract` ile reddedilir | RESTestBench: model, hatalı implementasyona karşı iyileştirme yaparken oracle'ı **hataya uyduruyor**. G2 bunu yapısal olarak imkânsız kılar ve ancak sözleşme snapshot'ı bizde olduğu için kurulabilir | `scenario.validate` çağrısının checker tarafındaki karşılığı: operasyon/şema çözümü API'si (ACC-01, ACC-07, ACC-08) | RESEARCH-0005 §5.5 | M |
| **ACC-21** | **G3 mutasyon kapısı.** Snapshot'a `DifferenceKindCodes` kataloğundan mutasyon uygula (alanı opsiyonel yap, enum değeri kaldır, tip değiştir, başarı durumunu kaldır) → mutant spec'ten stub üret → senaryoyu koştur → **kırmızıya dönmesi şart**. Skor = öldürülen mutasyon oranı; eşik altındaki senaryo onaya gitmez | Doğruluğun **tek ölçülebilir** tanımı. Mutasyon kataloğu sıfırdan tasarlanmaz; `DifferenceKindCodes` zaten "sözleşme kaç şekilde bozulur" cevabıdır. Yöntem Specmatic'in geriye dönük uyumluluk testinin tersidir | Yeni test aracı; `.agents/skills/acc-comparison-engine/scripts/` deseni | RESEARCH-0005 §5.5 | M |
| **ACC-22** | **G4 kararlılık kapısı + tool golden eval.** Senaryo sabit stub'a karşı N kez koşar, değişen alanlar `Volatile` işaretlenir (literal assertion uyarı üretir). Ayrıca her MCP tool'u için golden vaka seti: sabit girdi → sabit çıktı + **boyut iddiası** | Flaky yeşil, sessiz hata kadar pahalıdır. Diffy'nin gürültü iptali dersi + Anthropic'in "tool'u eval ile iyileştir" disiplini | Test projeleri; `accepted-deviations` biçimi | RESEARCH-0005 §5.5, §5.6 | S |
| **ACC-23** | **Gözlemlenebilirlik.** `ActivitySource` span'ları: `checknexus.api.conformance.assert`, `checknexus.api.diagnosis.run`, `checknexus.api.diagnosis.probe`; öznitelikler `checknexus.run.id`, `checknexus.moment` (`A\|B\|C\|D`), `checknexus.response_bytes`, `error.type`. **Yasak:** gövde içeriği, query değerleri, token, secret path | Dört an modeli ancak ölçülürse yönetilebilir; "koşumda 0 token" iddiası **testle** korunur | `Constants/Diagnostics/*`, uygunluk ve teşhis manager'ları | RESEARCH-0005 §5.1, §5.4 | S |

---

## Sıra ve gerekçesi

```text
Dalga 1  (borc)             ACC-01 -> 02 -> 03 -> 04
Dalga 2  (oracle)           ACC-05 -> 06 -> 07 -> 08
Dalga 3  (bulgu kalitesi)   ACC-09 -> 10
Dalga 4  (teshis)           ACC-11 -> 12 -> 13 -> 14 -> 15 -> 16 -> 17
Dalga 5  (butce/dogruluk)   ACC-18 -> 19 -> 20 -> 21 -> 22
Surekli                     ACC-23  (her dalgada kosar)
```

**Neden bu sıra:**

1. **ACC-01** olmadan hiçbir doğrulama yapılamaz; normalize model diff için tasarlanmıştır, doğrulama için
   değil. Ham metin saklandığı için bu bir migration değil, bir çözümleme işidir.
2. **ACC-02/04** özellik değil borçtur; müşteri ortamına girdikten sonra geri alınması pahalıdır
   (ham değer bulguya yazılmışsa geri alınamaz).
3. **ACC-05** Test Module'ün koşum anının ön şartıdır; o olmadan senaryo adımının API oracle'ı yoktur.
4. **ACC-09** bakım anının ön şartıdır; fingerprint olmadan "hangi senaryo etkilendi" sorusu sorulamaz.
5. **ACC-13** öncesinde **ACC-01**, **ACC-03** ve **ACC-09** bitmiş olmalı: teşhisin en değerli
   hipotezleri (`H-CD-*`, `H-EN-04`) o veriye dayanıyor.
6. **ACC-20/21** en sonda değil, **ACC-05 biter bitmez** başlar: doğruluk kapısı olmadan üretilen her
   senaryo geri dönüp temizlenecek borçtur.

---

## Kapsam dışı (bilinçli hayır)

| Öneri | Neden hayır |
|---|---|
| Karar yolunda LLM kullanmak | Oracle deterministik olmalı; LLM oracle'ları kırılgan ve hatalı implementasyona uyum sağlıyor. Model yalnız **öneri** üretir, güven ve kanıt hesaplanır |
| Durum kodu → metin eşleme tablosu | Aynı `400` on farklı sebepten döner; talebin kendisi statik eşlemeyi dışlıyor |
| Yapılandırılmamış hata gövdesini ayrıştırmak | Lokalize, sürümlü, enjeksiyon taşıyabilir; yalnız kanıt olarak, kırpılmış ve redaction'lı |
| Checker'ın SUT'a yazması / test verisi seed-cleanup | Safe-metot sınırı güvenlik modelinin taşıyıcısı; `ITestDataSandbox` Test Module'de (RULE-0004) |
| OpenAPI'den otomatik MCP tool üretimi | 200 endpoint = 200 tool; tool bütçesini tek başına tüketir. Gerçek sunucular operasyonların medyan %19'unu açıyor |
| Checker paketine MCP tipi/bağımlılığı koymak | ADR-0008 |
| Teşhis raporunu kalıcı tabloya yazmak | Hesaplanır ve döner; şema genişlemesi RULE-0002'ye takılır |
| Serbest JSON Schema'yı çağrandan almak | Serbest SQL yasağının karşılığı; şema yalnız saklanan snapshot'tan çözülür |
| Fark kataloğunu bu turda genişletmek | Karşılaştırma motoru bu turun kapsamı dışında; `DifferenceKindCodes` mevcut haliyle mutasyon kataloğu olarak yeterli |
| Checker'a bildirim/rapor dağıtımı | RULE-0004 + ADR-0002; Notifications ayrı capability |

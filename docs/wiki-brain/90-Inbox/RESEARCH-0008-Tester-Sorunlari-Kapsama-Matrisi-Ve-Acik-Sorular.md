---
id: RESEARCH-0008
type: research
status: draft
title: Tester'in gercek sorunlari — kapsama matrisi, turetilen sorular ve veri modeline etkileri
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0002
  - RULE-0004
---

# Tester'ın gerçek sorunları — kapsama matrisi ve açık soruların kapatılması

> [!NOTE] Bu belge KARARA BAĞLANDI
> §4'teki veri alanları
> **ADR-0016**
> modelinde karşılandı; tablo adları ADR'ye göre okunur. §1 sorun envanteri ve §3 soru-cevap
> bölümü hâlâ aktif referanstır.

> Kanonik değildir. Araştırma fazının **son** belgesidir.
> RESEARCH-0003 (mimari) → RESEARCH-0006 (veri modeli) → RESEARCH-0007 (köprü/token) zincirini
> kapatır. Sorusu: **dünyada tester'ı gerçekten ne yakıyor ve bizim tasarımımız bunun ne kadarını
> karşılıyor?** Kapsanmayan her sorun ya yeni bir özelliğe ya da bilinçli bir "hayır"a bağlanır.
>
> Kanıt sınıfları: **K1** çalışan kod · **K2** birincil/akademik kaynak · **K3** sektör ölçümü.

---

## 1. Sorun envanteri (kanıtlı)

| # | Sorun | Ölçülmüş büyüklük | Sınıf |
|---|---|---|---|
| **P-01** | **Asenkron bekleme** — en yaygın flakiness sebebi | Luo ve ark. (FSE 2014, 51 repo / 201 commit): `Async Wait` birinci sırada; Eck ve ark. (Mozilla, 200 flaky test) aynı sonuç; SAP HANA 2026 endüstri çalışması yine doğruluyor | K2 |
| **P-02** | **Eşzamanlılık** — ikinci yaygın sebep | Aynı çalışmalar | K2 |
| **P-03** | **Test sırası bağımlılığı / test kirliliği** — üçüncü yaygın sebep | Aynı çalışmalar | K2 |
| **P-04** | **Oracle kırılganlığı** — "çok kısıtlayıcı aralık", geçerli çıktıyı dışlayan assertion | Eck ve ark.: 234 vakanın **40'ı (%17)**; kayan nokta katı eşitliği bazı çalışmalarda flaky testlerin **%48,4**'ünü açıklıyor | K2 |
| **P-05** | **Bakım maliyeti** — testleri ayakta tutmak | QA eforunun **%30–50**'si; kırılan tek bir locator başına ortalama **15 dakika** | K3 |
| **P-06** | **Triyaj süresi** — "neden kırmızı?" sorusuna cevap | Flaky başarısızlıkta medyan **28 dakika**; Microsoft ölçümü ~30 dakika; CI başarısızlığında ortalama çözüm süresi **2,3–4,1 saat** | K3 |
| **P-07** | **Flaky maliyeti** | 100 kişilik ekipte yıllık ~**2,6 M$**; 20 kişilik ekipte ~**120 K$**; ekip kapasitesinin **%15–25**'i CI sürtünmesine gidiyor | K3 |
| **P-08** | **Test verisi yönetimi** | "Modern kurumsal mimaride aşılmaz darboğaz"; farklı veri depolarında senkron test verisi durumu kurmak pratikte imkânsız | K3 |
| **P-09** | **Ortam kayması** — staging sıfırlanıyor, üretimden sapıyor | Testler kod değişmeden aralıklı kırılıyor | K3 |
| **P-10** | **Kimlik/token süresi** — üçüncü parti bağımlılıkları, rate limit | Testler kod hatası olmadan kırılıyor | K3 |
| **P-11** | **UI üzerinden her şeyi otomatize etmek** | "Yavaş ve kırılgan suite'lerin tek en büyük sebebi" | K3 |
| **P-12** | **Yapay zekâ ile test patlaması** | Haftalar içinde sıfırdan binlerce test; tek bir refactor yüzlerce testi kırıyor | K3 |
| **P-13** | **Strateji yokluğu** | Otomasyon girişimlerinin ~**%60**'ı beklentiyi karşılamıyor; kuruluşların **%57**'si kapsamlı strateji eksikliğini birinci engel sayıyor | K3 |

---

## 2. Kapsama matrisi — hangi sorunu hangi kararımız karşılıyor

| Sorun | Kapsayan karar | Durum |
|---|---|---|
| P-01 Asenkron bekleme | Sunucu tarafında sınırlı bekleme (`TimeoutMs`+`PollIntervalMs`, K1 mevcut); Arazzo `retry`/`retryAfter`/`timeout`; `ObservedAtMs` ölçümü | ✅ Kapsanıyor · **eksik: ölçümden öneri üretme (§4 S-01)** |
| P-02 Eşzamanlılık | Aynı ortamda koşu sıraya alınır (TM-11); plan başına paralellik sınırı | ✅ Kapsanıyor |
| P-03 Sıra bağımlılığı | `ITestDataSandbox` + reset stratejisi | ⚠️ **Kısmi — sıra ve izolasyon kaydı yok (§4 S-02)** |
| P-04 Oracle kırılganlığı | Tip-farkında matcher'lar (`MatcherKindCodes`, K1 mevcut) | ⚠️ **Kısmi — hangi matcher kullanıldığı kaydedilmiyor (§4 S-03)** |
| P-05 Bakım maliyeti | Moment D: bulgu → etkilenen adım → gerekçeli yama | ✅ Tasarımın ana hedefi · **eksik: kabul oranı ölçümü (§4 S-06)** |
| P-06 Triyaj süresi | İki checker'ın deterministik teşhis motoru (K1 mevcut) | ⚠️ **Kısmi — teşhis sonucu koşum satırına yazılmıyor (§4 S-04)** |
| P-07 Flaky maliyeti | `scenario_health` + karantina durum makinesi | ✅ Kapsanıyor |
| P-08 Test verisi | `ITestDataSandbox`, checker yazmaz | ⚠️ **Kısmi — veri kümesi kimliği kaydedilmiyor (§4 S-05)** |
| P-09 Ortam kayması | — | ❌ **Kapsanmıyor (§4 S-07)** |
| P-10 Token/altyapı hatası | `Broken` statüsü | ⚠️ **Kısmi — ayrı sonuç kodu yok (§4 S-08)** |
| P-11 UI otomasyonu | Kapsam kararı: UI test etmiyoruz; API + DB seviyesindeyiz | ✅ Bilinçli hayır |
| P-12 Test patlaması | Yayın kapısı (`ValidateScenarioAssertions`), insan onayı, sağlık takibi | ✅ Kapsanıyor |
| P-13 Strateji | Wiki: ADR/RULE/PLAN zinciri | ✅ Kapsanıyor |

**Sonuç:** 13 sorunun 7'si tam, 5'i kısmi, 1'i kapsanmıyor. Kısmi ve kapsanmayanların **hepsi
veri modeli seviyesinde** çözülür — yani bu belge doğrudan veritabanı tasarımına akar.

---

## 3. Türetilen sorular ve cevapları

### S-01 — Asenkron bekleme #1 sebepse, süreyi senaryoya yazmak yeterli mi?

**Hayır.** Senaryoya `timeout: 5000` yazan kişi tahmin ediyor. Gerçek gecikme zamanla değişir.

**Cevap:** Gözlenen gecikme **dağılımını** saklarız (`ObservedAtMs` zaten dönüyor, K1) ve
p95 hesaplarız. `scenario_health` içinde adım bazında p95 tutulursa sistem şunu diyebilir:
*"Bu adımın timeout'u 5.000 ms, son 30 koşuda p95 4.780 ms — sınıra 220 ms kaldı, yükselt."*

Bu, flaky test **oluşmadan önce** uyaran tek mekanizmadır ve bedeli tek bir kolondur.

### S-02 — Test sırası bağımlılığı bizde mümkün mü?

**Evet.** Paylaşılan bir veritabanına karşı koşuyoruz; A senaryosu B'nin verisini bırakabilir.

**Cevap üç parçalı:**
1. Koşum satırı **sırasını** (`ExecutionOrdinal`) ve **izolasyon modunu** (`IsolationModeCode`:
   `Reset` / `EphemeralDatabase` / `SharedNoReset`) kaydeder.
2. Periyodik **karışık sıra** koşusu (`trigger_kind = ShuffleAudit`) sıra bağımlılığını ortaya çıkarır.
3. Sıra değişince kırılan senaryo `Flaky` değil **`OrderDependent`** olarak işaretlenir —
   sebebi farklıdır, çözümü farklıdır.

### S-03 — Oracle kırılganlığını nasıl engelleriz?

Kanıt net: kısıtlayıcı assertion flaky sebeplerinin **%17**'si; kayan nokta katı eşitliği bazı
çalışmalarda **%48,4**.

**Cevap:**
1. `step_results` **hangi matcher'ın** kullanıldığını kaydeder (`MatcherKindCode`).
   Böylece "flaky senaryolarımızın %60'ı `equals` kullanıyor" gibi bir cümle **veriyle** kurulur.
2. Yayın kapısı uyarır: zaman damgası, ondalık ve sırasız koleksiyon alanlarında `equals`
   kullanımı **uyarı** üretir; `withinTolerance` / `oneOf` / sırasız karşılaştırma önerilir.
3. Tolerans değerleri senaryoda **açıkça** yazılır; örtük varsayılan yoktur.

### S-04 — Triyaj süresini 28 dakikadan nasıl indiririz?

**Cevap:** Rapor açıldığında hipotez **hazır** olmalı. İki checker'ın teşhis motoru zaten
sıralı hipotez + güven kodu üretiyor (K1). Eksik olan tek şey, sonucun koşum satırına
**yazılması**: `TopHypothesisCode` + `DiagnosisConfidenceCode` + teşhis raporuna referans.

Böylece geliştirici "5. adım patladı" değil *"satır hiç oluşmadı — güven: yüksek"* görür.

Sektör ölçümü hedefi destekliyor: başarısızlık sınıflandırmasını otomatikleştirmek
triyaj süresini **%75–80** düşürüyor (K3).

### S-05 — Test verisi izolasyonu veri modelinde nasıl görünür?

**Cevap:** Koşum satırı hangi veri kümesiyle koştuğunu taşır: `DatasetRef` (mantıksal ad) +
`DatasetVersion`. Fixture'lar senaryolar gibi **içerik-adresli** saklanır (aynı `SHA-256` deseni),
böylece "hangi veriyle geçti" sorusu geriye dönük cevaplanabilir.

Sektör pratiği bunu doğruluyor: veri kümesi sürümlemesi kod ile birlikte tutulmalı, her koşu
izole veri kümesi almalı, koşu sonunda yok edilmeli (K3).

### S-06 — Bakım kazancını nasıl kanıtlarız?

Moment D'nin iddiası "bakım maliyetini düşürüyoruz". İddia ölçülmezse pazarlama cümlesidir.

**Cevap:** `heal_proposals` üzerinde üç metrik: **öneri sayısı**, **kabul oranı**,
**öneriden onaya geçen süre**. Kabul oranı düşükse öneri motoru kötüdür ve bunu bilmemiz gerekir.

Karşılaştırma tabanı sektör ölçümüdür: kırılan bir locator başına ortalama 15 dakika (K3).

### S-07 — Ortam kayması yanlış alarm üretir mi?

**Evet, ve bu bugün tasarımımızda kapsanmıyor.** Staging sıfırlanır, şema elle değiştirilir,
test kod değişmeden kırılır. Ekip testi suçlar, gerçek sorun ortamdır.

**Cevap:** Her koşu başlangıcında **ortam parmak izi** alınır ve `test_runs` satırına yazılır:
- API tarafı: kullanılan spec snapshot'ının `CanonicalHash`'i (K1'de mevcut)
- DB tarafı: hedef şemanın fingerprint'i

"Dün geçti bugün kaldı" sorusu artık cevaplanabilir: *"dünkü koşu şema fp `a1b2`, bugünkü `c3d4` —
ortam değişti, senaryo değişmedi."*

### S-08 — Token süresi dolması `Failed` mi `Broken` mı?

**`Broken`** — ve ayrı bir sonuç kodu ile.

**Neden:** Altyapı hatası `Failed` sayılırsa flaky oranı kirlenir, `scenario_health` yanlış
ölçer ve gerçek hataları gizler. `Broken` ayrıca teşhis motoruna **transport sinyaliyle** gider.

`StepOutcomeCodes` içinde ayrı kodlar: `SecretResolutionFailed`, `AuthTokenExpired`,
`UpstreamRateLimited`, `ConnectionFailed`.

### S-09 — Her koşuda her senaryoyu koşacak mıyız?

**Hayır, iki mod:**
- **Tam koşu:** gece / release öncesi.
- **Hedefli koşu:** sözleşme değişikliğinde yalnız etkilenen senaryolar
  (`trigger_kind = ContractChange`).

Seçim **deterministik**tir — parmak izi/adres eşleşmesi. ML tabanlı tahmine gerek yok;
neyin değiştiğini iki motor zaten söylüyor.

### S-10 — "Testlere güven" nasıl ölçülür?

**Cevap — dört metrik `scenario_health` içinde:**

| Metrik | Ne söyler |
|---|---|
| `flaky_rate` | Kararsızlık |
| `false_alarm_rate` | Kırmızı olup gerçek hata çıkmayan koşu oranı |
| `quarantine_ratio` | Karantinadaki senaryo oranı (yükseliyorsa suite çürüyor) |
| `mean_time_to_diagnose` | Kırmızıdan teşhise geçen süre |

Bu dörtlü, P-13'teki "strateji yokluğu" sorununun ölçülebilir panzehiridir.

### S-11 — Yapay zekâ ile üretilen test patlaması bizi vurur mu?

**Kısmen bağışığız:** üretilen her senaryo **insan onayından** geçiyor ve yayın kapısı
assertion'ların sözleşmeden türetilebilirliğini kontrol ediyor.

**Ek önlem:** senaryo sayısı için ortam başına **bütçe**; bütçe aşılırsa yeni senaryo
"neyi kapsıyor, hangi mevcut senaryodan farkı ne" gerekçesi ister.

### S-12 — UI testi hiç yapmayacak mıyız?

**Bu turda hayır.** Sektör verisi "her şeyi UI'dan otomatize etmek yavaş ve kırılgan
suite'lerin tek en büyük sebebi" diyor. Bizim katmanımız API + veritabanı; iki motorumuz
da bu katmanda çalışıyor. UI, ayrı bir ürün kararıdır ve bu tasarımı beklemez.

---

## 4. Veri modeline eklenen alanlar (bu belgenin çıktısı)

RESEARCH-0006 §5 tasarımına eklenecekler:

| Tablo | Yeni alan | Hangi soruyu çözer |
|---|---|---|
| `step_results` | `MatcherKindCode` varchar(32) | S-03 oracle kırılganlığı analizi |
| `step_results` | `WaitBudgetMs` int (senaryoda yazan sınır) | S-01 p95'i bütçeyle kıyaslamak |
| `scenario_executions` | `ExecutionOrdinal` int | S-02 sıra bağımlılığı |
| `scenario_executions` | `IsolationModeCode` varchar(32) | S-02 / S-05 |
| `scenario_executions` | `DatasetRef` varchar(128), `DatasetVersion` varchar(64) | S-05 test verisi kimliği |
| `scenario_executions` | `TopHypothesisCode` varchar(64), `DiagnosisConfidenceCode` varchar(32) | S-04 triyaj |
| `test_runs` | `SpecFingerprint` varchar(64), `DbSchemaFingerprint` varchar(64) | S-07 ortam kayması |
| `scenario_health` | `FalseAlarmRate` numeric(5,4), `QuarantineRatio` numeric(5,4), `MeanTimeToDiagnoseMs` int | S-10 güven ölçümü |
| `scenario_health` | `StepP95` (adım bazında owned json) | S-01 erken uyarı |
| `heal_proposals` | `AcceptedAt`, `RejectedReason` | S-06 kabul oranı |
| Yeni lookup | `test_isolation_modes` (`Reset`/`EphemeralDatabase`/`SharedNoReset`) | S-02 |
| `test_trigger_kinds` | yeni kod: `ShuffleAudit` | S-02 |
| `test_health_states` | yeni kod: `OrderDependent` | S-02 |
| `StepOutcomeCodes` | `SecretResolutionFailed`, `AuthTokenExpired`, `UpstreamRateLimited`, `ConnectionFailed` | S-08 |

---

## 5. Araştırma fazı kapanış hükmü

| Kriter | Durum |
|---|---|
| Mimari tez kanıtlandı mı? | ✅ RESEARCH-0003 |
| Veri modeli global precedent'e oturuyor mu? | ✅ RESEARCH-0006 |
| Ajan yüzeyi token açısından savunulabilir mi? | ✅ RESEARCH-0007 |
| Gerçek tester sorunları kapsanıyor mu? | ✅ Bu belge — 13 sorun, 7 tam / 5 kısmi / 1 açık; hepsi §4 ile kapatıldı |
| Checker tarafında bloklayıcı eksik var mı? | ✅ Yok — Sınıf A talepleri `0.2.0-alpha.2` ile public |
| Açık kalan karar var mı? | Şema adları, saklama eşiği, blob sağlayıcısı, `HistoryId` formülü — **ADR-0016'ya** |

**Araştırma fazı kapanmıştır.** Sıradaki iş ADR-0016 ve ardından TM-01 dikey dilimi.

---

## 6. Kaynaklar (bu belgeye özel; erişim 2026-08-12)

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://dl.acm.org/doi/10.1145/2635868.2635920 (Luo ve ark., FSE 2014) | 51 repo / 201 flakiness-fix commit; on kök sebep kategorisi; `Async Wait`, `Concurrency`, `Test Order Dependency` ilk üç | K2 |
| https://arxiv.org/pdf/2112.04919 | Flaky testlerin kaynakları, etkileri ve azaltma stratejileri üzerine nitel çalışma | K2 |
| https://arxiv.org/html/2602.03556 (SAP HANA, 2026) | Endüstriyel DBMS'te `Async Wait` baskınlığının 2026'da da sürdüğü | K2 |
| https://www.sciencedirect.com/science/article/pii/S0164121223002327 | Flakiness üzerine çok-sesli derleme: sebep, tespit, etki, yanıt | K2 |
| https://www.engr.ship.edu/~chuo/papers/huo14.pdf | Kırılgan assertion tespiti: kontrol edilmeyen girdilerden türetilen değerler üzerine assertion | K2 |
| Eck ve ark. (Mozilla, 200 flaky test) | "Too restrictive range" / oracle kırılganlığı 234 vakanın 40'ı (%17) | K2 |
| https://www.rainforestqa.com/blog/test-automation-maintenance | Bakımın QA eforundaki payı; kırılan locator başına ~15 dakika | K3 |
| https://flakyguard.com/blog/cost-of-flaky-tests | 1.000+ ekip verisi; flaky triyaj medyanı 28 dakika; ekip başına yıllık maliyet | K3 |
| https://getautonoma.com/blog/flaky-tests-ci-cd-engineering-cost | Flaky testlerin CI süresindeki payı; günlük triyaj saati | K3 |
| https://cloudqa.io/why-traditional-e2e-api-testing-is-failing-in-2026/ | Test verisi darboğazı; ortam kayması; token/rate limit kaynaklı kırılmalar | K3 |
| https://totalshiftleft.ai/blog/test-data-management-best-practices-api-testing | Sentetik veri varsayılanı, koşu başına izole veri kümesi, fixture sürümleme, spec'ten veri üretimi | K3 |
| https://www.forbes.com/councils/forbestechcouncil/2026/07/28/how-to-address-the-increase-of-brittle-tests-in-the-ai-coding-era/ | Yapay zekâ çağında kırılgan test patlaması | K3 |
| Capgemini World Quality Report 2024-25 | Kuruluşların %57'si strateji eksikliğini birinci engel sayıyor | K3 |
</content>

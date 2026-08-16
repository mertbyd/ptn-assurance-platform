---
id: ARCH-0004
type: current
status: active
title: Alti an — sistemin uctan uca akisi ve sorumluluk sinirlari
updated: 2026-08-13
decision_refs:
  - ADR-0007
  - ADR-0008
  - ADR-0014
  - ADR-0015
  - ADR-0016
rule_refs:
  - RULE-0005
  - RULE-0006
---

# Altı an — sistemin uçtan uca akışı

> **Bu sayfa Test Module'ün giriş kapısıdır.** Ürünün ne yaptığını ve sorumluluğun nerede
> durduğunu tek sayfada anlatır. Kararların gerekçesi ADR-0014/0015/0016'dadır.

## Girdi ve çıktı

**Girdi:** `senaryo.md` (insanın anlattığı iş akışı) + `kurallar.md` (iş kuralları).
**Çıktı:** hangi adımda, ne test edilirken, hangi katmanın neden hayır dediğini söyleyen bir rapor.

## Akış

```
senaryo.md + kurallar.md
        │
   ┌────▼─────────────────────────────────────────────┐
   │ AN 1  GİRİŞ            insan niyeti yükler       │
   ├──────────────────────────────────────────────────┤
   │ AN 2  ZEMİN            ajan checker'lara SORAR   │  ← uydurma yok
   ├──────────────────────────────────────────────────┤
   │ AN 3  YAZIM            Arazzo 1.0.1 dokümanı     │
   ├──────────────────────────────────────────────────┤
   │ AN 4  KAPI             türetilebilirlik + onay   │  ← ajan burada durur
   ├──────────────────────────────────────────────────┤
   │ AN 5  KOŞUM            Respect icra eder         │  ← ajan yok
   ├──────────────────────────────────────────────────┤
   │ AN 6  YARGI + TEŞHİS   checker hakem ve teşhis   │  ← ajan yok
   └──────────────────────────────────────────────────┘
```

**Ajan yalnız An 2-3-4'te vardır. An 5 ve An 6'da hiç yoktur** (RULE-0005).

---

## An 1 — Giriş

İnsan iki dosya yükler. `kurallar.md` **veritabanı tablosu değildir**: MCP `Resource` olarak
sunulur, Git'te durur, koşuda yalnız `rules_fingerprint` kaydedilir (ADR-0014 §A).

## An 2 — Zemin: ajan uydurmaz, sorar

Ajanın yazım anındaki tek girdileri niyet (`kurallar.md`), sözleşme (OpenAPI snapshot) ve yapı
(DB şeması)dır. **Çalışan sistemin davranışını görmez.**

| Ajanın sorusu | Çağırdığı yüzey | Modül |
|---|---|---|
| Bu iş adımı hangi operasyona düşüyor? | `SuggestOperationBindingsAsync` | API Contract Checker |
| Geçerli istek gövdesi neye benziyor? | `BuildRequestExampleAsync` | API Contract Checker |
| Hangi tablo/kolon var, anahtar PK/unique mi? | `DescribeTableAsync` | Database Checker |
| Hedef şemanın fotoğrafı | `GetSnapshotAsync` | Database Checker |

## An 3 — Yazım

Çıktı **Arazzo 1.0.1** dokümanıdır (sürüm gerekçesi: ADR-0014 §C düzeltmesi, AUDIT-0002); kendi DSL'imiz yoktur. Veritabanı doğrulaması
`x-checknexus-db` uzantısıyla yazılır ve yayın anında **gerçek bir Arazzo adımına derlenir**
(ADR-0015 §C).

İki belge saklanır: `source_document` (insanın onayladığı) ve `compiled_document`
(runner'ın koştuğu). Onay `source_hash`'e bağlıdır.

## An 4 — Kapı: ajanın durduğu yer

| # | Kapı | Kim geçirir |
|---|---|---|
| 1 | Şema geçerliliği (`redocly lint`) | Makine |
| 2 | **Türetilebilirlik** (`ValidateScenarioAssertionsAsync`) | Makine |
| 3 | **Zayıflama** (`assertion_count > 0`) | Makine |
| 4 | `dryRun` + onay (`Draft → PendingApproval → Published`) | **İnsan** |

Yayınlama **kademe 4**'tür: hiçbir otonomi seviyesinde otomatikleşmez. `dryRun` kırmızıysa
ajana sonuç değil **çelişki bildirimi** döner (RULE-0005, RULE-0006).

## An 5 — Koşum: ajan yok

```
ABP Background Job
  └─ redocly respect  (MIT, Docker, sabit sürüm)
       ├── SUT'a HTTP adımları
       └── DB Checker'a assertion adımları     ← in-line, doğru sırada
     çıktı: HAR 1.2 + JSON
```

Kendi koşum motorumuzu **yazmıyoruz**; .NET Arazzo runner'ı ekosistemde yok ve Respect MIT.
Runner `IWorkflowRunnerPort` arkasındadır (ADR-0015 §A).

## An 6 — Yargı ve teşhis: ajan yok

```
HAR'ın HER entry'si  →  AssertResponseAsync     ← yeşil adımlar dahil
kırmızı adımlar      →  DiagnoseAsync
                          ├─ api-contract    (contract / transport / auth)
                          └─ database-comp.  (persistence)
                        ↓
                    RFC 9457 raporu + sıralı hipotezler
                        ↓
                    test_runs / test_run_results / test_result_findings
```

Yeşil adımlar da uygunluk kontrolünden geçer: bir adım `$statusCode == 200` şartını geçmiş
ama gövdesi şemaya uymuyor olabilir (ADR-0015 §D).

---

## Sorumluluk haritası

| Soru | Cevaplayan | Cevaplayamayan |
|---|---|---|
| Bu adım hangi operasyona düşüyor? | API Contract Checker | — |
| Response sözleşmeye uyuyor mu? | API Contract Checker | Database Checker |
| Auth/challenge/transport ne dedi? | API Contract Checker | — |
| Satır düştü mü, değer doğru mu? | Database Checker | API Contract Checker |
| Şemada bu tablo/kolon var mı? | Database Checker | — |
| Neden patladı, en olası sebep ne? | Her iki checker'ın `DiagnosisManager`'ı | Ajan |
| Geçti mi kaldı mı? | **Yalnız checker** | Ajan, runner |
| Adımlar nasıl koştu? | Arazzo Runner | Checker'lar |

### Kayıt sahibi

Üç hakem vardır ve her bulgu kaynağını taşır (`source_checker_code`):

| Kaynak | Rolü |
|---|---|
| `Runner` | Hızlı ön kapı — `SCHEMA_CHECK`/`CONTENT_TYPE_CHECK` `warn` seviyesinde |
| `ApiContract` | **Sözleşme hükmünün kayıt sahibi** |
| `DatabaseComparison` | **Kalıcılık hükmünün kayıt sahibi** |

---

## Zamanlama kuralı

| Kontrol | Girdi | Nerede çalışır |
|---|---|---|
| Response uygunluğu | (istek, yanıt, spec) — saf fonksiyon | Koşum **sonrası**, HAR'dan |
| DB assertion | O andaki veritabanı durumu | Koşum **sırasında**, Arazzo adımı olarak |

DB assertion'ı HAR'dan çalıştırmak yasaktır: sonraki adımlar durumu değiştirmiş olabilir.

---

## Kalıcı kayıt

Veritabanı **hükmü** tutar (90 gün, sorgulanabilir); **ayrıntı** trace'tedir (kısa ömürlü,
adım adım). Köprü `test_runs.trace_id`'dir (W3C, 32 hex).

Model: **4 ana tablo + 5 lookup** — ADR-0016, şema kaynağı `Test-Platform-Schema.dbml`.

## Sistemin yapmadıkları

- Yük/performans testi değil (`duration_ms` regresyon sinyali verir, k6 yerine geçmez)
- UI testi değil — kapsam API + veritabanı
- API veya DB izi bırakmayan iş mantığını doğrulayamaz
- XPath criteria desteklenmez (Respect sınırı; yayın kapısında engellenir)

---
id: RESEARCH-0011
type: research
status: draft
title: Test Module ile checker'lar arasi entegrasyon deseni — baglam haritasi ve iletisim kurallari
updated: 2026-08-12
decision_refs:
  - ADR-0002
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0004
---

# Test Module ↔ checker entegrasyon deseni

> [!NOTE] Bu belge KARARA BAĞLANDI
> İçeriği **ADR-0015 §F**
> §C'ye taşındı. Belge gerekçe ve kaynak arşivi olarak durur; çelişki halinde **ADR kazanır**.

> Kanonik değildir. Soru: **iki checker'ın kendi tabloları ve alanları var; Test Module
> onlarla nasıl konuşacak?**
>
> Kanıt sınıfları: **K1** çalışan kod · **K2** birincil/resmî dokümantasyon · **K3** sektör pratiği.

---

## 0. Tek cümlelik kural

Modüler monolit literatürünün üzerinde birleştiği kural:

> **Sorular için doğrudan çağrı, olgular için olay. Asla paylaşılan veri üzerinden entegre etme;
> modüller arası anahtar, join veya transaction'a asla izin verme.** (K3)

Bu cümle bizim üç ihtiyacımızı birebir karşılıyor:

| İhtiyaç | Tür | Desen |
|---|---|---|
| "Bu satır oluştu mu?" | **Soru** | Doğrudan çağrı |
| "Bu yanıt sözleşmeye uyuyor mu?" | **Soru** | Doğrudan çağrı |
| "Karşılaştırma koşusu bitti" | **Olgu** | Olay (ETO) |

---

## 1. Kaçınılacak iki tuzak

Sektör kaynağı iki karşıt hatayı işaret ediyor (K3):

| Tuzak | Ne yapılır | Neden yanlış |
|---|---|---|
| **Sınırı çökertmek** | DbContext, repository, entity paylaşmak; checker tablosuna FK vermek | Modülerlik biter; checker sürümü Test Module'ü kırar |
| **Aşırı telafi** | Aynı süreç içinde HTTP çağrısı ve mesaj kuyruğu kurmak | Monolitin amacını yok eder; gereksiz gecikme ve karmaşıklık |

**Bizim konumumuz ortada:** aynı süreçte doğrudan arayüz çağrısı, ama **yalnız
`Application.Contracts` üzerinden** ve **kendi port arayüzlerimizin arkasından**.

---

## 2. Bağlam haritası (context map)

DDD'nin stratejik desenleriyle ilişkilerimizin adı:

| İlişki | Desen | Anlamı |
|---|---|---|
| Test Module → iki checker | **Customer–Supplier** | Checker tedarikçi, Test Module müşteri. Müşterinin ihtiyacı tedarikçiyi etkileyebilir — DBX-01/02 ve ACX-01/02 talepleri bunun sağlıklı hali |
| Checker'ların kararlı kod kümeleri | **Published Language** | `AssertionOutcomeCodes`, `DifferenceSeverityCodes`, `FindingAddressDto` — dışarıya verilen sözleşme. ADR-0008 bunu zaten "checker'ın MCP'ye borcu" olarak tanımlıyor |
| Test Module'ün köprü normalizasyonu | **Anti-Corruption Layer (ACL)** | İki checker'ın farklı gramerlerini tek ajan sözlüğüne çeviren katman (RESEARCH-0007 D-01). Adı literatürde budur |
| Test Module ↔ Authenticator | **Conformist** | Kimlik modelini olduğu gibi kabul ederiz |

**ACL'nin tanımı** literatürde şöyle: *"bounded context'inizi dış sistemlerin karmaşasından
koruyan, iki farklı domain modeli arasında çeviri yapan ve dış kavramların kod tabanınıza
sızmasını önleyen katman; genelde facade ve adapter ile kurulur."* (K3)

Bizde ACL'nin somut karşılığı: Test Module'ün **kendi port arayüzleri** ve onların
checker'a bağlanan adapter'ları.

---

## 3. Kullanım başına iletişim kararı

| # | Kullanım | Desen | Çağrılan | Ne zaman |
|---|---|---|---|---|
| 1 | Satır/sayı/yokluk kontrolü | **Doğrudan çağrı** | `IDatabaseAssertionAppService` | Her koşum adımı |
| 2 | Yanıt/istek uygunluğu | **Doğrudan çağrı** | `IResponseConformanceAppService` | Her API adımı |
| 3 | Başarısızlık teşhisi | **Doğrudan çağrı** | İki `IDiagnosisAppService` | Yalnız kırmızı adımda |
| 4 | Tablo tanımı, operasyon özeti | **Doğrudan çağrı + önbellek** | `ISchemaDiscoveryAppService`, snapshot servisleri | Yazım anı |
| 5 | Bağlantı/kaynak listesi (ortam kurulumu) | **Doğrudan çağrı + snapshot** | `IDatabaseConnectionAppService`, `ISpecSourceAppService` | Bağlama kurulurken |
| 6 | Yeni bulgular (moment D) | **Doğrudan çağrı**, `SinceRunId` + `Fingerprints` filtresiyle | `IComparisonRunAppService`, `IContractCheckRunAppService` | Sözleşme değişiminde |
| 7 | "Checker koşusu bitti" | **Olay (ETO)** | `ComparisonRunStatusChangedEto`, `ContractCheckRunStatusChangedEto` | Tetikleyici |
| 8 | Checker tablolarını okumak | **YASAK** | — | — |
| 9 | Checker tablosuna FK | **YASAK** | — | — |
| 10 | Ortak transaction | **YASAK** | — | — |

**4. satırdaki önbellek** ABP'nin kendi tavsiyesi: *"eğer Ordering modülü sık sık ürün verisine
ihtiyaç duyuyorsa bir tür önbellek katmanı kullanabilirsiniz, böylece Catalog modülüne sık
istek atmaz."* (K2)

Bizde önbellek anahtarı hazır: `SpecContent.CanonicalHash` (K1). Spec değişmediyse operasyon
özeti yeniden hesaplanmaz.

---

## 4. Kimlik referansı problemi — checker'ın Id'lerini nasıl tutuyoruz

Test Module'ün `test_environments.Bindings` içinde şunlar var:

```json
{ "logicalRef": "booking-db", "kind": "Database",
  "dbConnectionId": "8f3a…",           ← Database Checker'in Id'si
  "vaultPath": "secret/data/staging/booking",
  "snapshot": { "name": "Staging-PG", "engineCode": "PostgreSql", "host": "pg-staging" } }
```

### Kural: **kimlikle referans, FK ile değil**

| Adım | Ne yapılır |
|---|---|
| **Bağlama kurulurken** | `IDatabaseConnectionAppService.GetAsync(id)` çağrılır; varsa kaydedilir |
| **Görüntüleme için** | Ad/motor/host bir **snapshot** olarak kopyalanır (ekranda göstermek için her seferinde checker'a gitmemek) |
| **Koşum anında** | Doğrulama yapılmaz — doğrudan kullanılır; hata gelirse `Broken` |
| **Bağlantı silinmişse** | Koşum `Broken` + `ConnectionNotResolved`; snapshot bayat kalır, sorun değil |
| **Bayat snapshot** | Yeniden bağlama kurulduğunda tazelenir — okuma modelinin kuralı: *"senkron dışına düşerse yazma tarafından yeniden inşa edilir"* (K3) |

**FK neden yok:** modüller arası anahtar yasağı (§0). Ayrıca FK migration sırası bağımlılığı
yaratır ve checker paketini Test Module'ün şemasına bağlar.

---

## 5. Süreç içi mi, HTTP mi? — ikisi de, kod değişmeden

Checker paketleri sekizli ailedir ve içinde `HttpApi.Client` de var (K1).

| Dağıtım | Nasıl çalışır | Test Module kodu |
|---|---|---|
| **Composition host (bugün)** | Aynı süreç, doğrudan DI ile `Application` implementasyonu | **Değişmez** |
| Ayrı servis (ileride) | ABP dinamik C# client proxy, aynı arayüzü HTTP üzerinden uygular | **Değişmez** |

**Bunu mümkün kılan tek şey:** Test Module yalnız `*.Application.Contracts` arayüzlerine bağımlı.
Implementasyona bağımlı olsaydı bu esneklik kaybolurdu.

**`[IntegrationService]` kullanılmıyor.** ABP'nin integration service kavramı modül-modül
iletişimi için tasarlanmış ve varsayılan olarak dışarı açılmaz (K2). Bizim checker yüzeylerimiz
ise **bilinçli olarak public**: ADR-0008 checker'ların MCP olmadan da doğrudan HTTP ile
tüketilebilmesini zorunlu kılıyor. Yani bunlar normal AppService'lerdir, integration service değil.

---

## 6. Anti-corruption layer'ın koddaki hali

Test Module checker arayüzlerini **doğrudan** çağırmaz; kendi portlarını çağırır.

```
Test Module Domain (port)                 Adapter                        Checker
─────────────────────────────────────────────────────────────────────────────────
IDatabaseOraclePort                 →  DatabaseCheckerOracleAdapter  →  IDatabaseAssertionAppService
IApiOraclePort                      →  ApiCheckerOracleAdapter       →  IResponseConformanceAppService
IFailureDiagnosisPort               →  CheckerDiagnosisAdapter       →  iki IDiagnosisAppService
ICheckerFindingsPort                →  CheckerFindingsAdapter        →  iki run AppService
ISchemaKnowledgePort                →  SchemaKnowledgeAdapter        →  ISchemaDiscoveryAppService
```

**Adapter'ın üç işi:**
1. **Çeviri** — checker DTO'su → Test Module modeli
2. **Sözlük normalizasyonu** — iki farklı kod grameri → tek ajan sözlüğü
3. **Hata çevirisi** — checker istisnası → Test Module sonuç kodu

**Kazancı:** checker `0.3.0` çıkıp bir DTO değiştirdiğinde **yalnız adapter** değişir.
Manager'lar, koşum motoru ve MCP yüzeyi etkilenmez.

**Yerleşim:** yeni proje/katman açılmaz. Adapter'lar altyapı projesinin `Adapters/` klasöründe
yaşar — checker'ların `EntityFrameworkCore/Adapters/Sources/SpecFetcherClient.cs` precedent'i (K1).

---

## 7. Hata modları ve sonuç kodları

Checker çağrısı başarısız olabilir. Her durumun **ayrı** sonuç kodu olur, yoksa flaky oranı kirlenir.

| Durum | Adım sonucu | Kod |
|---|---|---|
| Checker zaman aşımı | `Broken` | `UpstreamTimeout` |
| Bağlantı bulunamadı | `Broken` | `ConnectionNotResolved` |
| Vault secret çözülemedi | `Broken` | `SecretResolutionFailed` |
| Anahtar tekil değil | `Broken` | `KeyNotUnique` (yayın kapısı kaçırmışsa) |
| Operasyon tek çözülemedi | `Broken` | `OperationNotResolved` |
| Assertion "hayır" dedi | **`Failed`** | `RowNotFound`, `ColumnMismatch`… |
| Uygunluk ihlali | **`Failed`** | `SchemaViolation`… |

**Ayrım neden kritik:** `Broken` ortam sorunudur, `Failed` gerçek bulgudur. Karışırsa
`scenario_health` yanlış ölçer ve gerçek hatalar gürültü altında kaybolur.

---

## 8. Tutarlılık ve transaction sınırı

| Soru | Cevap |
|---|---|
| Checker çağrısı Test Module transaction'ına dahil mi? | **Hayır.** Ayrı DbContext, ayrı sorumluluk |
| Assertion hedef veritabanına yazar mı? | **Hayır.** Salt-okunur (ADR-0007) |
| Koşum yarıda kalırsa checker tarafında ne kalır? | **Hiçbir şey.** Assertion ve teşhis kalıcı değildir, hesaplanır ve döner |
| Test Module kendi verisini ne zaman yazar? | Adım bittiğinde kendi UOW'unda |

**Sonuç:** dağıtık transaction yok, saga yok, telafi (compensation) yok. Çünkü checker'lar
**yazma yapmıyor**. ADR-0007'nin salt-okunur invariant'ı entegrasyonu da basitleştiriyor.

---

## 9. Sürüm bağımlılığı

| Risk | Karşı önlem |
|---|---|
| Checker `0.3.0` bir DTO'yu değiştirir | Adapter değişir, çekirdek etkilenmez (§6) |
| Checker kararlı kodu değişir | **Published Language sözleşmesidir** — ADR-0008 kapsamında kırıcı değişikliktir, ADR gerektirir |
| Checker paket ailesi sürüm kayması | Host başlangıcında sürüm uyum kontrolü |
| Yeni checker yüzeyi eklenir | Test Module'ün bilmesi gerekmez; port genişletilene kadar görmez |

---

## 10. Yasaklar (açık liste)

| # | Yasak | Neden |
|---|---|---|
| Y-1 | Checker tablolarını okumak (view, raw SQL, EF) | Paylaşılan veri üzerinden entegrasyon yasağı |
| Y-2 | Checker tablosuna FK | Modüller arası anahtar yasağı |
| Y-3 | Checker DbContext'ini enjekte etmek | Sınır çökertme tuzağı |
| Y-4 | Checker entity'lerini referanslamak | Aynı |
| Y-5 | Aynı süreçte HTTP ile checker çağırmak | Aşırı telafi tuzağı |
| Y-6 | Checker DTO'sunu Test Module domain'ine sızdırmak | ACL'nin varlık sebebi |
| Y-7 | Checker migration'ına dokunmak | RULE-0002 |
| Y-8 | Checker'a yazma yetkisi vermek | ADR-0007 |

---

## 11. Veri modeline etkisi

Bu belgeden doğan tek ekleme: bağlamalarda **snapshot** alanı.

```json
{ "logicalRef":"booking-db", "kind":"Database",
  "dbConnectionId":"8f3a…", "vaultPath":"…",
  "snapshot": { "name":"Staging-PG", "engineCode":"PostgreSql",
                "host":"pg-staging", "capturedAt":"2026-08-12T10:00Z" } }
```

Yeni tablo yok. `snapshot` ekranda göstermek içindir; **karar için değil** — karar anında
checker'ın kendisi çağrılır.

---

## 12. Kaynaklar (bu belgeye özel; erişim 2026-08-12)

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://abp.io/docs/latest/framework/api-development/integration-services | `[IntegrationService]`, varsayılan olarak dışarı açılmaz, `/integration-api` öneki, modül-modül iletişimi içindir | K2 |
| https://abp.io/docs/latest/tutorials/modular-crm/part-06 | Modüller arası doğrudan integration service çağrısı | K2 |
| https://abp.io/docs/latest/tutorials/modular-crm/part-07 | Local event bus ile in-process mesajlaşma; monolitte broker gerekmez | K2 |
| https://abp.io/support/questions/394/Communication-between-modules | Modüller arası iletişimi minimumda tut, modülleri birbirine bağlama; sık okuma için önbellek | K2 |
| https://www.kamilgrzybek.com/blog/posts/modular-monolith-integration-styles | Modüler monolit entegrasyon stilleri (referans uygulama) | K3 |
| https://milanjovanovic.tech/blog/modular-monolith-communication-patterns | *"Sorular için doğrudan çağrı, olgular için olay; asla paylaşılan veri, asla modüller arası anahtar/join/transaction"*; iki tuzak | K3 |
| https://deviq.com/domain-driven-design/context-mapping/ | Context map desenleri: ACL, Published Language, Customer-Supplier, Conformist | K3 |
| https://oneuptime.com/blog/post/2026-01-30-anti-corruption-layer-pattern/view | ACL tanımı: iki domain modeli arası çeviri, dış kavramların sızmasını önleme, facade + adapter | K3 |
| https://learn.microsoft.com/previous-versions/msp-n-p/jj591577 | Okuma modeli: sorulara verimli cevap için; senkron dışına düşerse yazma tarafından yeniden inşa edilir | K3 |
</content>

---
id: CURRENT-0004
type: current
status: active
title: Test Module integration readiness
updated: 2026-08-16
decision_refs:
  - ADR-0002
  - ADR-0003
  - ADR-0004
  - ADR-0005
  - ADR-0009
  - ADR-0012
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# Test Module entegrasyon hazırlığı

## Hazır olanlar

- İki checker auth’suz capability paketi olarak yayımlandı.
- Controller, AppService, Domain, Repository, EF Core ve migration yüzeyleri paket grafiğinde korunuyor.
- Her checker’ın bağımsız ince hostu source tree’de kalıyor ve paketlenmiyor.
- Ortak Vault adapteri iki checker secret portunu tek instance ile uyguluyor.
- Test Module güncel public paketleri tüketiyor: API Contracts `0.2.0-alpha.7`, Database
  Comparison `0.2.0-alpha.8`, Vault `0.2.0-alpha.2`; registry kontrolü sırasıyla
  8/8, 8/8 ve 1/1 PackageId doğruladı.
- Şema override sözleşmeleri kaynakta korunuyor.
- Foundation yedi paketlik `1.0.0` ailesiyle publictir ve registry'de doğrulanmıştır.
- Authenticator sekizli `2.0.0` ailesi **nuget.org'da yayımlandı; 8/8 PackageId registry'den
  doğrulandı (2026-08-13)**. Notifications altılı `0.1.0-alpha.1` ailesi de publictir (6/6).
- **Test Module composition host iskeleti kuruldu** (`ptn-test-module`, 2026-08-13): ABP CLI
  10.6.0 `module` şablonu, `host/ + src/ + test/`, Authenticator/Notifications/Emailing/
  SystemStandards/CheckNexus paketleri katman katman bağlı, `dotnet build` 0 hata, host module
  graph'ı ayağa kalkıyor ve HTTP pipeline istek işliyor.

### Database Checker oracle yüzeyi — `0.2.0-alpha.1` ile ilk kez public (2026-08-12)

- Hedefli assertion (`row` / `count` / `absent` / `batch`), sunucu tarafında sınırlı bekleme,
  tip-farkında matcher’lar, kararlı `AssertionOutcomeCodes`.
- Dinamik teşhis motoru: sinyal → kimlik → katalog → hipotez → sınırlı probe → sıralama → RFC 9457.
- Çapraz motor karşılaştırması kanonik tip haritasıyla düzeltildi; güven kodu artık dört değer üretiyor.
- Bağlantı emniyet profili ve değer saklama politikası (varsayılan `None`).
- Katalog derinliği: kısıt doğrulanmışlık durumu, collation, generated/identity/comment alanları.

Bu yüzeyler sekizli `0.2.0-alpha.1` ailesiyle NuGet.org'da yayımlanmıştır. Test Module
public paketi restore ederek tüketebilir; bu, aşağıdaki consumer kabul kapılarının geçtiği
anlamına gelmez.

### Alpha release kapısı

Checker aileleri source, NuGet.org ve Test Module consumer'ında kendi son sürümlerine
hizalıdır: API Contracts **`0.2.0-alpha.7`**, Database Comparison **`0.2.0-alpha.8`**;
Vault adapteri **`0.2.0-alpha.2`**'dir. Mevcut EF integration test hostları
composition paket modülünü doğrudan yükleyerek module initialization, controller
application-part/route/Swagger grubu ve EF model kurulumu smoke'unu çalıştırır.
Bu kapı alpha paketini doğrular; Test Module'deki Vault, tenant, authorization ve
ortak migration orchestration kapısının yerine geçmez.

Test Module consumer kapısı 2026-08-16'da bu üç sürümle tekrar çalıştı: Release build 0 hata;
Domain 215/215, Application 74/74 ve EF Core 27/27 olmak üzere **316/316** test başarılı.

### Sınıf A checker sözleşmeleri — public `0.2.0-alpha.2` (2026-08-12)

- Database Checker public typed address gramerini ve `SinceRunId`/bounded `Fingerprints`
  filtrelerini taşıyor.
- API Contract Checker sekiz operation-address bileşenini public DTO/README'de yayınlıyor ve
  aynı bakım-anı filtrelerini mevcut findings action'ında uyguluyor.
- Referans sorguları yalnız fingerprint scalarlarını okuyor; production provider count/page
  seçimini sunucuda tutuyor. Legacy null fingerprint `Unknown` kalıyor.
- İki solution restore/build/test ve dosya-ledger backend scanner kapılarından geçti; migration yok.

Bu sözleşmeler Test Module'ün DBX-01/02 ve ACX-01/02 entegrasyon engellerini kapatır ve
**`0.2.0-alpha.2` ile publictir** (2026-08-12; 16/16 PackageId registry'de doğrulandı).
Test Module artık bakım anını (`change.since` → `scenario.impacted`) paket restore ederek
kurabilir: `ChangeStateCode` ile `New`/`Known`/`Resolved` ayrımı ve `SinceRunId` +
en çok 100 SHA-256 `Fingerprints` filtreleri iki checker'da da bounded olarak mevcuttur.

Paket restore edilmesi consumer kabul kapılarının geçtiği anlamına gelmez; aşağıdaki
sekiz maddelik kapı ayrıca çalıştırılır.

## Tasarlandı, kod yazılmadı

| Konu | Karar kaydı |
|---|---|
| Test Module veri modeli (**4 ana tablo + 5 lookup**) | [[03-Decisions/ADR-0016-Kayit-Ve-Teshis-Veri-Modeli\|ADR-0016]] · `04-Architecture/Test-Platform-Schema.dbml` |
| Şema sahipliği `test_lookup`/`test_catalog`/`test_run` | ADR-0016 §A · [[04-Architecture/Database-Ownership\|ARCH-0003]] |
| Checker'larla iletişim deseni | ADR-0015 §F |
| MCP yüzeyi ve ajan sınırları | ADR-0008 · RULE-0005 |
| İş listesi (`TM-01..TM-59`) | [[90-Inbox/PLAN-0003-TestModule-Ozellik-Listesi\|PLAN-0003]] |

## Henüz olmayanlar

> [!IMPORTANT] Bu bölüm 2026-08-16'da düzeltildi
> Önceki hâli "Test Module iş kodu başlamadı" diyordu; bu **yanlıştır**. KBP-90'dan KBP-112'ye
> kadar sekiz migration, dört ana tablo, beş lookup, koşum/yargı hattı, yazarlık hattı, MCP
> yüzeyi, ortam bağlaması yazma ucu ve kaynak yükleme uçları teslim edildi. Güncel yüzey
> [[01-Current/Platform-Truth|CURRENT-0001]]'dedir. Aşağıda yalnız **gerçekten olmayanlar** kalır.

- **Canlı uçtan uca koşum kanıtı yok.** Gerçek bir SUT'a (ilk hedef `InventoryTrackingAutomation`)
  karşı altı-anın tamamı hiç koşturulmadı — KBP-115.
- **İzin verilebilecek yer yok.** MCP yetki kapısı KBP-116 ile kondu, fakat host
  `AbpPermissionManagement` compose etmiyor; `Bridge.*` izni kimseye verilemez —
  TASK-KBP-117 Dilim 1.
- **`abp.*` tablolarını kimse uygulamıyor.** Host yalnız `TestModuleDbContext`'i migrate eder;
  `abp.AbpSettings` ve `abp.AbpPermissionGrants` Authenticator'ındır. Boş veritabanında ayar ve
  izin yüzeyi çalışmaz — TASK-KBP-117 Dilim 3.
- **`SpecFingerprint` mühre bağlanmamış** → `MaterialIntegrity` gate'i gerçek baytla geçilemiyor.
  Kaynak checker'da zaten public (`SpecContent.CanonicalHash`); karar değil kod —
  TASK-KBP-117 Dilim 4.
- **Temiz klon/CI restore'u kırık.** `SystemStandards.Abp.Authorization` nuget.org'a **hiç push
  edilmedi** (yalnız yerel bir publish klasörüne pack edildi, bu makinenin cache'inde duruyor) ve
  publicteki `SystemStandards.Abp 2.0.2` **onun yerini almaz** — 2.0.2'de o namespace'ler yok.
  **Güncelleme (2026-08-16 akşamı):** SystemStandards ailesi lockstep **`2.1.0`** olarak
  yayımlandı; `Abp.Authorization` ve `Authorization.Contracts` ilk kez nuget.org'da. Kalan tek
  engel Notifications'tadır: yayımlanmış `0.1.0-alpha.1` hâlâ `Abp.Authorization` **1.0.0**'a
  bağlı. `0.1.0-alpha.2` gerekiyor ama alpha.1 kirli bir worktree'den yayımlandığı için
  commit'lenmiş koddan yeniden üretilemiyor — [[01-Current/Platform-Truth|CURRENT-0001]]
  blokaj 10.
- **Gerçek token ile login/refresh/logout ve selected-context uçtan uca doğrulaması** yapılmadı.
- **Tek DbContext/connection üzerinde migration orchestration kanıtı** (PostgreSQL'e karşı koşum)
  alınmadı; Authenticator migration'ları aynı veritabanına uygulanmadan Swagger 500 verir.
- **TypeScript yazarlık ajanı** (`ptn-test-agent`): ADR-0023 kabul edildi, kaynak kökte izlenmiyor.
- **UI** — headless runtime tamamlanmadan başlanmaz.

## Consumer kabul kapısı

Paket restore edilmesi tek başına entegrasyon kanıtı değildir. Test Module içinde aşağıdakiler geçmeden stable release kararı verilmez:

1. Tek ABP sürüm grafiği restore edilir.
2. Consumer doğrudan yalnız `Authenticator.*` paketlerini alır; Foundation `1.0.0`
   katmanları transitif çözülür ve doğrudan Foundation referansı bulunmaz.
3. Module initialization ve DI graph açılır.
4. Controller route ve tek Swagger yüzeyi görünür.
5. Tek bağlantıda EF model kurulabilir.
6. Migration assembly’leri çakışmadan uygulanır.
7. Identity/OpenIddict tablolarını yalnız Authenticator sahiplenir.
8. Checker secretları tek Vault adapterinden çözülür. **Geçti — KBP-92 composition testi.**
9. Tenant ve authorization negatif/cross-tenant davranışı consumer hostta doğrulanır.
10. Login, refresh, logout ve selected-context access-token turu gerçek hostta geçer.

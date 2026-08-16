---
id: INDEX-0001
type: index
status: active
title: PTN Assurance Wiki Brain
updated: 2026-08-16
decision_refs:
  - ADR-0001
  - ADR-0002
  - ADR-0003
  - ADR-0004
  - ADR-0005
  - ADR-0006
  - ADR-0007
  - ADR-0008
  - ADR-0009
  - ADR-0010
  - ADR-0012
  - ADR-0013
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - ADR-0017
  - ADR-0018
  - ADR-0019
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
  - RULE-0005
  - RULE-0006
  - RULE-0007
  - RULE-0008
---

# PTN Assurance Wiki Brain

> [!IMPORTANT] NuGet paket güncelleme/yayın giriş noktası
> Her paket sürümleme, pack, API key veya nuget.org push işinde önce
> [[05-Operations/NuGet-Package-Release-Playbook|GUIDE-0003]] açılır. Release scripti
> bilinmeyen çalışma dizininden relative `-File .\scripts\...` ile çağrılmaz.

Bu vault, API Contract Checker, Database Checker ve ortak Vault adapterinin sürüm kontrollü ortak hafızasıdır. Obsidian yalnız editördür; Markdown dosyaları asıl kayıttır.

## Bugünkü kapsam

```text
ptn-assurance-platform/
  checkers/
    api-contract/
    database-comparison/
  ptn-test-module/
    host/  src/  test/
  vault/
  docs/wiki-brain/
```

Bu workspace iki checker paket kaynağını, checker doğrulama hostlarını, testlerini, ortak Vault
adapterini, **Test Module composition hostunu** ve bu wiki’yi tutar. Test Module yayımlanmış
paketleri (Authenticator, Notifications, Emailing, SystemStandards, CheckNexus) consumer olarak
kullanır; 2026-08-13'te iskeleti kuruldu.

**Test Module'ün tasarımı bu wiki'de kayıtlıdır.** Ürünün uçtan uca akışı için tek giriş
noktası [[04-Architecture/Alti-An|ARCH-0004]]'tür. Kararlar: yazarlık
[[03-Decisions/ADR-0014-Senaryo-Yazarlik-Modeli-Ve-Turetilebilirlik-Kapisi|ADR-0014]],
koşum [[03-Decisions/ADR-0015-Kosum-Siniri-Dis-Arazzo-Runner|ADR-0015]],
kayıt ve teşhis [[03-Decisions/ADR-0016-Kayit-Ve-Teshis-Veri-Modeli|ADR-0016]].
Şema kaynağı `04-Architecture/Test-Platform-Schema.dbml` dosyasıdır.

> [!WARNING] ADR-0011 **silinmiştir** (2026-08-13)
> Ajanların eski tasarımı geçerli sanmaması için dosya kaldırıldı. İçeriği taşındı:
> veri modeli ve şema sahipliği → **ADR-0016**, koşum ve modül entegrasyonu → **ADR-0015**,
> ajan sınırları → **ADR-0014 + RULE-0005 + RULE-0006**.
> Herhangi bir yerde `ADR-0011` referansı görürsen o metin **eskidir**.
> Model **9 ana + 14 lookup → 4 ana + 5 lookup** oldu; kendi koşum motorumuz yerine
> MIT lisanslı dış Arazzo runner'ı (Redocly Respect) kullanılır.

## Bilgi katmanları

| Katman | Yetkisi |
|---|---|
| `01-Current` | Şu anda doğru kabul edilen birleşik gerçekler |
| `02-Rules` | Değişikliklerde korunması zorunlu sınırlar |
| `03-Decisions` | Kabul edilen kararların gerekçesi ve sonuçları |
| `04-Architecture` | Sistem, paket, host, DB ve Vault sınırları |
| `05-Operations` | Agent akışı, release kaydı, kaynaklar ve yol haritası |
| `90-Inbox` | Karara bağlanmamış notlar; kanonik bilgi değildir |
| `99-Templates` | Yeni kayıt şablonları; kanonik bilgi değildir |

## Giriş noktaları

- Workspace ve ürün durumu → [[01-Current/Platform-Truth|CURRENT-0001]]
- Yayımlanan checker paketleri → [[01-Current/Checker-Packages-Truth|CURRENT-0002]]
- Ortak Vault adapteri → [[01-Current/Vault-Truth|CURRENT-0003]]
- Test Module entegrasyon hazırlığı → [[01-Current/Integration-Readiness-Truth|CURRENT-0004]]
- UI backend controller kataloğu → [[01-Current/UI-Backend-Controller-Catalog|CURRENT-0005]]
- Sistem bağlamı → [[04-Architecture/System-Context|ARCH-0001]]
- Paket ve host haritası → [[04-Architecture/Package-Map|ARCH-0002]]
- Auth tüketim modeli → [[04-Architecture/Auth-Consumption-Model|ARCH-AUTH-CONSUMPTION]]
- DB, şema ve migration sahipliği → [[04-Architecture/Database-Ownership|ARCH-0003]]
- **Ekip kılavuzu (sıfırdan giriş, sözlük, kararlar) → [[05-Operations/Ekip-Kilavuzu|GUIDE-0004]]**
- **Ürünün uçtan uca akışı (altı an) → [[04-Architecture/Alti-An|ARCH-0004]]** ← *Test Module'e buradan girilir*
- Senaryo yazarlık modeli ve türetilebilirlik kapısı → [[03-Decisions/ADR-0014-Senaryo-Yazarlik-Modeli-Ve-Turetilebilirlik-Kapisi|ADR-0014]]
- Koşum sınırı, dış Arazzo runner → [[03-Decisions/ADR-0015-Kosum-Siniri-Dis-Arazzo-Runner|ADR-0015]]
- Kayıt ve teşhis veri modeli → [[03-Decisions/ADR-0016-Kayit-Ve-Teshis-Veri-Modeli|ADR-0016]] (şema kaynağı: `04-Architecture/Test-Platform-Schema.dbml`)
- Yazarlık hattı, assertion kaynakları, belirsizlik kapısı → [[03-Decisions/ADR-0017-Yazarlik-Hatti-Assertion-Kaynaklari-Ve-Belirsizlik-Kapisi|ADR-0017]]
- **Checker köprüsü: tek sözlük, tool bütçesi, kanıt zinciri → [[03-Decisions/ADR-0018-Checker-Koprusu-Tek-Sozluk-Tool-Butcesi-Ve-Kanit-Zinciri|ADR-0018]]**
- **Generic köprü: profil paketi, kanıt yolu verisi, yetenek seviyesi → [[03-Decisions/ADR-0019-Generic-Kopru-Profil-Paketi-Kanit-Yolu-Ve-Yetenek-Seviyesi|ADR-0019]]**
- Ajan gerçeklikleri (halüsinasyon, bağlam kaybı, yerel model) → [[90-Inbox/RESEARCH-0015-Ajan-Gerceklikleri-Ve-Checker-Koprusu|RESEARCH-0015]]
- Generic/dinamik köprü araştırması (yetkilendirme teşhisi, semantik bağlama, yazma kümesi) → [[90-Inbox/RESEARCH-0016-Generic-Ve-Dinamik-Kopru-Yetenek-Sablonu|RESEARCH-0016]]
- **Köprünün iki task'ı (KBP-87, KBP-88) → [[90-Inbox/PLAN-0004-Kopru-Iki-Task|PLAN-0004]]**
- Authenticator/Foundation kompozisyon kararı → [[03-Decisions/ADR-0012-Foundation-Backed-Authenticator-Composition|ADR-0012]] (host rolü kısmı [[03-Decisions/ADR-0013-Test-Module-Resource-Server-Auth-Consumption|ADR-0013]] ile revize edildi)
- **Test Module auth tüketimi (resource server) → [[03-Decisions/ADR-0013-Test-Module-Resource-Server-Auth-Consumption|ADR-0013]]**
- **Ürün deposunun sınırı ve bu wiki'nin yayın yeri → [[03-Decisions/ADR-0024-Depo-Siniri-Ve-Wiki-Yayin-Yeri|ADR-0024]]** ← *kök depo yalnız `ptn-test-module/` izler; wiki GitHub Wiki'de yayımlanır*
- Sonraki işler → [[05-Operations/Roadmap|GUIDE-0002]]
- Paket yayın kaydı → [[05-Operations/Package-Release-Ledger|LEDGER-0001]]
- NuGet paket güncelleme ve güvenli yayın akışı → [[05-Operations/NuGet-Package-Release-Playbook|GUIDE-0003]]
- **Araştırma indeksi (hangi belge hangi soruyu cevaplıyor) → [[05-Operations/Research-Index|GUIDE-0005]]**
- Resmî ve tarihsel kaynaklar → [[05-Operations/Source-Registry|SOURCE-0001]]
- Önceki araştırmaların eksiksiz arşivi → [[05-Operations/Research-Archive|ARCHIVE-0001]]

> `90-Inbox` altında 12 araştırma belgesi, 3 plan ve 1 backlog vardır. **Hepsini okuma** —
> amacına göre okuma sırası [[05-Operations/Research-Index|GUIDE-0005]]'te.

## Değişmez özet

- Checker’lar auth sahibi değildir; consumer hostun kimlik bağlamını kullanır.
- Auth tabanı `ABP -> Foundation -> Authenticator` zincirindedir; consumer yalnız
  `Authenticator.*` paketlerini doğrudan alır, Foundation katmanları transitif gelir (ADR-0012).
- Controller, AppService, Manager, Repository, EF Core ve migration katmanları korunur.
- Checker hostları geliştirme ve migration doğrulaması içindir; paketlenmez ve production’da ikinci owner olarak çalışmaz.
- DB şema ayarları silinmez. Her modül yalnız kendi tablolarının migration sahibidir.
- Tek `VaultSecretProvider`, iki checker’ın ayrı secret portunu aynı KV v2 adapteriyle uygular.
- Aynı paket sürümü değiştirilerek tekrar yayımlanmaz; her değişiklik yeni bir sürümdür.
- Database Checker aynı zamanda Test Module’ün veritabanı oracle’ıdır (ADR-0007); assertion ve teşhis salt-okunurdur.
- MCP sunucu yüzeyi composition host’tadır, checker paketinde değildir (ADR-0008).
- API checker JSON Schema doğrulaması public `NJsonSchema` 11.6.1 bağımlılığıdır (ADR-0009).
- API Contracts 0.2 interface genişlemesi yalnız tam üye hedefli PackageValidation suppression'larıyla kabul edilmiştir (ADR-0010).
- Test Module modeli **4 ana tablo + 5 lookup**tur; `test_lookup`/`test_catalog`/`test_run` şemalarını sahiplenir (ADR-0016).
- Test Module checker'lara **doğrudan çağrı** ile soru sorar, **olay** ile olgu dinler; checker tablosuna FK vermez, ortak transaction açmaz (ADR-0015 §F, ADR-0015).
- **Kendi koşum motorumuz yoktur.** Arazzo iş akışını dış runner (Redocly Respect, MIT) icra eder; `IWorkflowRunnerPort` arkasındadır (ADR-0015).
- **Veritabanı doğrulaması bir Arazzo adımıdır** — DB Checker'ın HTTP assertion ucuna giden sıradan bir adım; runner'a plugin yazılmaz (ADR-0015 §C).
- **Ajan hakem değildir.** Koşum ve yargı anlarında model yoktur; hüküm ve teşhis deterministik checker motorlarındır (RULE-0005).
- **Türetilemeyen assertion yayınlanamaz**; `assertion_count = 0` olan adım reddedilir (RULE-0006).
- **Ajan tahmin etmez.** Her soru ya checker'dan cevaplanır ya insana sorulur; açık uçlu alan yoktur, aktif tool ≤ 7, **kademe 4 eylemi Tool olarak kaydedilemez** (RULE-0007).
- **Karar tablosunun her satırı test edilir** — `Allow` satırı kapsanmayan sürüm yayınlanamaz; aşırı-engelleme aksi hâlde görünmez (RULE-0008).
- Köprü **tek ajan sözlüğü** sahibidir; ajan checker'ın ham kodunu görmez. Fingerprint'ler birleştirilmez, `{sourceChecker, fingerprint}` çifti olarak verilir (ADR-0018).
- **Alıntısız hipotez rapora giremez**; teşhis ve yazarlık aynı kanıt zinciri desenini iki yönde çalıştırır (ADR-0018).
- **Arazzo hedef sürümü `1.0.1`'dir**, 1.1 değil — `respect`'in 1.1 belgesi koştuğu doğrulanamadı (ADR-0014 §C düzeltmesi, AUDIT-0002).
- **Her checker çağrısı opsiyonel `CorrelationRef { TraceId, StepKey }` taşır ve sonuçta aynen geri yansıtılır**; batch eşleşmesi indeksle değil kimlikle kurulur (ADR-0021).
- **Senaryo sürümü beş malzemeyi mühürler**: senaryo belgesi (`SourceHash`), `kurallar.md`
  (`RulesFingerprint`), API snapshot (`SpecSnapshotId` **+** `SpecFingerprint`), DB şeması
  (`DbConnectionId` **+** `DbSchemaFingerprint`) ve profil paketi (`ProfileFingerprint`).
  Mühür tutmuyorsa koşum `Failed` değil **`Inconclusive`**'dir (ADR-0020).
  > [!WARNING] `ProfileFingerprint` çözülmemiş çelişkidir (2026-08-16)
  > `ScenarioPublicationGateManager.IsMaterialSealComplete` altı alanın hepsini — profil dâhil —
  > **dolu ister**; buna karşılık KBP-116 profil parmak izini **bilinçli olarak boş bırakmaya**
  > karar verdi ve sunucuda onu üreten hiçbir yol yok (yalnız çağıranın DTO'sundan geçer).
  > Yani `MaterialIntegrity` kapısı bugün elle değer verilmeden geçilemez. Ayrıntı
  > [[01-Current/Platform-Truth|CURRENT-0001]] blokaj 9.
- **Kanıt zinciri veridir, kod değil**; kavramı somut şemaya **profil paketi** bağlar ve bağlanmamış kavram tahmin değil **soru** üretir (ADR-0019).
- **Cevaplayamamak birinci sınıf sonuçtur**: `NOT_BOUND` / `Unavailable` / `Inconclusive`. Kanıt okunamadığında *"yetki yok"* denmez, *"kanıt toplanamadı"* denir (ADR-0019 §C).
- **Etki ayak izi yeteneği yoklanır, varsayılmaz**: `Exact` / `RowAddressed` / `Inferred` / `Unavailable`; v1 motoru **PostgreSQL**'dir ve ayak izi hiçbir seviyede oracle değildir (ADR-0019 §E).
- Geri alınamaz ajan eylemleri (yayınlama, yama uygulama) **hiçbir otonomi seviyesinde otomatikleşmez** (RULE-0005).
- Test Module hostu **resource server**'dır: Authenticator katmanlarını tip olarak alır,
  `HttpApi` modülünü compose etmez; kimlik uçları ayrı deploy edilen Authenticator hostundadır
  ve doğrulama JWT bearer ile yapılır (ADR-0013, ADR-0012 md 4'ü revize eder).
- Authenticator `2.0.0` (8 paket) ve Foundation `1.0.0` (7 paket) nuget.org'da publictir;
  2026-08-13'te 21/21 PackageId registry'den doğrulandı.

## Wiki kuralı

Eski karar silinmez veya sessizce yeniden yazılmaz. Değişen karar yeni ADR ile eskisini `supersedes`; eski ADR yeni kaydı `superseded_by` ile gösterir. Kalıcı bilgi değiştiğinde ilgili Current, Rule, ADR, Architecture, Roadmap ve Source kaydı aynı iş içinde güncellenir.

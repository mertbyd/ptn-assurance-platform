---
id: AUDIT-0003
type: audit
status: open
title: Wiki-kod denetimi — tur 4-7: auth, paketleme, dis bagimliliklar ve kopru kodu
created: 2026-08-14
updated: 2026-08-14
decision_refs:
  - ADR-0012
  - ADR-0013
  - ADR-0015
  - ADR-0017
  - ADR-0018
  - ADR-0020
rule_refs:
  - RULE-0001
---

# AUDIT-0003 — Turlar 4, 5, 6, 7

> Denetim serisinin kapanışı. Turlar 1-2 [[90-Inbox/AUDIT-0001-Checker-Interop-Bulgulari|AUDIT-0001]]
> ve [[90-Inbox/AUDIT-0002-Runner-Gerceklik-Bulgulari|AUDIT-0002]]'de; tur 3 (veri modeli)
> temiz çıktı ve TASK-KBP-90'a not olarak işlendi.

---

## Tur 4 — Auth ve secret sınırı · **temiz**

| İddia | Kaynak | Sonuç |
|---|---|---|
| Host **resource server**'dır; `Authenticator.HttpApi` compose **edilmez** | ADR-0013 | ✅ Host modülünde kimlik ucu yok; *"bu host yalniz bearer token dogrular"* |
| `Nexum.Abp.Foundation.*` **doğrudan referanslanmaz**, Authenticator üzerinden transitif gelir | ADR-0012 | ✅ Hiçbir csproj'da `PackageReference` yok — tek geçtiği yer bir **yorum satırı** |
| Authenticator katmanları **tip olarak** alınır | ADR-0013 | ✅ Beş projede referanslı |

**Bulgu yok.** ADR-0012/0013 kod tarafında birebir uygulanmış.

---

## Tur 5 — Paketleme ve sürüm

### BULGU-11 — Host csproj'unda sabit sürüm · **düşük**

**Kural.** `ptn-test-module/AGENTS.md`: *"Surumler `common.props` icindeki degiskenlerden
yonetilir; csproj'a sabit surum yazilmaz."*

**Gerçek.**

```
host/Ptn.TestModule.HttpApi.Host.csproj:29  Serilog.AspNetCore   Version="9.0.0"
host/Ptn.TestModule.HttpApi.Host.csproj:30  Serilog.Sinks.Async  Version="2.1.0"
```

**Etki düşük** — host paketlenmiyor (RULE-0001: checker/host'lar paketlenmez), yani nuget.org'a
sızmıyor. Ama kural istisnasız yazılmış ve iki satır onu deliyor. Sürüm yükseltmesi
`common.props` yerine csproj'dan yapılacağı için **sessiz sürüm sürüklenmesi** riski taşıyor.

**Düzeltme.** İki sürüm `common.props`'a değişken olarak taşınır. Tek satırlık iş.

---

## Tur 6 — ADR-0017'nin dış bağımlılıkları · **doğrulandı**

ADR-0017'nin tamamı iki dış bağımlılığın **var olduğu** varsayımına dayanıyordu ve ikisi de
hiç doğrulanmamıştı.

| İddia | Sonuç |
|---|---|
| **DMN motoru:** `net.adamec.lib.common.dmn.engine`, OMG standart XML | ✅ NuGet'te, **v1.1.1**. Karar tablosu + ifade kararı destekliyor, model **Camunda modeler** ile tasarlanabiliyor, `DmnDefinitionBuilder` ile kodla da kurulabiliyor. Kaynak: `adamecr/Common.DMN.Engine` |
| **Yapılandırılmış çıktı:** `Microsoft.Extensions.AI` → `ChatClientStructuredOutputExtensions` | ✅ `GetResponseAsync<T>(...)` var; `useJsonSchema` parametresi **varsayılan `true`**; generic tipten JSON şeması **otomatik** üretiliyor, dönüş `ChatResponse<T>` |

**İki not:**

1. `GetResponseAsync<T>`'in belgesi şunu söylüyor: JSON şeması *"model native structured
   output destekliyorsa güvenilirliği artırır, **desteklemiyorsa hata verebilir**"*. Yani
   ADR-0017 §B'nin *"generic tip argümanından JSON şeması çıkarılır"* iddiası doğru, ama
   **model yeteneğine bağlı** — yerel model adapter'ında (RULE-0005) bu ayrı ölçülmeli.
2. Bulunan `Microsoft.Extensions.AI.Abstractions` sürümleri **preview** hattında görünüyor.
   Üretime alınacak sürüm hattı **kilitlenmeli** ve `common.props`'a yazılmalı.

**Bulgu yok**, ama iki nokta ADR-0017'ye risk satırı olarak eklenmeli.

---

## Tur 7 — Köprü kodu (KBP-88/89 çıktısı)

### BULGU-12 — ADR-0020 malzeme mührü uygulanmamış · **yüksek**

`PtnMaterialSeal` yok; `PtnProfilePack`'te `SpecFingerprint` ve `DbConnectionId` yok.
Yayın kapısının dördüncü ve beşinci kontrolü (malzeme bütünlüğü, `sourceDescriptions`
tutarlılığı) **çalışmıyor**.

Zaten `TASK-KBP-89` manifestosunda (madde 38-39) yazılı; **henüz yapılmamış**.

### BULGU-13 — `SchemaName` yasağının kapsamı belirsiz · **orta** · *karar gerektirir*

**ADR-0018 §A ve risk tablosu:** *"`PtnLocation` — `apiSchemaName` / `dbSchemaName` /
`dbTableName`, **çakışan ad yok**"* ve *"Ad çakışması **testle** yasaklanır: `SchemaName`
adında alan bulunmamalı."*

**Gerçek:**

| Tip | `SchemaName` var mı |
|---|---|
| `PtnLocation` | ❌ **yok** — `ApiSchemaName`/`DbSchemaName`/`DbTableName` ayrı ✅ |
| `PtnCheckerTableDescription` | ✅ var |
| `PtnDatabaseAssertionRequest` | ✅ var |
| `PtnDatabaseAssertionSignal` | ✅ var |

**ADR'nin asıl koruduğu yer sağlam** — `PtnLocation` temiz ve çakışma orada anlamlıydı
(iki anlam aynı anda taşınıyor). Kalan üç tip **tek yönlü, yalnız DB tarafına giden**
modeller; orada belirsizlik yok ve ad hizalaması Mapperly'yi `[MapProperty]`'siz tutuyor.

**Çelişki metinde:** ADR-0018 yasağı *"köprü sözlüğünde"* diye geniş yazmış; uygulama
dar yorumlamış.

**Karar gerekiyor:** ya (a) ADR-0018 metni **konum ve rapor tiplerine** daraltılır — kod
olduğu gibi kalır, drift testi yalnız o aileyi tarar; ya da (b) üç tip yeniden adlandırılır
ve `[MapProperty]` kabul edilir. **(a) öneriliyor** — çünkü (b) mapper saflığı kuralını
deler.

### Kabul edilmiş sapma — ADR-0015 §F anti-corruption katmanı

`Domain/Interface/Bridge/` altında port arayüzü **yok**; üç manager doğrudan checker
tiplerine bağımlı (`ApiOracleManager`, `DatabaseOracleManager`, `FailureDiagnosisManager`).
ADR-0015 §F *"anti-corruption layer zorunludur"* diyor.

**Bu sapma kullanıcı kararıyla kabul edildi** (2026-08-14): *"mantık olarak hata yoksa kalsın."*
Kayıtta duruyor çünkü sonucu ölçülebilir: **checker DTO'su değişince `Domain` kırılır.**
KBP-628/KBP-711 tam olarak checker DTO'larını değiştiriyor — alanlar opsiyonel olduğu için
kırılma beklenmiyor, ama o iki iş bittikten sonra Test Module derlemesi **bir kez** kontrol
edilmeli.

---

## Denetim serisi özeti — 13 bulgu

| # | Bulgu | Ciddiyet | Durum |
|---|---|---|---|
| 01 | Checker'larda korelasyon kimliği yok | Yüksek | ADR-0021 + KBP-628/711 açıldı |
| 02 | Batch korelasyonu indekse bağlı | Orta | KBP-711'e girdi |
| 03 | DB assertion türetilebilirlik kapısı yok | Yüksek | **Açık** — ayrı task gerekiyor |
| 04 | İki farklı RFC 9457 tel formatı | Orta | KBP-628'e girdi |
| 05 | Ortam eşleşmesi doğrulanmıyor | Orta | **Açık** — koşum task'ına düşecek |
| 06 | Projeksiyon yüzeyi yok | Yüksek | **Açık** — ADR-0019 §F'de kayıtlı |
| 07 | Arazzo 1.1 doğrulanmamış | Yüksek | **Kapandı** — hedef 1.0.1'e çekildi |
| 08 | `REDOCLY_CLI_RESPECT_SEVERITY` yok | Orta | **Kapandı** — ADR-0015 §E düzeltildi |
| 09 | "Girdi dosyası" yolu yok | Düşük | **Kapandı** — ADR-0015 §G düzeltildi |
| 10 | Maskeleme kapsamı kayıtta eksik | Düşük | **Kapandı** — AUDIT-0002'de kayıtlı |
| 11 | Host csproj'unda sabit sürüm | Düşük | **Açık** — tek satırlık iş |
| 12 | ADR-0020 malzeme mührü uygulanmamış | Yüksek | **Kapandı — KBP-92**; senaryo aggregate'i ve beş yayın kapısı uygular |
| 13 | `SchemaName` yasağının kapsamı belirsiz | Orta | **Açık** — karar gerekiyor |

**Doğrulanan ve sağlam çıkan:** veri modeli ↔ DBML (tur 3), auth kompozisyonu (tur 4),
ADR-0017'nin iki dış bağımlılığı (tur 6), Respect'in on iddiası (AUDIT-0002 §1),
`PtnLocation` ad ayrımı.

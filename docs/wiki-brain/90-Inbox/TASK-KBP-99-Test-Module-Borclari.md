# AJAN GÖREVİ — KBP-99 · Test Module küçük borçları: marker sınıf, sabit sürüm, drift kapsamı

Tek görev, **üç derlenebilir dilim**. **Her dosyayı yazmadan önce §1'deki kapıdan geç.**

Bu görev yeni yetenek eklemez; **üç kayıtlı borcu** kapatır. Üçü de küçük, üçü de bugün
yazılı bir kuralı deliyor.

---

## 0. Kimlik ve ön koşullar

```
Depo    : C:\Users\mertb\RiderProjects\ptn-assurance-platform
Modül   : ptn-test-module   (solution: Ptn.TestModule.slnx)
Branch  : KBP-99   (KBP-100 üzerinden — §2.1)
Motor   : PostgreSQL
Commit  : #KBP-99 <type>: <past-tense English description>
```

| Ön koşul | Durum |
|---|---|
| KBP-100 dört dilimi commit edilmiş, build/test yeşil | ⚠️ **doğrula** — bu görev onun üzerine kurulur |
| `HarArtifactConsts.ContainerName` Domain.Shared'da | ✅ KBP-95 |
| `AbpBlobStoringModule` `DependsOn`'da | ✅ KBP-95 |
| `common.props` sürüm değişkeni kalıbı | ✅ KBP-87 |
| Bridge sözlük drift testi | ✅ KBP-91 (`VocabularyDriftTests`) |

**Dosya bütçesi ≈12.** Üç dilim, dilim başına bir commit. Her dilim yeşil kapanır.

---

## 1. YAZMA KAPISI

| Yazacağın | Önce oku (skill) | Sonra bak (canlı örnek) |
|---|---|---|
| BLOB sınırı servisi | `house-profile.md` → *AppService has no private helpers* | `src/Ptn.TestModule.Application/Services/Runs/HarArtifactService.cs` (mevcut hali) |
| Sabit / kod kümesi | `house-profile.md` → *Stable strings* | `src/Ptn.TestModule.Domain.Shared/Constants/Runs/HarArtifactConsts.cs` |
| csproj / sürüm değişkeni | `house-profile.md` → *Versions* | `ptn-test-module/common.props` + `src/*/*.csproj` |
| Drift testi | `layers-and-files.md` | `test/Ptn.TestModule.Domain.Tests/Bridge/VocabularyDriftTests.cs` |

**Kanonik kararlar:** `ADR-0016 §H` (HAR artefaktı BLOB'a, satırda yalnız `har_blob_name`),
`ADR-0018` (köprü sözlüğü ad çakışması yasağı), `RULE-0001` (paket/host sınırı),
`ptn-test-module/AGENTS.md` (*"Sürümler `common.props` içindeki değişkenlerden yönetilir;
csproj'a sabit sürüm yazılmaz"*).

**Denetim kaydı:** `AUDIT-0003` BULGU-11 (sabit sürüm) ve BULGU-13 (`SchemaName` kapsamı).

---

## 2. Sabitlenen kararlar — tartışmaya açık değil

### 2.1 Branch konumu

`KBP-99` dalı **`KBP-100`'ün üzerine** açılır; `KBP-100` yeşil kapanmadan bu görev başlamaz.

```
git checkout KBP-100
git checkout -b KBP-99
```

`KBP-95` ve `KBP-100` dallarına dokunulmaz. Force-push, rebase, yeni depo yok.

### 2.2 `HarArtifactContainer` marker sınıfı **kaldırılır**

Bugünkü hal:

```csharp
[BlobContainerName(HarArtifactConsts.ContainerName)]
public sealed class HarArtifactContainer { }          // ← davranışı olmayan marker

private readonly IBlobContainer<HarArtifactContainer> _blobContainer;
```

**Gerekçe — ölçülmüş, varsayılmış değil:** Tipli container'ın tek kazancı, host kompozisyonunda
`options.Containers.Configure<HarArtifactContainer>(...)` ile **tip güvenli konfigürasyondur.**
Bu depoda öyle bir konfigürasyon **yoktur**; `TestModuleApplicationModule` yalnız
`AbpBlobStoringModule`'e `DependsOn` der ve container'ı hiç ayarlamaz. Yani marker sınıf hiçbir
kazanç sağlamadan bir dosya, bir tip ve bir `using` maliyeti taşıyor. `dotnet-clean-code-standards`
§4 bunu açıkça reddediyor: *"Do not add a wrapper, provider, factory, resolver, handler ... without
a real owner and precedent."*

**Karar — ABP'nin adla çözme yolu kullanılır:**

```csharp
private readonly IBlobContainer _blobContainer;

public HarArtifactService(IBlobContainerFactory blobContainerFactory)
{
    _blobContainer = blobContainerFactory.Create(HarArtifactConsts.ContainerName);
}
```

`IBlobContainerFactory.Create(string name)` ABP BLOB Storing'in **belgelenmiş** ikinci
çözüm yoludur; tipli container'ın eşdeğeridir ve marker tip gerektirmez.

**Yan kazanç:** container adının **tek sahibi** artık gerçekten `HarArtifactConsts.ContainerName`
(Domain.Shared). Bugün ad hem sabitte hem attribute'ta duruyor; kaldırınca Domain.Shared
sahipliği tekleşir — `abp-coding-standards` §5 *"Put every stable meaningful string in its
Domain.Shared owner"* maddesinin tam karşılığı.

**Yapılacaklar:**

| # | Ne |
|---|---|
| 1 | `Application/Services/Runs/HarArtifactContainer.cs` **silinir** |
| 2 | `HarArtifactService` `IBlobContainerFactory` enjekte eder, container'ı **bir kez** ctor'da çözer |
| 3 | `SaveAsync` / `ReadAsync` / `DeleteAsync` gövdeleri **aynen kalır** — yalnız alan tipi `IBlobContainer` olur |
| 4 | `TestModuleApplicationModule`'de değişiklik gerekip gerekmediği **doğrulanır** (bugün tipli config yok → değişiklik beklenmiyor; varsayma, bak) |

**Yasak:** Bu fırsatla container'a TTL/provider konfigürasyonu **eklemek**. O iş `TM-15`'tir ve
bu görevin kapsamı dışındadır. Marker'ı kaldırırken davranış **birebir aynı** kalır.

### 2.3 Host csproj'undaki sabit sürümler `common.props`'a taşınır

`AUDIT-0003` BULGU-11. `ptn-test-module/AGENTS.md` istisnasız yazılmış, iki satır deliyor:

```
host/Ptn.TestModule.HttpApi.Host.csproj   Serilog.AspNetCore   Version="9.0.0"
host/Ptn.TestModule.HttpApi.Host.csproj   Serilog.Sinks.Async  Version="2.1.0"
```

Etki düşük (host paketlenmiyor — RULE-0001) ama **sessiz sürüm sürüklenmesi** riski taşır.
İki sürüm `common.props`'a değişken olarak taşınır; csproj değişkeni kullanır. Değişken adı
**depodaki mevcut kalıbı izler** — yeni bir adlandırma şeması icat etme, `common.props`'a bak.

### 2.4 `SchemaName` drift testinin kapsamı daraltılır — **seçenek (a)**

`AUDIT-0003` BULGU-13. ADR-0018 yasağı *"köprü sözlüğünde `SchemaName` adında alan bulunmamalı"*
diye **geniş** yazılmış; uygulama **dar** yorumlamış:

| Tip | `SchemaName` | Değerlendirme |
|---|---|---|
| `PtnLocation` | ❌ yok — `ApiSchemaName`/`DbSchemaName`/`DbTableName` ayrı | ✅ ADR'nin asıl koruduğu yer |
| `PtnCheckerTableDescription` · `PtnDatabaseAssertionRequest` · `PtnDatabaseAssertionSignal` | ✅ var | tek yönlü, yalnız DB tarafına giden modeller |

ADR'nin gerçek amacı **iki anlamın aynı tipte çakışmasını** engellemekti; bu yalnız
`PtnLocation`'da anlamlıydı. Kalan üç tip tek yönlüdür ve ad hizalaması Mapperly'yi
`[MapProperty]`'siz tutuyor — yani mevcut kod **mapper saflığı kuralını koruyor.**

**Karar: seçenek (a).** Kod olduğu gibi kalır; **drift testi yalnız konum ve rapor tiplerini
tarar.** Test kapsamı ADR metniyle **birebir** aynı olur.

> ADR-0018 metninin daraltılması **KBP-97'ye** aittir (doküman işi). Bu görev yalnız **testin
> kapsamını** düzeltir. İkisi ayrı commit, ayrı dal — test ile metin arasında geçici bir fark
> kalması normaldir ve raporda bildirilir.

---

## 3. Dilimler ve dosya manifestosu

### Dilim 1 — Marker sınıfı kaldır (≈4 dosya)

| # | Dosya | Değişiklik |
|---|---|---|
| 1 | `Application/Services/Runs/HarArtifactContainer.cs` | **silinir** |
| 2 | `Application/Services/Runs/HarArtifactService.cs` | `IBlobContainerFactory` + adla çözüm (§2.2) |
| 3 | `Application/TestModuleApplicationModule.cs` | **yalnız gerekirse** — önce doğrula |
| 4 | HAR artefakt testi | Save → Read → Delete turu **adlandırılmış container'a karşı** hâlâ yeşil |

`HarArtifactService`'in mevcut testi yoksa **bir tane yazılır**: aynı container adına yazılan
artefakt geri okunuyor ve siliniyor. Davranışın değişmediğinin kanıtı budur.

**Commit:** `#KBP-99 refactor: replaced the har blob marker container with named container resolution`

---

### Dilim 2 — Host sabit sürümlerini `common.props`'a taşı (≈2 dosya)

| # | Dosya | Değişiklik |
|---|---|---|
| 5 | `ptn-test-module/common.props` | İki Serilog sürümü için değişken (mevcut adlandırma kalıbıyla) |
| 6 | `host/Ptn.TestModule.HttpApi.Host.csproj` | İki `Version="..."` → değişken referansı |

**Kabul:** `dotnet build Ptn.TestModule.slnx -m:1` → **0 hata, 0 yeni uyarı**; çözülen Serilog
sürümleri değişiklikten **önceki** ile birebir aynı (`dotnet list package` ile doğrula, varsayma).

**Commit:** `#KBP-99 chore: moved the host serilog package versions to common props`

---

### Dilim 3 — Drift testi kapsamı (≈2 dosya)

| # | Dosya | Değişiklik |
|---|---|---|
| 7 | `test/Ptn.TestModule.Domain.Tests/Bridge/VocabularyDriftTests.cs` | `SchemaName` taraması yalnız konum + rapor tiplerinde koşar (§2.4) |
| 8 | Aynı dosya | Daraltmanın **gerekçesini** taşıyan test adı/yorumu; ADR-0018'e atıf |

**Yasak:** Testi silmek, `Skip` etmek veya assertion'ı zayıflatıp yeşile almak. Kapsam daralır,
**sıkılık daralmaz** — kapsam içindeki tipler için kural aynen sert kalır.

**Commit:** `#KBP-99 test: scoped the bridge naming drift check to the location and report types`

---

## 4. Kesme bölgesi

Bütçe aşılırsa **Dilim 3** bir sonraki göreve devredilir — kayıtlı bir denetim bulgusudur,
kaybolmaz. Kesilmeyecekler: **Dilim 1 ve Dilim 2'nin tamamı.**

---

## 5. Yasaklar

1. `HarArtifactService`'in **davranışını** değiştirme — yalnız container çözüm yolu değişir.
2. Bu görevde container'a **TTL/provider konfigürasyonu ekleme** — o TM-15'tir.
3. Yeni proje, yeni katman, `Infrastructure/`, `Providers/`, `Factories/` açma.
4. Yeni sürüm değişkeni **adlandırma şeması** icat etme — `common.props`'taki kalıbı izle.
5. Serilog sürümlerini **yükseltme** — taşıma işidir, güncelleme değil.
6. Drift testini **silme, `Skip` etme veya zayıflatma** (§Dilim 3).
7. ADR-0018 **metnini** bu dalda değiştirme — o KBP-97'nin işi (§2.4 notu).
8. Migration üretme — **bu görev şema değiştirmez.**
9. `KBP-95` / `KBP-100` dallarına commit atma; force-push, rebase, amend.
10. Ara dilimlerde build/test atlama — **her dilim yeşil kapanır.**

---

## 6. Kabul kriterleri

- `HarArtifactContainer.cs` **yok**; `IBlobContainer<T>` kullanımı depoda **sıfır**.
- Container adının tek sahibi `HarArtifactConsts.ContainerName`; ad hiçbir yerde tekrarlanmıyor.
- HAR Save → Read → Delete turu yeşil; artefakt davranışı **değişmemiş**.
- `host/*.csproj` içinde **sabit sürüm kalmadı**; çözülen sürümler değişiklik öncesiyle aynı.
- Drift testinin kapsamı ADR-0018'in koruduğu tiplerle **birebir**; kapsam içi sıkılık aynı.
- `dotnet build Ptn.TestModule.slnx -m:1` → 0 hata; `dotnet test` → 0 başarısız.
- Migration **üretilmiyor**.

---

## 7. Bitiş

1. §5'in 10 maddesini kendi kodunda tek tek kontrol et.
2. Üç dilimi sırayla commit et.
3. Tek sefer: `dotnet build Ptn.TestModule.slnx -m:1` → `dotnet test Ptn.TestModule.slnx --no-restore`
4. `/backend-verify` gate'i; her commit öncesi başlık doğrulayıcısını koştur:
   `check-backend-diff.ps1 -CommitMessage "<tam başlık>"`
5. Raporda: dosya listesi, çözülen Serilog sürümlerinin öncesi/sonrası, drift testinin eski ve
   yeni kapsamı, KBP-97'ye bırakılan ADR-0018 metin borcu, yaptığın **her varsayım**.

**Komut hijyeni:** build/test'e ≥ 600000 ms timeout, `-m:1`; kilit hatasında
`dotnet build-server shutdown` → bir kez; döngüde tekrar etme.

---

## 8. Kapattığı wiki borcu

| Kayıt | Madde |
|---|---|
| `AUDIT-0003` BULGU-11 | Host csproj sabit sürüm |
| `AUDIT-0003` BULGU-13 | `SchemaName` kapsamı — **test tarafı** |
| `PLAN-0005 §4` | KBP-99'un tamamı |
| Kullanıcı talebi (2026-08-15) | `HarArtifactContainer` marker sınıfının kaldırılması |

## 9. Bu görevde olmayan iş

| Ne | Nereye |
|---|---|
| ADR-0018 metninin daraltılması | **KBP-97** |
| BLOB TTL, saklama, parçalı silme | TM-15 |
| Vault paketleme borçları | **KBP-98** |
| Serilog sürüm yükseltmesi | Ayrı bakım işi |

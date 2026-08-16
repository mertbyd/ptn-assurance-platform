# Hata kataloğu — bu evde gerçekten yaşanmış hatalar

Her madde **canlıya çıkmış veya bir turu boşa harcamış** gerçek bir olaydır.
Bir şey beklenmedik davrandığında önce burayı tara: belirti → sebep → düzeltme.
Düzeltme **her zaman tek merkezdedir**; aynı hatayı ikinci bir yerde yamamak
ihlaldir ([kanonik checker kuralları](../../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#13-checker-gelistirme-kurallari)).

---

## 1. Bileşen DI'da hiç görünmüyor, resolver "desteklenmiyor" fırlatıyor

**Belirti:** Yeni format okuyucusunu yazdın, kaydettin, ama
`SpecFormatComponentResolver<T>` "desteklenmeyen format" diyor.

**Sebep:** Sınıf adı, arayüz adının `I`'siz hali ile **bitmiyor**. Conventional DI
bileşeni o arayüz altında hiç açığa çıkarmıyor.

**Düzeltme:** `OpenApi31SpecReader` → `ISpecReader` ✅ ·
`OpenApi31Reader` → `ISpecReader` ❌

dbchecker'da 2026-07-08'de canlı hata. Sessiz başarısızlık olduğu için en pahalı
tuzak: derleme geçer, test geçmez, sebep görünmez.

---

## 2. Pasife çekme başarılı ama endpoint 404 dönüyor

**Belirti:** `POST {id}/passivate` kaydı gerçekten pasifleştiriyor ama yanıt 404.

**Sebep:** Kod "kaydet, sonra id ile tekrar oku" yapıyor. `IPassivable` global
filtresi **varsayılan açık** olduğu için kaydedilen satır kendi okuma sorgusuna
görünmüyor.

**Düzeltme:** Yeniden okuma. Zaten yüklü entity'yi eşle:

```csharp
var saved = await Repository.UpdateAsync(entity, autoSave: true);
return Mapper.MapToDto(saved);      // ✅
// return await GetAsync(id);       // ❌ 404
```

dbchecker'da canlı bug olarak çıktı, testle sabitlendi.

---

## 3. Repository'ye `.Where(x => x.IsActive)` "geri eklemek"

**Belirti:** İnceleyen kişi filtrenin eksik olduğunu sanıyor ve ekliyor.

**Sebep:** `IPassivable` global filtresi zaten uyguluyor. Elle yazılan filtre,
elle `IsDeleted` filtresi yazmak kadar gereksiz — ve `IDataFilter.Disable()` ile
bilinçli olarak pasif okuma yapan akışları **sessizce bozuyor**.

**Düzeltme:** Filtrenin **yokluğu doğru koddur**. Pasif satır okumak gerekiyorsa
`IDataFilter<IPassivable>.Disable()`; **asla** `IgnoreQueryFilters()`.

dbchecker'da bir inceleyen 2026-07-16'da tam olarak bunu yanlış okudu.

---

## 4. Migration'a beklenmeyen tablo sızması

**Belirti:** `Up()` içinde dış sistemin okuma modeline ait tablolar CREATE ediliyor.

**Sebep:** Dış kaynağı okumak için tanımlanan EF read-model'leri uygulama modeline
karışmış.

**Düzeltme:** **Her EF model değişikliğinden sonra migration'ı üret ve OKU.**
Uygulamanın sahibi olmadığı hiçbir tablo `Up()` içinde CREATE edilmemeli.
dbchecker'da 2026-07-08'de canlıya çıktı.

---

## 5. Liste endpoint'i devasa JSON gövdelerini çekiyor

**Belirti:** Liste sorgusu yavaş; bellek şişiyor.

**Sebep:** Owned JSON kolonları (bulgular, rapor) liste yolunda materyalize
ediliyor.

**Düzeltme:** Liste ve özet yolları **yalnız başlık projeksiyonu** kullanır
(`select new …HeaderModel { … }`); gövdeyi yalnız detay yolu çeker. Gövdeyi çeken
bir liste yolu **eklenmez**.

---

## 6. Sır yanıtta veya logda görünüyor

**Belirti:** Kimlik bilgisi veya secret yolu DTO'da / log satırında.

**Sebep:** Yanıt DTO'su entity'den otomatik eşlenmiş, secret alanı da gelmiş.

**Düzeltme:** Sır alanları **yalnız istek DTO'sunda** bulunur, yanıt DTO'sunda
**asla**. Secret yolu da dışa verilmez. Yeni bir yanıt DTO'su yazarken alan alan
kontrol et.

---

## 7. Silme deseni jenerik altyapıyı da yuttu

**Belirti:** Alan temizliği sonrası derleme, kaldırılmaması gereken bir taban
sınıfın kaybolduğunu söylüyor.

**Sebep:** `*Lookups*` gibi yol desenleriyle toplu silme, aynı klasördeki
**jenerik** dosyaları da kapsıyor.

**Düzeltme:** Toplu silmede **koruma listesi** (keep-list) kullan, sonra
`git checkout HEAD -- <yol>` ile geri al. KBP-605'te üç kez oldu:
`LookupCreateModel`, `LookupUpdateModel`, `ApiContractCheckerPermissions.Lookups`.

---

## 8. `head` ile boru hattı silme döngüsünü sessizce kesiyor

**Belirti:** Toplu silme "bitti" diyor ama dosyaların bir kısmı duruyor.

**Sebep:** `find … | while read … | head -N` — `head` boruyu kapatınca döngü
SIGPIPE ile ölüyor; çıktı kırpılmış görünüyor ama **iş de yarım kalıyor**.

**Düzeltme:** Toplu işlem yapan döngüyü asla `head`'e boru etme. Say, sonra
göster: `n=$((n+1))` + sonda `echo`.

---

## 9. Büyük/küçük harf duyarsız dışlama fazlasını siliyor

**Belirti:** Klonlamadan sonra derleme, olması gereken kaynak dosyaların yok
olduğunu söylüyor.

**Sebep:** `robocopy /XD secrets` Windows'ta harf duyarsız çalışır ve katman içi
`Secrets/` **kod** klasörlerini de eler.

**Düzeltme:** Dışlamayı **tam yol** ile ver, çıplak klasör adıyla değil.
KBP-601'de 7 dosya kaybedildi.

---

## 10. Testler ortam sırrı olmadan hiç ayağa kalkmıyor

**Belirti:** `dotnet test` → `ArgumentNullException('vaultToken')`, tek test bile
koşmuyor.

**Sebep:** Test modülü başlatılırken seed akışı gerçek Vault'a dokunuyor.

**Düzeltme:** Test altyapısı dış sır deposuna **bağımlı olmamalı**. Bu madde
KBP-605'te açık; kapanmadan dal merge edilmez.
[kanonik araştırma sentezi](../../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#14-global-arastirma-sentezi) kaydı vardır.

---

## 11. Host çalışırken "derleme hatası" sanılan dosya kilidi

**Belirti:** MSB3021 / MSB3027, HttpApi.Host üzerinde.

**Sebep:** Host çalışıyor, çıktı dosyası kilitli.

**Düzeltme:** Bunlar derleme hatası **değildir**. Host'u durdur, tekrar derle.
Ayrıca host'u sandbox **dışında** çalıştır (`--urls http://localhost:5000`);
sandbox DataProtection-Keys'i engeller ve host sessizce çıkar.

---

## 12. Elle eşleme yeni alanı sessizce düşürüyor

**Belirti:** Yeni eklenen alan API yanıtında hep `null`.

**Sebep:** Bir yerde `new SomeDto { … }` elle yazılmış; alan eklenince orası
güncellenmemiş. Derleyici uyarmaz.

**Düzeltme:** Eşlemenin sahibi Mapperly'dir. Elle eşleme yalnız üç yerde meşru:
EF projeksiyonu, test doğrulaması, dış ham yükün ayrıştırılması.

---

## 13. Bayat yorumu gerçek sanmak

**Belirti:** Bir kararı koddan değil yorumdan okuyup yanlış sonuca varmak.

**Sebep:** dbchecker'da `DatabaseCheckerDbContext` ve `ComparisonScopeRule`
yorumları "kapsam kuralları `ComparisonDefinition`'a owned JSON olarak gömülür"
diyor. **Gerçek bu değil:** `20260717070022_Kbp46RemovePersistedScope`
migration'ı o kolonları düşürmüş, entity'lerde kapsam alanı yok. Kod ilerlemiş,
yorum kalmış.

**Düzeltme:** Bir kararın ne olduğunu ararken sıra: **entity → EF configuration →
migration**. Yorum en son bakılacak yerdir ve tek başına kanıt değildir.
2026-08-02'de bir tasarım turunu boşa harcadı.

---

## 14. PowerShell 5.1 `Set-Content -Encoding utf8` BOM ekliyor

**Belirti:** Türkçe karakterler `â€”`, `Ä±` gibi görünüyor; `validate_brain.py`
"missing YAML frontmatter" diyor.

**Sebep:** Windows PowerShell 5.1'de `-Encoding utf8` **BOM'lu** yazar; doğrulayıcı
frontmatter'ı `\A---` ile arar, `﻿---` eşleşmez. İçerik ayrıca çift-kodlanabilir.

**Düzeltme:** Markdown/kaynak dosyasını PowerShell ile **yazma** — Write/Edit
aracını kullan. Zorunluysa
`[IO.File]::WriteAllText($path, $text, (New-Object System.Text.UTF8Encoding($false)))`.

---

## 15. Commit yanlış dala düşüyor

**Belirti:** `git log` bir dalda başka görevin commit'lerini gösteriyor.

**Sebep:** Kabuğun çalışma dizini turlar arasında korunuyor ama **aktif dalın
korunduğu varsayılıyor**. Commit'ten önce dal doğrulanmamış.

**Düzeltme:** Her commit'ten **önce** `git branch --show-current`. Yanlış dala
düştüyse: doğru dala geç → `git checkout <sha> -- <yol>` ile içeriği al → orada
commit et → yanlış dalı `git branch -f <dal> <dogru-sha>` ile geri al.
2026-08-02'de oldu.

---

## 16. Kelime taraması temiz ama eski alan kavramı sistemde yaşamaya devam ediyor

**Belirti:** Silinen alanın ana klasör ve tip adları bulunmuyor; buna rağmen eski
model yorumlarda, ayar anahtarlarında, proje klasör girdilerinde veya kanonik
dokümanlarda hâlâ sistem gerçeği gibi anlatılıyor.

**Sebep:** Temizlik yalnız `Comparison` gibi bilinen kelime ve yol adlarıyla
doğrulanmış; kavramın sistemde üstlendiği roller (alıcı ilişkisi, saklanan rapor,
kapsam anlık görüntüsü, satır karşılaştırma ayarı gibi) yeni veri modeliyle tek tek
karşılaştırılmamış. Kelime değişince tarama geçiyor, anlam borcu kalıyor.

**Düzeltme:** Önce uygulanabilir veri modelinden **kavram envanteri** çıkar:
var olanlar, yeniden adlandırılanlar ve bilinçli olarak kaldırılanlar. Sonra her
kavramı tip adıyla sınırlı kalmadan yorum, ayar, izin, `.csproj` klasör girdisi,
DTO, sabit, seed, EF yapılandırması ve current/rule sayfalarında ara. Son kapı,
"eski kelime kaldı mı" değil, "her kalan cümle yeni modelde hâlâ doğru mu"
sorusudur.

KBP-605/606 devrinde ilk tarama `Comparison` klasörünü zararsız saydı; iki uyarı
sonrası yapılan kavram taraması `ComparisonRecipient` yorumu, `DataComparison`
ayarı, `FluentValidation\Comparisons` proje girdisi ve bayat model anlatımlarını
ortaya çıkardı. Bir tasarım turunu boşa harcadı.

---

## 17. EF integrated testleri tek başına geçiyor, birlikte `database is locked` veriyor

**Belirti:** Yeni ABP EF Core test sınıfları tek tek geçiyor; proje topluca koşunca bazı
sınıflar daha kurulurken SQLite `Error 5: database is locked` ile düşüyor.

**Sebep:** İki ayrı çakışma aynı belirtiyi üretir. xUnit farklı ABP test uygulamalarını
paralel başlatabilir. Paralellik kapalıyken de test modülü tek açık `SqliteConnection`
nesnesini bütün DbContext örneklerine verirse EF Core 10 her relational connection
kurulumunda SQLite fonksiyonlarını aynı native bağlantıya kaydetmeye çalışır ve seed
veya tenant çözümleme sırasında rastlantısal kilit oluşur.

**Düzeltme:** EF integrated test sınıflarını `DisableParallelization = true` taşıyan
tek bir xUnit collection altında seri çalıştır. Inherited ABP test sınıflarında bu
collection tek başına deterministik değilse **yalnız EF integration test assembly'sine**
`[assembly: CollectionBehavior(DisableTestParallelization = true)]` ekle ve runner'ın
bu ayarı atlayabildiği koşullar için aynı test projesindeki `xunit.runner.json` içinde
`parallelizeTestCollections: false` + `maxParallelThreads: 1` değerlerini sabitle.
Buna rağmen kilit sürerse her test uygulamasına benzersiz adlı
`Mode=Memory;Cache=Shared;Pooling=False` veritabanı ver: tek keeper bağlantı yalnız
veritabanını yaşatsın, `UseSqlite(connectionString)` ile her DbContext kendi bağlantısını
açsın ve keeper uygulama kapanışında dispose edilsin. Domain/saf unit testlerin
paralelliğini global olarak kapatma. KBP-606 veri modeli testleri eklenirken
2026-08-02'de başladı; KBP-607'de seri ayarlara rağmen tekrarlandığı için shared-memory
keeper düzeniyle beş ardışık 18/18 koşuda doğrulandı.

---

## 18. Tek kullanımlı EF literali alan sözleşmesi sayılmıyor

**Belirti:** Veri modeli doğru kuruluyor fakat `ToJson("findings")` gibi bir kolon
adı EF configuration içinde çıplak string olarak kalıyor; aynı projedeki rota,
Swagger, yapılandırma ve seed stringleri de kullanım yerlerine dağılabiliyor.

**Sebep:** Tarama tekrar sayısına odaklanıp yalnız bir kez görülen stringi zararsız
saymış; eski RULE-0001 metni de tek EF eşlemesi için istisna tanımıştı. Kullanım
sayısı ile sözleşme sahipliği birbirine karıştırıldı.

**Düzeltme:** Elle yazılmış bütün `src/` ve `host/` C# kodunu kavramsal sahiplik
açısından tara. EF kolon adı/tipi, rota, Swagger grubu ve configuration key tek
kullanımlı olsa bile uygun `Domain.Shared` sahibine taşınır. Generated migration
ve designer çıktısına elle dokunma; test senaryo verisini ürün sözleşmesinden ayır.
Repository `backend-verify` desenini de aynı ihlalin yeniden girişini yakalayacak
şekilde güncelle. KBP-606 son kontrolünde kullanıcı `ToJson("findings")` satırını
işaretlediğinde proje genelinde ortaya çıktı ve ek düzeltme turu gerektirdi.

---

## 19. AppService base akışını yeniden kurup sorumlulukları yığıyor

**Belirti:** Yeni ana varlık servisi en yakın kardeş `EntityCrudAppServiceBase`
kullanmasına rağmen `EntityReadAppServiceBase`'ten türetiliyor; validator, UOW,
load/persist ve mapping akışı concrete serviste yeniden yazılıyor. Dış işbirlikçi
çağrıları, recovery ve UOW ayrıntıları sahiplik analizi yapılmadan aynı akışa
yığılıyor. Mapper'da tanı görülmeden toplu `MapperIgnoreSource` ekleniyor.

**Sebep:** “En yakın kardeşi oku” adımı somut kanıt kapısına bağlanmadı; agent sınıf
bildirimini, base hook'larını ve mapper tanılarını yan yana karşılaştırmadan yeni
akış tasarladı. Aggregate constructor'ı generic base'i bırakma gerekçesi sanıldı;
servisin use-case adımları ile her adımın sahibi çıkarılmadı; genel güvenlik sezgisi
yerel davranış sözleşmesinin önüne geçirildi. İlk skill düzeltmesi de kök nedeni
“temiz servis ve sorumluluk sahipliği” yerine teknoloji adı yasağına çevirerek aynı
hatayı başka biçimde tekrarladı.

**Düzeltme:** Koddan önce kardeşin class declaration + base + override hook + use-case
adımları + collaborator sahipliği + RMG çıktısını çıkar. Davranış aynıysa aynı base'i
kullan; ikinci aggregate base'in entity kurma varsayımına uymuyorsa mevcut base'e
aggregate-safe `BuildEntity` hook'u ekleyip bütün tüketicileri taşı. Shape validator
base/AppService sınırında, DB/business validasyonu küçük manager metotlarında kalır.
Her bağımlılığı adıyla
değil use-case'teki sorumluluğu, repo bağımlılık yönü ve katmana sızdırdığı ayrıntı
üzerinden değerlendir; teknoloji blacklist'i kurma. Mapper ignore yalnız attributesiz
build'in somut RMG tanısıyla eklenir. Yerel
gereksinim/Wiki/kardeşte olmayan recovery semantiği eklenmez; kural çatışması varsa
önce bildirilir. KBP-607 incelemesinde 2026-08-02'de yaşandı ve servis 236 satır
yerine mevcut base ve temiz orkestrasyon yaklaşımıyla yeniden kuruldu.

---

## 20. Doğru port adapteri iki tur boyunca yanlış katman sanılıyor

**Belirti:** Aynı doğru tasarım iki tur boyunca “yanlış katman” diye tartışılıyor;
Domain portu ve `.EntityFrameworkCore` içindeki implementasyonu taşınmak veya silinmek
isteniyor.

**Sebep:** Katman haritası `.EntityFrameworkCore` projesinin bu ABP şablonunda
altyapı katmanı olduğunu hiçbir yerde söylemiyor. Projenin adı EF diyor, rolünün
repository, secret store ve HTTP gibi Domain portlarını uygulayan infrastructure
olduğu görünmüyor.

**Düzeltme:** Altyapı rolünü ARCH-0001 ve RULE-0003'te açıkça yaz; Domain'de
tanımlanan port implementasyonlarının `.EntityFrameworkCore` içindeki yerini resmî
ABP dayanağıyla SOURCE-0001'de tut. KBP-608 incelemesinde 2026-08-02'de yaşandı ve
iki tur harcandı.

---

## 21. Guard, üçüncü tarafın spec kalitesini bizim reddetme sebebimiz sanıyor

**Belirti:** Admin UI "Anlık sözleşmeyi aç" dediğinde 403
`ApiContractChecker:SpecSnapshot:DocumentInvalid`. URL, auth ve erişim sağlam;
doküman indiriliyor ama snapshot hiç açılmıyor. Kaynağın **hiçbir** dokümanı
alınamıyor, yani servis izlenemiyor.

**Sebep:** `SpecDocumentReader` varsayılan `ValidationRuleSet` ile okuyor ve
`diagnostic.Errors.Count > 0` görünce reddediyordu. Varsayılan ruleset üçüncü
tarafın **stil** ihlallerini de `Errors`'a yazar. Canlı ptn-payment-auth-api-dev
dokümanında ölçülen beş hata: bir `PathMustBeUnique` (`/api/notification-template/{id}`
ile `/api/notification-template/{notificationTemplateId}` aynı imzaya çözülüyor) ve
dört `KeyMustBeRegularExpression` (ABP generic DTO şema adları
``PagedResultDto`1[[...]]`` component key regex'ine uymuyor). Doküman **tam**
ayrıştırılmıştı: 76 path, 140 şema, 0 uyarı, 0 okuyucu hatası. Yani guard'ın
gerekçesi olan "kısmi çözülme" hiç yoktu. ABP + Swashbuckle üreten her servis bu
ihlalleri üretir; guard ürünü kendi hedef kitlesine karşı kullanılamaz yapıyordu.

**Neden basit tip ayrımı çözmez:** "validator hatasını yok say" yetmez — eksik
`info.version` (`InfoRequiredFields`) da stil ihlali de aynı `OpenApiValidatorError`
tipidir. Ayrım tipte değil, **ruleset'in çalışıp çalışmamasındadır**.

**Düzeltme:** Okuyucu `RuleSet = ValidationRuleSet.GetEmptyRuleSet()` ile çalışır;
`Errors` yalnız okuyucu düzeyinde kalır. Reddetme sınırı okunabilirliktir:
doküman üretilemezse `ParseFailed`, üretildiği hâlde okuyucu hatası varsa
`DocumentInvalid`, spec sürümü tanınmazsa `UnsupportedFormat`. Sonuncusu
`Errors`'a düşmez, `OpenApiUnsupportedSpecVersionException` **fırlatır** —
yakalanmazsa HTML hata sayfası 500 olur; bu, repodaki sanctioned try/catch
noktasıdır.

Genel kural: bu ürün üçüncü taraf dokümanlarını **gözler, denetlemez**. Gözlenen
servisin kendi kalite ihlali bizim guard'ımız değildir. KBP-618 sonrası
2026-08-05'te canlı hata olarak çıktı.

---

## 22. Host kalkmıyor, konteyner "healthy" — Vault mühürlü

**Belirti:** Reboot sonrası host ayağa kalkmıyor, seed hatası gibi görünüyor.
`docker ps` Vault'u **healthy** gösterdiği için sebep Vault sanılmıyor ve saatler
appsettings/seed/migration tarafında aranıyor.

**Sebep:** Kalıcı (file storage) Vault, konteyner her yeniden başladığında
**mühürlü** gelir. Healthcheck bunu bilerek tolere eder (`vault status` mühürlüyken
2 ile çıkar, healthcheck bunu başarı sayar) — yani "healthy" **açık demek değildir**.
Mühür yalnız restart'ta geri geldiği için haftalarca hiç karşılaşılmaz; ilk reboot
gününde aniden çıkar ve yeni bir hata gibi görünür.

**Düzetme:** Host kalkmıyorsa **ilk bakılacak yer** budur:

```bash
docker exec api-contract-checker-vault-dev vault status
```

`Sealed true` ise başka hiçbir yere bakma. 2026-08-05'ten beri yerelde dev-mode
Vault (`--profile local-vault-dev`) kullanılıyor ve mühür ritüeli kaldırıldı;
gerekçe ve geri alma koşulu `vault/README.md`'de. Kalıcı Vault'a dönülürse unseal
key **parola yöneticisine** alınır — kaybolursa `vault_data` bir daha açılamaz,
bu makinede tam olarak bu oldu.

2026-08-05'te bir turu boşa harcadı.

---

## Kataloğa madde ekleme

Bir hata **iki kez** olduysa veya **bir turu boşa harcadıysa** buraya yazılır.
Biçim: belirti → sebep → düzeltme → nerede yaşandı. Tahmin yazma; yalnız gerçekten
olan.

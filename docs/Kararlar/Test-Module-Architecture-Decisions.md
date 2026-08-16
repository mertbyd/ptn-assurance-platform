# Test Module — Mimari Karar Dokümanı

| | |
|---|---|
| **Doküman** | Test Module mimari kararları, veri yapıları ve teknoloji seçimleri |
| **Sürüm** | 1.0 |
| **Tarih** | 13 Ağustos 2026 |
| **Durum** | Onaya sunuldu |
| **Kapsam** | Veri modeli, modül entegrasyonu, standart seçimleri, yapay zekâ katmanı sınırları |
| **Ekler** | `Test-Module-Database-Schema.dbml` |
| **Hedef kitle** | Yazılım mimarları, backend ekibi, test ekibi, teknik yöneticiler |

---

## İçindekiler

1. [Amaç ve kapsam](#1-amaç-ve-kapsam)
2. [Problem tanımı](#2-problem-tanımı)
3. [Sistem mimarisi](#3-sistem-mimarisi)
4. [Mimari kararlar](#4-mimari-kararlar)
5. [Kullanılan standartlar ve teknolojiler](#5-kullanılan-standartlar-ve-teknolojiler)
6. [Veri modeli](#6-veri-modeli)
7. [Modül entegrasyon modeli](#7-modül-entegrasyon-modeli)
8. [Yapay zekâ katmanı](#8-yapay-zekâ-katmanı)
9. [İncelenen referans projeler ve ürünler](#9-i̇ncelenen-referans-projeler-ve-ürünler)
10. [Ölçüm ve kabul kriterleri](#10-ölçüm-ve-kabul-kriterleri)
11. [Riskler ve karşı önlemler](#11-riskler-ve-karşı-önlemler)
12. [Açık maddeler](#12-açık-maddeler)
13. [Kaynakça](#13-kaynakça)

---

## 1. Amaç ve kapsam

### 1.1 Bu doküman neyi tanımlar

Test Module, iş senaryolarını otomatik olarak çalıştıran ve doğrulayan bir platformdur.
Bu doküman, modülün inşasından önce alınması gereken mimari kararları, bu kararların
gerekçelerini, dayandıkları kaynakları ve kullanılacak veri yapılarını tanımlar.

### 1.2 Kapsam içi

- Kalıcı veri modeli ve şema sahipliği
- Diğer modüllerle iletişim deseni
- Senaryo tanım formatı ve doğrulama katmanları
- Yapay zekâ destekli senaryo üretiminin sınırları ve güvenlik modeli
- Ölçüm ve kabul kriterleri

### 1.3 Kapsam dışı

- Kullanıcı arayüzü tasarımı
- Kimlik doğrulama ve bildirim altyapısı (ayrı yetenek modülleri)
- Kullanıcı arayüzü katmanı testleri (§4.8.3'te gerekçelendirilmiş bilinçli kapsam dışı)
- Ayrıntılı sınıf ve metot tasarımı

---

## 2. Problem tanımı

### 2.1 Mevcut durumda çözülemeyen sorun

Yazılım sistemlerinde hataların önemli bir bölümü, tek tek bileşenler doğru çalışırken
**bileşenler arası iş kuralının** ihlal edilmesinden kaynaklanır. Örnek:

> Bir bilet satın alma işlemi başarıyla tamamlanır, HTTP yanıtı sözleşmeye tam olarak
> uyar, bilet kaydı veritabanına yazılır — fakat araçtaki boş koltuk sayacı güncellenmez.

Bu hata:

- Şema doğrulamasıyla yakalanamaz (yanıt kusursuzdur)
- Kalıcılık kontrolüyle yakalanamaz (kayıt oluşmuştur)
- Yalnızca **iş değişmezi** kontrolüyle yakalanabilir

### 2.2 Test otomasyonunun ölçülmüş sorunları

Sektör ve akademik ölçümlere göre test otomasyonunun temel maliyet kalemleri şunlardır:

| Sorun | Ölçülmüş büyüklük | Kaynak |
|---|---|---|
| Bakım maliyeti | Kalite güvence eforunun %30–50'si | [K11] |
| Başarısızlık teşhis süresi | Kararsız test başına medyan 28 dakika | [K12] |
| Kararsız test maliyeti | 100 kişilik ekipte yıllık ~2,6 M$ | [K12] |
| Otomasyon girişimi başarısızlığı | ~%60 beklentiyi karşılamıyor | [K13] |

Kararsızlığın kök sebepleri sıralamasında ilk üç neden, 2014'ten 2026'ya kadar yapılan
bağımsız çalışmalarda değişmemiştir: **asenkron bekleme, eşzamanlılık, test sırası
bağımlılığı** [A7][A8]. Dördüncü sırada **oracle kırılganlığı** yer alır: incelenen 234
vakanın 40'ı (%17) geçerli çıktıyı dışlayan aşırı kısıtlayıcı beklentiden kaynaklanmıştır [A9].

### 2.3 Çözüm yaklaşımı

Test Module üç bileşenli bir mimariye dayanır: iki bağımsız doğrulama motoru ve bunları
iş senaryosuna bağlayan bir orkestrasyon katmanı. Ayırt edici özelliği, uygulama
değiştiğinde hangi senaryonun etkilendiğini **tahmin etmek yerine deterministik olarak
hesaplamasıdır**.

---

## 3. Sistem mimarisi

### 3.1 Bileşenler

```
                    ┌─────────────────────────────┐
   Kullanıcı ─────► │  Test Module                │
   (iş dili)        │  senaryo saklama, koşum,    │
                    │  kanıt, etki analizi        │
                    └──────────┬──────────────────┘
                               │ doğrulama sorusu
              ┌────────────────┴────────────────┐
              ▼                                 ▼
  ┌───────────────────────────┐   ┌───────────────────────────┐
  │ API Sözleşme Doğrulayıcı  │   │ Veritabanı Doğrulayıcı    │
  │ "yanıt sözleşmeye uyuyor  │   │ "beklenen satır oluştu    │
  │  mu?"                     │   │  mu?"                     │
  └───────────────────────────┘   └───────────────────────────┘
```

### 3.2 Bileşen sorumlulukları

| Bileşen | Sorumluluk | Durum |
|---|---|---|
| API Sözleşme Doğrulayıcı | OpenAPI dokümanı karşılaştırma, yanıt uygunluk kontrolü, dinamik teşhis | Üretimde |
| Veritabanı Doğrulayıcı | Şema/veri karşılaştırma, hedefli satır doğrulaması, dinamik teşhis | Üretimde |
| Test Module | Senaryo yaşam döngüsü, koşum, kanıt, etki analizi, bakım | Tasarım tamamlandı |

### 3.3 Üç bileşenli yapının gerekçesi

1. **Bağımsız değer.** "İki ortamın veritabanı şeması aynı mı" sorusu testten bağımsız
   bir sorudur ve tek başına değerlidir.
2. **Bağımsız sürümleme.** Test Module'deki her değişiklik doğrulama motorlarının yeniden
   yayımlanmasını gerektirmez.
3. **Tek doğruluk kaynağı.** Şema doğrulaması iki ayrı yerde uygulanırsa zamanla birbirinden
   ayrışır ve sessiz tutarsızlık üretir.

---

## 4. Mimari kararlar

Her karar; **ne yapıldığı**, **gerekçesi**, **değerlendirilen alternatifi** ve **dayandığı
kaynak** ile birlikte verilmiştir.

### 4.1 Yapay zekâ modelinin konumu

#### K-01. Model koşum döngüsünde yer almaz

**Karar.** Yapay zekâ modeli yalnızca senaryo üretimi, teşhis anlatımı ve bakım önerisi
aşamalarında çalışır. Senaryo koşumu sırasında hiçbir model çağrısı yapılmaz.

**Gerekçe.**
1. *Maliyet.* Tam ajan tabanlı koşumda test başına yaklaşık 114.000 token ölçülmüştür;
   dosya tabanlı akışta aynı iş ~27.000 token ile tamamlanmaktadır [K1].
2. *Belirlenimcilik.* Model olasılıksal çalışır; aynı senaryo iki koşumda farklı sonuç
   verebilir. Test raporunun temel gereksinimi tekrarlanabilirliktir.
3. *Süre.* Model çıkarımı adım başına saniyeler mertebesindedir.

**Alternatif.** Her koşumda ajan çalıştırmak. Reddedilmiştir: maliyet, kararsızlık ve süre
açısından savunulamaz.

**Dayanak.** Aynı ayrım, Microsoft'un Playwright test ajanlarında da uygulanmaktadır:
üç ajan (planlayıcı, üretici, onarıcı) yalnızca **üretim** aşamasındadır; üretilen test
daha sonra sıradan bir test koşucusu tarafından çalıştırılır [R1].

#### K-02. Karar mercii her zaman deterministik doğrulayıcıdır

**Karar.** "Bu sonuç doğru mu?" sorusuna model değil, doğrulama motorları cevap verir.

**Gerekçe.** Büyük dil modeli tabanlı test oracle'ları kırılgandır: prompt üzerindeki küçük
değişiklikler veya model sürüm güncellemeleri kararı değiştirebilir; gerekçe denetlenebilir
değildir [A1]. Pasif model hakemlerin düşük kesinlik ve yüksek yanlış-pozitif oranıyla
sınırlı kaldığı raporlanmıştır [A2].

**Modelin rolü.** Yalnızca hangi kontrolün yazılacağını önermek. Yazıldıktan sonra kontrol
insan onayından geçer ve deterministik olarak koşar.

#### K-03. Doğrulayıcılar hedef sisteme yazmaz

**Karar.** İki doğrulama motoru da salt-okunur çalışır. Serbest SQL kabul etmez; yalnızca
katalogda doğrulanmış nesne adları ve parametreli sorgular kullanır.

**Gerekçe.** Güvenlik literatüründe "ölümcül üçlü" olarak adlandırılan risk bileşimi
(özel veriye erişim + dış kaynaklı içerik + dışarıya veri gönderme yeteneği) bir arada
bulunduğunda sistem ele geçirilebilir hale gelir. Yazma yetkisi bu üçlünün en kritik
ayağını açar.

**Sonuç.** Test verisi hazırlama ve temizleme, ayrı ve açıkça yetkilendirilmiş bir
bağlantı üzerinden Test Module'ün sorumluluğundadır. Doğrulama motorları bu bağlantıyı
hiçbir koşulda görmez.

**Yan fayda.** Bu karar entegrasyonu da basitleştirmiştir: doğrulayıcılar yazma yapmadığı
için dağıtık işlem, saga veya telafi mekanizmasına ihtiyaç yoktur.

### 4.2 Senaryo formatı ve kimliği

#### K-04. Senaryo formatı endüstri standardıdır

**Karar.** Senaryolar Arazzo 1.1.0 formatında saklanır. Kendi tanım dili geliştirilmez.

**Gerekçe.** İhtiyaç duyulan tüm yapılar standartta mevcuttur: adım dizisi, girdi
parametreleri, adımlar arası değer taşıma, başarı ölçütleri (basit karşılaştırma, düzenli
ifade, JSONPath, XPath), sınırlı yeniden deneme, zaman aşımı, asenkron olay bekleme ve
korelasyon [S1]. Ayrıca standart format, üretici modelin eğitim verisinde bulunduğu için
her senaryo üretiminde formatın anlatılmasına gerek kalmaz.

**Eksik olan tek unsur** veritabanı adımıdır; bu, standardın kendi genişletme mekanizmasıyla
eklenmiştir (`x-` önekli uzantı alanları). Standart çatallanmamıştır.

**Alternatifler.** Kendi tanım dili (dokümantasyon, bakım ve eğitim maliyeti; dış araç
uyumsuzluğu), araç formatları (standart değildir, test mantığını betiğe gömer).

#### K-05. Senaryo kimliği kalıcıdır, türetilmez

**Karar.** Her senaryonun elle verilen, değişmeyen bir anahtarı bulunur. Kimlik ad veya
parametrelerden hesaplanmaz.

**Gerekçe.** Kimliği `md5(tam_ad + sıralı_parametreler)` biçiminde hesaplayan bir ticari
test yönetim ürünü, kendi dokümantasyonunda bu yaklaşımın sonucunu uyarı olarak
belgelemektedir: fonksiyon adı veya parametre değiştiğinde kimlik değişir, sistem kaydı
yeni bir test olarak algılar ve geçmiş veri kopar [R3].

#### K-06. Senaryo içeriği hash ile adreslenir

**Karar.** Senaryo dosyasının metni ayrı bir tabloda, SHA-256 özeti ile adreslenmiş ve
değişmez olarak saklanır. Aynı içerik ikinci kez yazılmaz.

**Gerekçe.** Sözleşme testi alanının yaygın aracı olan Pact Broker, sözleşme dosyasını
hash'leyerek tekilleştirir ve böylece aynı sözleşmeyi paylaşan sürümler için doğrulamayı
yeniden kullanır [R7]. Aynı desen senaryo dosyalarına uygulanmıştır.

**İki hash tutulur:** ham baytların özeti (bayt düzeyinde eşitlik) ve normalize edilmiş
halin özeti (anlamsal eşitlik). İkincisi, yalnızca biçimsel değişikliklerin gereksiz sürüm
üretmesini engeller.

#### K-07. Koşum kaydı senaryo sürümünü kopyalar

**Karar.** Koşum kaydı, o an geçerli olan senaryo sürümünün numarasını **kopya** olarak
saklar; yabancı anahtar kullanmaz.

**Gerekçe.** Senaryo daha sonra değiştirildiğinde geçmiş raporların hangi tanımın
koştuğunu doğru bildirmesi gerekir. Açık kaynak test yönetim sistemlerinde aynı alan
`case_text_version` adıyla mevcuttur [R5].

### 4.3 Sonuç sınıflandırması

#### K-08. Koşum sonucu altı değerlidir

**Karar.** Bir senaryonun koşum sonucu şu altı değerden biridir:

| Değer | Anlamı | Derleme sürecini durdurur mu |
|---|---|---|
| `Passed` | Ana yol koştu, tüm beklentiler tuttu | Hayır |
| `Failed` | Doğrulayıcı olumsuz cevap verdi — gerçek bulgu | **Evet** |
| `Broken` | Adım hiç çalışamadı — ortam veya altyapı sorunu | Ayrı raporlanır |
| `Skipped` | Bilinçli olarak atlandı | Hayır |
| `Quarantined` | Kararsız; koşar fakat sonucu bağlayıcı değildir | Hayır |
| `Inconclusive` | Önkoşul sağlanmadı, ana yol koşmadı | Hayır |

**`Failed` / `Broken` ayrımının gerekçesi.** Bu ayrım, teşhis motoruna hangi sinyalin
gönderileceğini belirler. Tek statü kullanıldığında gerçek hatalar ortam kaynaklı
başarısızlıkların gürültüsü altında kaybolur ve kararsızlık ölçümü kirlenir.

**`Inconclusive` statüsünün gerekçesi.** Önkoşulu sağlanmayan bir senaryo hiçbir şey
doğrulamamıştır; bu durumun "başarılı" sayılması, yanlış pozitiften daha tehlikeli bir
güven yanılsaması üretir. Literatürde bu tür sonuçlar "yanlış negatif" olarak
sınıflandırılır ve *"uygulamanın kalitesi hakkında yanlış güven aşıladıkları için çok daha
tehlikeli"* kabul edilir [K14]. Aynı literatür, beklenti içermeyen ve totolojik testleri
"işe yaramaz test" başlığında toplayarak ortak sorunu *"testin yokluğu değil, yanlış
güven"* olarak tanımlar [K15].

**Ölçüm sonucu.** Sonuçsuz koşum oranı bir sağlık göstergesidir; yükselmesi test ortamı
verisinin bozulduğunu veya önkoşul stratejisinin yanlış seçildiğini gösterir.

### 4.4 İş senaryosu yetenekleri

#### K-09. Koşullu akış birinci sınıf kavramdır

**Karar.** Senaryolar karar noktası içerebilir; seçilen dal koşum kaydına yazılır.

**Gerekçe.** İş senaryolarının çoğu koşulludur ("belirtilen saatte bilet varsa satın al").
Koşulun adım içine gizlenmesi, test kokusu olarak sınıflandırılan bir kalıptır [A14].
Karar noktası ayrı ve görünür bir adım olarak modellenir.

#### K-10. Önkoşul birinci sınıf kavramdır

**Karar.** Senaryo, çalışabilmesi için gereken başlangıç durumunu açıkça bildirir.
İki strateji desteklenir:

| Strateji | Yöntem | Sağlanamazsa |
|---|---|---|
| `Arrange` | Veri test ortamında üretilir | `Broken` |
| `Discover` | Mevcut veriden aranır | `Inconclusive` |

Kullanılan strateji koşum kaydına yazılır.

**Gerekçe.** Test verisi yönetimi, kurumsal test otomasyonunun en sık raporlanan darboğazıdır
[K16]. Önkoşulun açık olmaması, "test geçti" ile "test bir şey doğruladı" arasındaki farkı
gizler.

#### K-11. İş kuralları değişmez kalıplarıyla doğrulanır

**Karar.** İş kuralı doğrulaması, tek bir beklenen değer yerine **ölçümler arası ilişki**
üzerinden yapılır. On kalıp tanımlanmıştır:

| Kod | Kalıp | Genel biçim |
|---|---|---|
| M-1 | Korunum | Toplam sabit kalır |
| M-2 | Fark | İşlem değeri tam N kadar değiştirir |
| M-3 | Tutarlılık | İki kaynak aynı değeri bildirir |
| M-4 | Tekillik | Aynı kaynak iki kez tahsis edilemez |
| M-5 | Gidiş-dönüş | Oluştur → oku → aynı veri |
| M-6 | Etkisizlik | Aynı işlem iki kez → ikinci reddedilir veya aynı sonuç |
| M-7 | Tek yönlülük | Değer yalnızca bir yönde değişir |
| M-8 | Olumsuz yol | Geçersiz istek reddedilir **ve durum değişmez** |
| M-9 | Yetki sınırı | Başka kiracının kaydına erişilemez |
| M-10 | Durum geçişi | Yalnızca izinli geçişler gerçekleşir |

**Gerekçe.** Bu yaklaşım literatürde "metamorfik test" olarak tanımlanır ve doğrudan
oracle problemini hedefler: *"Metamorfik ilişki oracle'ı tek bir beklenen çıktı değildir;
iki gözlenen çıktı arasındaki ilişkidir."* REST servisleri için altı soyut ilişki kalıbı
tanımlanmıştır [A3]. 2026'da yayımlanan çalışmalar, bu ilişkilerin OpenAPI dokümanından
model desteğiyle çıkarılabildiğini göstermektedir [A4].

**M-8'in özel önemi.** Yaygın uygulamada olumsuz senaryolar yalnızca hata kodunu kontrol
eder. Asıl doğrulanması gereken, işlemin **yan etki bırakmadığıdır**.

**Teknik ön koşul.** Bir adımda ölçülen sayısal değerin sonraki adımda kullanılabilmesi
gerekir. Bu, koşum motorunun adım çıktısı taşıma yeteneğiyle karşılanır.

### 4.5 Veri modeli ilkeleri

#### K-12. Ayrı tablo kuralı

**Karar.** Bir kavram, aşağıdaki üç ölçütten **en az biri** sağlanıyorsa ayrı tablo olur;
aksi hâlde parent kaydında owned JSON kolonu olarak taşınır.

1. Başka bir tablodan yabancı anahtar ile gösteriliyor
2. Parent kaydından bağımsız sorgulanıyor
3. Parent kaydından bağımsız tekilleştiriliyor

**Sonuç.** İlk taslaktaki 28 tablo dokuza inmiş; yaklaşık 20 gereksiz kiracı kolonu ve
20 ayrı migration yüzeyi ortadan kalkmıştır.

**Dayanak.** Aynı desen mevcut doğrulama modüllerinde uygulanmaktadır: karşılaştırma
bulguları ve rapor içerikleri ayrı tablolar yerine koşu kaydının JSON kolonlarında
taşınmaktadır.

#### K-13. Adım sonuçları koşum kaydının içindedir

**Karar.** Adım adım sonuçlar ayrı tablo değil, koşum kaydının JSON kolonudur.

**Gerekçe ve sonuçları.**

| Kazanç | Karşılığı |
|---|---|
| Satır sayısı bir kat küçülür (500 senaryo × 10 adım → 500 satır) | Çapraz koşum analitiği JSON sorgusu gerektirir |
| Bölümleme (partition) ihtiyacı ortadan kalkar | Satır boyutu izlenmelidir |
| Saklama politikası basit satır silmeye indirgenir | — |

**Çökme kurtarma sonucu.** Adımlar tek satırda olduğu için kurtarma **senaryo seviyesinde**
yapılır. Bu, zaten doğru olan davranıştır: yarım kalmış bir iş işleminin (örneğin
tamamlanmış bir satın alma) ortasından devam etmek veri bütünlüğünü bozar. Koşum `Broken`
işaretlenir, test verisi sıfırlanır, senaryo baştan koşar.

#### K-14. Bölümleme bu sürümde uygulanmaz

**Karar.** Zaman bazlı tablo bölümleme bu sürümde kullanılmaz. Yerine zamanlanmış parçalı
silme uygulanır (10.000 satırlık partiler, her parti ayrı işlem).

**Gerekçe.** PostgreSQL'de bölümlenmiş bir tablonun birincil anahtarı, bölümleme kolonunu
içermek zorundadır. Bu kısıt, uygulama çatısının tek kolonlu tekil anahtar sözleşmesini
kırar ve veri erişim katmanında özel kod gerektirir.

**Geçiş eşiği.** 50 milyon satır **veya** günlük silme penceresinin 10 dakikayı aşması.
Ölçüm bu eşiği gösterdiğinde ilgili tablo uygulama nesnesi olmaktan çıkarılıp özel bir
okuma modeline dönüştürülür.

#### K-15. Çok kiracılılık veritabanı katmanındadır

**Karar.** Dokuz ana tablonun tamamı kiracı kimliği taşır ve çatının global sorgu filtresi
uygulanır. On dört referans tablosu kiracı taşımaz.

**Gerekçe.** Uygulama çatısının kiracı filtresi **entity tipi bazında** uygulanır ve
miras alınmaz: kendi arayüzünü uygulamayan bir kayıt doğrudan sorgulandığında tüm
kiracıların satırlarını döndürür. Modeldeki dokuz ana tablonun tamamı doğrudan sorgu
hedefidir.

Sektör pratiği aynı yönü işaret eder: *"Doğru mimari seçim, kiracı kapsamını uygulama
katmanında değil veritabanı katmanında yapmaktır; uygulama katmanı kapsamı kiracılar arası
sızıntıya tek hata uzaklıktadır."* [K17]

**Referans tablolarının kiracı taşımama gerekçesi.** Durum ve tür değerleri tüm kiracılarda
aynıdır; kiracı başına kopyalamak veri hacmini gereksiz yere çoğaltır.

### 4.6 Modül entegrasyonu

#### K-16. Sorular doğrudan çağrı, olgular olay ile taşınır

**Karar.**

| İhtiyaç | Desen |
|---|---|
| "Bu satır oluştu mu?" · "Bu yanıt uygun mu?" · "Neden başarısız oldu?" | Doğrudan çağrı |
| "Karşılaştırma koşusu tamamlandı" | Olay |
| Diğer modülün tablosunu okumak | **Yasak** |
| Diğer modülün tablosuna yabancı anahtar | **Yasak** |
| Ortak veritabanı işlemi | **Yasak** |

**Gerekçe.** Modüler monolit literatürünün üzerinde uzlaştığı kural: *"Sorular için doğrudan
çağrı, olgular için olay; asla paylaşılan veri üzerinden entegre etme, modüller arası
anahtar, birleştirme veya işleme asla izin verme."* [K18]

Aynı kaynak iki karşıt hatayı da işaret eder: **sınırı çökertmek** (veri bağlamı, kayıt
deposu veya nesne paylaşmak) ve **aşırı telafi** (aynı süreç içinde HTTP çağrısı veya mesaj
kuyruğu kurmak). İkisi de modüler yapının amacını ortadan kaldırır.

#### K-17. Bozulma önleyici katman zorunludur

**Karar.** Test Module diğer modüllerin arayüzlerini doğrudan çağırmaz; kendi port
arayüzlerini çağırır. Uyarlayıcılar üç iş yapar: veri aktarım nesnesi çevirisi, kod
sözlüğü normalizasyonu, hata çevirisi.

**Gerekçe.** Bu, alan güdümlü tasarımın "bozulma önleyici katman" deseninin uygulanmasıdır:
*"bağlamınızı dış sistemlerin karmaşasından koruyan, iki farklı alan modeli arasında çeviri
yapan ve dış kavramların kod tabanınıza sızmasını önleyen katman"* [K19].

**Somut kazanç.** Doğrulama modüllerinden birinde bir veri aktarım nesnesi değiştiğinde
yalnızca uyarlayıcı değişir; koşum motoru, iş kuralı yöneticileri ve ajan yüzeyi etkilenmez.

#### K-18. Dış modül kimlikleri referans olarak tutulur

**Karar.** Diğer modüllerin kayıtlarına yabancı anahtar verilmez; yalnızca kimlik saklanır.
Doğrulama, bağlama kurulurken bir kez yapılır; görüntüleme için özet bilgi kopyalanır.

**Gerekçe.** Yabancı anahtar, diğer modülün tablo yapısına bağımlılık yaratır ve migration
sırası zorunluluğu doğurur. Kopyalanan özet bilgi karar için kullanılmaz; karar anında
ilgili modülün uygulama servisi çağrılır.

### 4.7 Güvenlik ve gizlilik

#### K-19. Ham veri varsayılan olarak saklanmaz

**Karar.** Kanıt kayıtları "beklenen ile gerçek" arasındaki farkın **şeklini** taşır,
değerini taşımaz. Değer saklama modu varsayılan olarak kapalıdır ve açıldığında koşum
kaydına yazılır.

**Gerekçe.**
1. *Gizlilik.* Genel Veri Koruma Tüzüğü'nün saklama sınırlaması ve veri minimizasyonu
   ilkeleri, test kayıtlarında kişisel veri tutulmasını sınırlar.
2. *Güvenlik.* Test edilen sistemin yanıtına gömülü bir talimat, model bağlamına
   girdiğinde modeli yönlendirebilir. Bu risk (istem enjeksiyonu) 2026 yılı büyük dil
   modeli güvenlik listesinde birinci sıradadır [K20].

**Standart uyumu.** Otonom sistemler için hazırlanan denetim kaydı taslağı aynı ilkeyi
zorunlu kılar: *"Ham girdi ve çıktı verisi denetim kayıtlarında saklanmamalıdır;
uygulamalar bunun yerine girdi ve çıktı özetlerini kullanmalıdır."* [S8]

#### K-20. Parolalar veritabanında tutulmaz

**Karar.** Bağlantı kayıtlarında parola yerine gizli anahtar kasasındaki **adres** saklanır.

### 4.8 Yapay zekâ katmanı sınırları

#### K-21. Dört kademeli izin modeli

**Karar.** Ajanın gerçekleştirebileceği her eylem, **geri alınabilirlik ve etki yarıçapına**
göre dört kademeden birine yerleştirilir:

| Kademe | Tanım | Gözetim |
|---|---|---|
| 1 — Salt okuma | Dış dünyada yan etkisi yok | Kesintisiz |
| 2 — Geri alınabilir | Taslak, iç durum değişikliği | Serbest, kayıtlı |
| 3 — Dış sisteme dokunan | Test ortamına veri yazma | Kuyruğa alınır veya güven sinyaline bağlanır |
| 4 — Geri alınamaz | Yayınlama, yama uygulama | **Zorunlu insan onayı** |

**Gerekçe.** Sınıflandırmanın işlem kategorisine göre değil geri alınabilirliğe göre
yapılması, üretim ortamlarında yaygınlaşan desendir. Salt okuma eylemlerinin onaya
bağlanması *"yalnızca onay yorgunluğu üretir"*; buna karşılık *"yüksek güven, bir ajana
denetimsiz geri alınamaz eylem hakkı satın almaz"* [K21].

**Kesin sınır.** Yayınlama işlemini gerçekleştirebilecek bir araç, ajanın araç kataloğunda
**bulunmaz**. Ajan yönlendirilse dahi bu eylemi gerçekleştiremez.

#### K-22. Otonomi seviyesi kiracı ayarıdır

**Karar.** Kiracılar ajanın hangi kademelere kadar otonom çalışacağını belirler:
`Observe` (yalnız 1), `Assist` (1–2, varsayılan), `Act` (1–3). Dördüncü kademe hiçbir
seviyede otonom değildir.

**Dayanak.** Üretimdeki hata ayıklama ajanlarında aynı yaklaşım uygulanmaktadır:
kuruluşlar, insan devreye girmeden önce ajanın ne kadar ileri gideceğini yapılandırabilir [R9].

#### K-23. Onay içerik özetine bağlanır

**Karar.** Bir onay kaydı; kiracı, aktör, işlem, hedef, **içerik özeti (hash)**, politika
sürümü, son kullanma tarihi ve etkisizlik anahtarı taşır. Onaylanan içerik değişirse onay
geçersiz olur.

**Gerekçe.** İnsan onayı desenlerinde temel kural şudur: *"İçerik, alıcı veya hedef
değişirse onay artık geçerli değildir."* [K22] Bu bağlama, "onay al, farklı bir şey uygula"
senaryosunu kapatır.

**Onay arayüzü gereksinimi.** Onay ekranı dört bilgiyi göstermek zorundadır: ne yapılacak,
**neden**, ne değişecek, nasıl geri alınır.

#### K-24. Kuru koşum başarısızlığı ajana düzeltme yetkisi vermez

**Karar.** Üretilen senaryo canlı sistemde bir kez denenir. Deneme başarısız olursa sonuç
ajana düzeltme girdisi olarak **verilmez**; bir çelişki bildirimi olarak insana sunulur.

**Gerekçe — ölçülmüş bulgu.** Test üreteçleri hatalı bir sistemle etkileşime girdiğinde
beklentileri **hatalı davranışa uyacak şekilde uyarlamaktadır**. Ölçümde dört model için,
hatalı implementasyona karşı iyileştirme yapmak, hiç etkileşmeden tek seferde üretmekten
**daha kötü** sonuç vermiştir [A5].

**Uygulama.** Deneme başarısız olduğunda sistem şu bildirimi üretir: *"Bu adım başarısız.
İki olasılık vardır: senaryo yanlış olabilir veya uygulamada hata olabilir. Karar insana
aittir."* Kararın nasıl verildiği senaryo sürüm kaydına yazılır.

#### K-25. Beklenti zayıflaması engellenir

**Karar.** Beklenti içermeyen adım yayınlanamaz. Beklenti sayısını azaltan veya
karşılaştırıcıyı gevşeten yama önerileri işaretlenir ve onay ekranında ayrıca uyarılır.

**Gerekçe — ölçülmüş bulgu.** Derleyici geri bildirim döngülerinde modellerin
**beklentileri kaldırarak ve boş test gövdeleri üreterek** derlenebilirlik hedefini
optimize ettiği gözlenmiştir. Bir modelde %99 derleme başarısı elde edilmiş, ancak bu
*"kodun gerçek anlamda muhakeme edildiğini yansıtmamıştır"* [A6].

#### K-26. Tur sınırı serttir

**Karar.** Ajan profilleri için sert tur sınırları tanımlanır: senaryo yazımı 8, teşhis 4,
bakım 5, sohbet 10. Sınır aşıldığında işlem **başarısız** sayılır; sessizce devam edilmez.

**Gerekçe — ölçülmüş bulgu.** Öncü modeller tek adımlı görevlerde %80–90 başarı
gösterirken, uygulamalar arası çok adımlı iş akışlarında bu oran **%18–24**'e düşmektedir.
Adım başına %85 güvenilir bir ajan, on adımlı bir zincirde uçtan uca yaklaşık %20 başarı
verir [K23].

**Tasarımın buna verdiği yanıt.** Bu sistemde ajan görevleri kasıtlı olarak kısadır
(5–8 araç çağrısı) ve uzun ufuklu planlama gerektirmez.

#### K-27. Model bir soyutlama katmanının arkasındadır

**Karar.** Model erişimi port arayüzü üzerinden sağlanır. Bu sürümde tek sağlayıcı
kullanılır; yerel model desteği uyarlayıcı olarak sonradan eklenir.

**Gerekçe.** Model seçimi ölçümle yapılmalıdır. Bir ölçümde küçük bir model, öncü bir
modele göre daha yüksek mutasyon skoru (%70'e karşı %65) ve **25 kat düşük maliyet**
($0,41'e karşı $10,13) göstermiştir [A5]. Ayrıca düzenlenmiş sektör müşterileri şema ve
iş kuralı metaverisinin dış servise gönderilmesini kısıtlayabilir.

**Ölçüm altyapısı.** Oturum kayıtlarında kullanılan model saklanır; aşama bazında kabul
oranı ve maliyet karşılaştırılabilir.

#### K-28. Denetim izi orantılıdır

**Karar.** Sonuçlu eylemlerin denetim izi alan modelinde kalıcıdır (koşuyu kimin
tetiklediği, sürümü kimin onayladığı, yamayı kimin kabul ettiği). Hash zincirli, ekle-only
denetim günlüğü bu sürümde kurulmaz.

**Gerekçe.** Yüksek riskli yapay zekâ sistemleri için otomatik olay kaydı ve en az altı ay
saklama gereksinimi mevzuatta tanımlıdır [S9]. Bu sistem geliştirici aracı niteliğindedir
ve söz konusu sınıflandırmaya girmemektedir. Yükseltme yolu belgelenmiştir: düzenlenmiş
sektör müşterisi talep ettiğinde ayrı bir karar olarak açılır.

#### K-29. Telemetri sözlüğü standarttır

**Karar.** Ajan telemetrisi için OpenTelemetry üretken yapay zekâ anlamsal konvansiyonları
kullanılır (konuşma kimliği, girdi/çıktı token sayıları, model referansı). Kendi sözlüğü
tanımlanmaz [S7].

### 4.9 Bilinçli kapsam dışı bırakılanlar

| Konu | Gerekçe |
|---|---|
| Kullanıcı arayüzü katmanı testleri | Sektör analizlerinde *"yavaş ve kırılgan test paketlerinin tek en büyük sebebi"* olarak tanımlanmaktadır [K11]. Bu sistemin doğrulama katmanı arayüz altındadır. |
| Makine öğrenmesi tabanlı test seçimi | Bu sistemde seçim problemi olasılıksal değildir: değişikliğin hangi adresi etkilediği deterministik olarak bilinmektedir. Kural tabanlı etki analizi hem açıklanabilir hem de eğitim maliyeti sıfırdır [K10]. |
| Ayrı iş akışı orkestrasyon altyapısı | İkinci bir durum sahibi yaratır ve mevcut arka plan iş altyapısıyla çözülebilen bir problemi dış bağımlılığa taşır. |
| Sütun tabanlı analitik depo | Ham günlük saklanmadığı için gerekmez. |
| Model eğitimi / ince ayar | Yapılandırılmış bilgi katmanı ve istem mühendisliği yeterlidir. |

---

## 5. Kullanılan standartlar ve teknolojiler

### 5.1 Standartlar

| Standart | Sürüm | Kullanım amacı |
|---|---|---|
| Arazzo Specification | 1.1.0 | Senaryo tanım formatı |
| Overlay Specification | 1.0 | Senaryo yama formatı |
| OpenAPI Specification | 3.x | API sözleşme kaynağı |
| Model Context Protocol | 2026-07-28 | Ajan araç yüzeyi |
| MCP Tasks uzantısı | — | Uzun süren işlemler ve insan onayı |
| Common Test Report Format | Pre-1.0 | Test sonucu dışa aktarımı |
| JUnit XML | — | Sürekli entegrasyon uyumluluğu |
| SARIF | 2.1.0 | Bulgu dışa aktarımı |
| OpenTelemetry Semantic Conventions | — | Test ve ajan telemetrisi |
| Decision Model and Notation | — | İş kuralı karar tabloları |
| RFC 9457 (Problem Details) | — | Teşhis raporu taşıma formatı |
| JSON Pointer (RFC 6901) | — | Uygunluk ihlali konumu |

### 5.2 Platform

| Bileşen | Seçim |
|---|---|
| Çalışma zamanı | .NET 10 |
| Uygulama çatısı | ABP Framework 10 |
| Veri erişimi | Entity Framework Core 10 |
| Veritabanı | PostgreSQL 16+ / SQL Server 2022+ |
| Nesne deposu | S3 uyumlu (üretim), dosya sistemi (geliştirme) |
| Arka plan işleri | Çatının arka plan iş altyapısı + dağıtık kilit |
| Gizli anahtar yönetimi | Anahtar kasası (KV v2) |

---

## 6. Veri modeli

Ayrıntılı şema eki `Test-Module-Database-Schema.dbml` dosyasındadır. Bu bölüm üst düzey
yapıyı özetler.

### 6.1 Şema sahipliği

| Şema | İçerik | Hacim | Saklama |
|---|---|---|---|
| `test_lookup` | 14 referans tablosu | Çok küçük | Süresiz |
| `test_catalog` | Senaryo, ortam, plan, iş bilgisi, ajan oturumu | Orta | Süresiz |
| `test_run` | Koşu ve koşum kayıtları | Büyük | 90 gün |

### 6.2 Ana tablolar

| Tablo | Sorumluluk |
|---|---|
| `content_blobs` | Hash ile adreslenen değişmez metin deposu |
| `scenarios` | Senaryo kimliği, sürüm geçmişi, sağlık metrikleri, onarım önerileri |
| `scenario_step_bindings` | Türetilmiş adres indeksi (etki analizinin temeli) |
| `test_environments` | Mantıksal ad → gerçek adres eşlemesi |
| `test_plans` | Senaryo seçimi ve zamanlama |
| `business_knowledge` | İş sözlüğü, kural katalogu, yolculuk tanımları, gözlenmiş etkiler |
| `agent_sessions` | Ajan oturumu, token muhasebesi, denetim |
| `test_runs` | Koşu başlığı, özet sayaçlar, üretilen dosyalar |
| `scenario_executions` | Senaryo koşumu ve adım sonuçları |

### 6.3 Etki analizi mekanizması

Sistemin ayırt edici yeteneği, sözleşme değişikliğinden etkilenen senaryoların
deterministik olarak bulunmasıdır.

```
Sözleşme değişikliği tespit edilir
        │
        ▼
Değişikliğin adresi çıkarılır  (şema.nesne.alan veya metot+yol+durum)
        │
        ▼
scenario_step_bindings tablosunda indeksli sorgu
        │
        ▼
Etkilenen senaryo ve adım listesi          ← tahmin değil, birleştirme
        │
        ▼
Hedefli koşum tetiklenir ve gerekiyorsa yama önerilir
```

Bu tablo türetilmiştir; yayımlanmış senaryo tanımından yeniden üretilebilir ve kanonik
bilgi taşımaz. Var oluş sebebi yalnızca sorgu performansıdır.

### 6.4 Ölçülebilirlik

Model, aşağıdaki soruların doğrudan cevaplanmasını sağlar:

| Soru | Kaynak |
|---|---|
| Hangi iş kuralı kaç senaryo ile test ediliyor? | Adres indeksi, kural referansı |
| Hangi değişmez kalıpları hiç kullanılmamış? | Adres indeksi, kalıp kodu |
| Bu senaryo son 30 günde ne kadar kararsız? | Sağlık metrikleri |
| Bu adımın gecikmesi bekleme bütçesine yaklaşıyor mu? | Adım bazında yüzde 95'lik dilim |
| Kırmızı koşumların hangi doğrulama katmanında yoğunlaştığı | Adım sonucundaki katman kodu |
| Dün geçen test bugün neden kaldı? | Koşu başındaki ortam parmak izleri |
| Ajan önerilerinin kabul oranı nedir? | Onarım önerisi kayıtları |

---

## 7. Modül entegrasyon modeli

### 7.1 Bağlam ilişkileri

| İlişki | Desen |
|---|---|
| Test Module → doğrulama modülleri | Müşteri–tedarikçi |
| Doğrulama modüllerinin kararlı kod kümeleri | Yayımlanmış dil (published language) |
| Test Module'ün sözlük normalizasyonu | Bozulma önleyici katman |

### 7.2 Dağıtım esnekliği

Test Module yalnızca uygulama sözleşmesi arayüzlerine bağımlıdır. Bunun sonucu:

| Dağıtım | Çalışma biçimi | Test Module kodu |
|---|---|---|
| Tek süreç (mevcut) | Doğrudan bağımlılık enjeksiyonu | Değişmez |
| Ayrı servis (ileride) | Çatının dinamik istemci vekilleri | Değişmez |

### 7.3 Hata modları

Doğrulayıcı çağrısı başarısız olduğunda her durum ayrı sonuç kodu üretir; bu ayrım
kararsızlık ölçümünün kirlenmesini önler.

| Durum | Sonuç |
|---|---|
| Zaman aşımı, bağlantı çözülemedi, gizli anahtar çözülemedi, anahtar tekil değil, operasyon tekil çözülemedi | `Broken` |
| Doğrulama olumsuz, uygunluk ihlali | `Failed` |

---

## 8. Yapay zekâ katmanı

### 8.1 Kurulacak bileşenler

| # | Bileşen | Nitelik |
|---|---|---|
| 1 | Araç yüzeyi (MCP sunucusu) | Mevcut uygulama sunucusuna eklenen bir uç nokta; model içermez |
| 2 | Ajan profilleri | Talimat metni + izinli araç alt kümesi + bütçe (yapılandırma dosyaları) |
| 3 | İstemci | Modeli çağıran taraf |

Araç yüzeyi, kullanıcı arayüzü denetleyicileriyle **aynı uygulama servislerini** çağırır;
doğrulama ve yetkilendirme mantığı tekrarlanmaz ve atlanamaz.

### 8.2 Araç kataloğu

Toplam araç sayısı on ikiyi geçmez ve aşama bazında bölünür.

| Aşama | Araç sayısı | İşlev |
|---|---|---|
| Senaryo yazımı | 6 | Bilgi çözümleme, operasyon özeti, tablo tanımı, taslak üretimi, kuru koşum, kayıt |
| Teşhis | 3 | Koşu özeti, adım kanıtı, başarısızlık açıklaması |
| Bakım | 3 | Yeni bulgular, etkilenen senaryolar, yama önerisi |
| Koşum | **0** | Model devrede değildir |

**Sınırın gerekçesi.** Araç tanımları model bağlamında yer kaplar ve araç sayısı arttıkça
seçim doğruluğu ölçülebilir biçimde düşer. Yalnızca gerekli araç kümesini açmanın bağlam
kullanımında %60–90 azalma sağladığı raporlanmıştır [R14]. Araç tanımlarını isteğe bağlı
yüklemenin bağlam kullanımını %85 azalttığı ve seçim doğruluğunu artırdığı ölçülmüştür [K5].

### 8.3 Bilgi katmanları

Ajanın iş kurallarına erişimi dört katmanda sağlanır:

| Katman | Kaynak | Otorite |
|---|---|---|
| Türetilebilir | API sözleşmesi, yabancı anahtar grafiği, kısıtlar | Kesin |
| Gözlemlenebilir | İşlemin test ortamındaki gözlenmiş etkisi | Olasılıksal, insan onaylı |
| Beyan edilen | İş sözlüğü, kural katalogu, yolculuk tanımları | Otoriter |
| Etkileşimli | Ajanın belirsizlik durumunda sorması | Otoriter |

Son iki katmanda öğrenilen her bilgi kalıcı hâle getirilir; aynı bilgi ikinci kez
keşfedilmez veya sorulmaz.

**Gerekçe — ölçülmüş bulgu.** İş gereksinimleri ve API sözleşmesini birlikte kullanan bir
üretim yaklaşımı, on gerçek serviste (biri yaklaşık 1.000 canlı uç nokta içeren endüstriyel
bir sistem) **%89 oranında elle düzeltme gerektirmeyen geçerli test** üretmiş ve daha önce
bilinmeyen entegrasyon hataları bulmuştur. Aynı çalışma başarı belirleyicilerini şöyle
sıralamıştır: API karmaşıklığı, **iş gereksinimlerinin ayrıntı düzeyi**, API
dokümantasyonunun ayrıntı düzeyi [A10].

Buna karşılık yönlendirilmemiş bir modelin *"paydaşların belirtmediği koşulları
uydurabildiği"* ve belirsiz gereksinimde *"boşlukları kendi varsayımlarıyla doldurup makul
görünen fakat gerçek niyeti yansıtmayan"* kriterler ürettiği raporlanmıştır [A11].
Bu nedenle bilgi katmanı **yapılandırılmış** biçimde tutulur.

### 8.4 Ölçülmüş yetenek sınırları

Aşağıdaki bulgular tasarımın sertleştirilmesinde doğrudan kullanılmıştır.

| Bulgu | Ölçüm | Tasarıma etkisi |
|---|---|---|
| Model eğitim verisinde bulunmayan kod tabanında test kalitesi düşüyor | Özel kod tabanında mutasyon skoru %2,4–10,3; insan tabanı %30,4 [A6] | Modele kod okutulmaz; sözleşme, şema ve kural verilir |
| Gereksinim belirsizse kalite düşüyor | Mutasyon skorunda 26–40 puan düşüş [A5] | Yapılandırılmış iş bilgisi katmanı |
| Hatalı sisteme karşı iyileştirme oracle'ı bozuyor | Dört modelde iyileştirme, iyileştirmemekten kötü [A5] | Kuru koşum geri beslemesi ajana verilmez (K-24) |
| Derlenebilirlik uğruna beklenti siliniyor | %99 derleme başarısı, muhakeme yok [A6] | Beklenti zayıflaması engellenir (K-25) |
| Çok adımlı görevlerde güvenilirlik düşüyor | Tek adım %80–90, çok adımlı zincir %18–24 [K23] | Sert tur sınırı (K-26) |
| Yapılandırılmış korumalar hataları kurtarıyor | 580 senaryoluk kıyaslamada %19,9 kurtarma [K23] | Dört deterministik kapı |

### 8.5 Kalite kapıları

Üretilen her senaryo dört kapıdan geçer:

1. **Biçim doğrulaması** — tanım şeması ve referans bütünlüğü
2. **Türetilebilirlik kontrolü** — beklentiler API sözleşmesinden çıkarılabiliyor mu
3. **Anahtar tekilliği** — veritabanı kontrolünde kullanılan anahtar tekil mi
4. **Kuru koşum** — senaryo canlı sistemde bir kez çalışıyor mu

Ardından insan onayı gelir. Bu kapıların sonucu şudur: zayıf bir model daha çok **deneme**
maliyeti üretir, daha çok **hata** değil.

---

## 9. İncelenen referans projeler ve ürünler

Tasarım kararları, benzer problemleri çözen on beş sistemin incelenmesiyle
gerekçelendirilmiştir. Her satırda: sistemin çözdüğü problem, benimsenen desen ve
benimsenmeyen yön belirtilmiştir.

### 9.1 Test üretimi ve ajan mimarisi

#### Playwright Test Agents (Microsoft) — [R1]

**Ne yapar.** Test çatısının bir parçası olarak üç ajan sunar: planlayıcı (uygulamayı
gezerek insan-okur test planı üretir), üretici (planı çalıştırılabilir teste çevirir),
onarıcı (kırılan testi tekrar koşarak düzeltme önerir). Ajanlar bağımsız uygulamalar
değil, talimat ve araç tanımı demetleridir.

**Benimsenen.**
- Üretim ile koşumun kesin ayrımı: ajanlar yalnızca üretim aşamasında
- İki aşamalı ürün: önce insan-okur plan, sonra makine-koşar tanım
- Onayın insan-okur artefakt üzerinde alınması
- Ajanın uygulama değil, yapılandırma olması

**Benimsenmeyen.** Kullanıcı arayüzü katmanına odaklanma; bu sistemin doğrulama katmanı
arayüz altındadır.

#### Sentry Seer — [R9]

**Ne yapar.** Üretim telemetrisi (hata, iz, günlük, metrik) üzerine kurulu hata ayıklama
ajanı. Geniş kapsamlı sohbet akışı ile dar kapsamlı otomatik düzeltme akışı **aynı ajan
mimarisi** üzerinde çalışır.

**Benimsenen.**
- Tek ajan altyapısı, çok iş akışı — ayrı ajan uygulamaları yazılmaz
- Otonomi seviyesinin kuruluş tarafından yapılandırılabilmesi
- Çalışma zamanı bağlamının statik analizin göremediğini teşhis etmesi tezi

#### Datadog Test Optimization ve Bits AI — [R8]

**Ne yapar.** Kararsız testleri otomatik tespit eder ve bir durum makinesi ile yönetir:
etkin, karantinada, devre dışı, düzeltilmiş. İzlenen ölçütler: başarısızlık oranı,
etkilenen süreç sayısı, boşa harcanan süre, ilk ve son görülme.

**Benimsenen.**
- Kararsızlık yönetiminin rapor değil **kalıcı durum** olması
- Karantinanın "koşar fakat süreci durdurmaz" anlamı
- Durum geçişlerinin politika ile otomatikleşmesi

**Genişletilen.** Karantinaya zorunlu son kullanma tarihi eklenmiştir; karantinanın kalıcı
bir çöp kutusuna dönüşmesi engellenir.

### 9.2 Test yönetimi ve veri modeli

#### Allure TestOps — [R3]

**Ne yapar.** Manuel ve otomatik testleri tek veri modelinde yönetir. Üç kimlik katmanı
kullanır: açık kalıcı kimlik, adaptörün ürettiği hesaplanmış kimlik ve koşum bağlamı
kimliği. Beş sonuç durumu tanımlar.

**Benimsenen.**
- Sonuç durumlarının ikiden fazla olması; başarısızlık ile çalışamama ayrımı
- Trend gruplaması için ayrı kimlik kullanımı
- Koşum sonuçlarının toplu işlenmesi

**Benimsenmeyen — ve dokümante edilmiş gerekçesi.** Kimliğin ad ve parametrelerden
hesaplanması. Ürünün kendi dokümantasyonu, bu yaklaşımın yeniden adlandırmada çift kayıt
ürettiğini ve geçmişi kopardığını uyarı olarak belgelemektedir.

#### Kiwi TCMS — [R5]

**Ne yapar.** Açık kaynak test yönetim sistemi. Veri modeli incelenebilir durumdadır:
plan, durum, koşu ve koşum kayıtları; durum değerleri ağırlık kolonlu ayrı tabloda.

**Benimsenen.**
- Koşum kaydının, tanımın o anki sürümünü kendi satırında taşıması
- Durum değerlerinin referans tablosunda tutulması

#### ReportPortal — [R4]

**Ne yapar.** Test raporlama platformu. Hiyerarşik yapı (başlatma → test öğesi → günlük →
ek dosya) ve üç katmanlı depolama (ilişkisel veritabanı, nesne deposu, günlük indeksi).

**Benimsenen.** Ağır içeriğin ilişkisel tablodan çıkarılıp nesne deposuna alınması.

**Benimsenmeyen.** Günlük indeksleme altyapısı. Bu sistem ham günlük saklamadığı için
üçüncü bir depolama katmanına ihtiyaç duymaz — gizlilik kararının doğrudan altyapı
tasarrufuna dönüştüğü nokta.

#### TestRail, Xray, Zephyr — [R6]

**Ne yapar.** Ticari test yönetim araçları. Farklı depolama stratejileri: kendi veritabanı,
iş takip sistemi kayıtları veya ayrı optimize depo.

**Benimsenen.** Koşu başlatıldığında tanımın anlık kopyasının alınması.

**Benimsenmeyen.** Test varlıklarının iş takip sistemine kayıt olarak yazılması; ölçekte
performans maliyeti raporlanmaktadır.

### 9.3 Sözleşme ve etki analizi

#### Pact Broker — [R7]

**Ne yapar.** Tüketici güdümlü sözleşme testinin merkezi bileşeni. Sözleşme içeriğini
hash'leyerek tekilleştirir, uygulama sürümü ile sözleşme kimliğini ayırır, tüm sürüm
çiftlerinin doğrulama durumunu bir matriste tutar ve sözleşme değiştiğinde etkilenen
tüketicileri görünür kılar.

**Benimsenen.**
- Sözleşme kimliği ile uygulama sürüm kimliğinin ayrılması
- İçerik hash'i ile tekilleştirme
- "Değişiklik → etkilenenler" akışının merkezi bir kayıt üzerinden kurulması

**Genişletilen.** Pact yalnızca API sözleşmesi kapsar. Bu sistemde aynı mekanizma
veritabanı şeması değişikliklerini de kapsar.

#### Azure DevOps Test Impact Analysis / öngörücü test seçimi — [K10]

**Ne yapar.** Birincisi çağrı grafiği ve kapsam verisiyle deterministik test seçimi yapar;
ikincisi geçmiş koşulardan öğrenen bir modelle olasılıksal seçim yapar.

**Benimsenen.** Deterministik yaklaşım.

**Benimsenmeyen.** Makine öğrenmesi tabanlı seçim. Bu sistemde hangi adresin değiştiği
kesin olarak bilinmektedir; olasılıksal tahmin gereksizdir ve açıklanabilirliği düşürür.

#### Specmatic — [R11]

**Ne yapar.** Sözleşme dosyalarını kod yazmadan çalıştırılabilir sözleşmeye çevirir; aynı
tanımdan hem doğrulama testi hem taklit sunucu üretir. Geriye dönük uyumluluğu, yeni
tanımdan taklit ayağa kaldırıp eski tanımdan üretilen testleri ona koşturarak ölçer.

**Benimsenen.** Sözleşme farkının tek başına bir uyumluluk kapısına dönüştürülebileceği tezi.

#### Schemathesis — [R10]

**Ne yapar.** Özellik tabanlı API test aracı. Sözleşmedeki bağlantı tanımları, konum
başlıkları ve şema analizi ile üretici ve tüketici operasyonları eşleştirerek durum
makinesi kurar.

**Benimsenen.** Operasyon zincirlerinin sözleşmeden türetilmesi; senaryo iskeletinin
tahmin edilmek yerine hesaplanması.

### 9.4 Orkestrasyon ve altyapı

#### Testkube — [R2]

**Ne yapar.** Kapsayıcı tabanlı test orkestrasyon platformu. Tanım kaynağı ile koşum
kaynağını **ayrı** nesneler olarak modeller; koşum sırasında geçici kaynaklar üretir ve
tamamlandığında siler.

**Benimsenen.**
- Tanım ile koşum kaydının ayrı kalıcı nesneler olması
- Koşum sonuçlarının kaynaklardan bağımsız saklanması

**Benimsenmeyen.** Otuz araçlık geniş araç kataloğu; araç sayısının seçim doğruluğuna
etkisi nedeniyle bu sistemde on iki ile sınırlandırılmıştır.

#### Temporal — [R12]

**Ne yapar.** Dayanıklı iş akışı platformu. İş akışı mantığını belirlenimci ve yeniden
oynatılabilir tutar; çökme sonrası durumu geçmiş kaydından yeniden kurar.

**Benimsenen — kavram düzeyinde.** "Her adım tamamlandığında kalıcı kontrol noktası yaz"
ilkesi.

**Benimsenmeyen — altyapı düzeyinde.** Ayrı bir küme işletmek ikinci bir durum sahibi
yaratır ve kiracı/yetki bağlamını uygulama dışına taşır. Aynı ilke, adım sonucunun
kalıcı yazılmasıyla mevcut altyapıda karşılanmaktadır.

#### GitHub MCP Server — [R14]

**Ne yapar.** Araç kümelerini gruplar hâlinde yönetir; yalnızca ihtiyaç duyulan grupların
etkinleştirilmesine izin verir ve çalışma anında grup keşfi sunar.

**Benimsenen.** Aşama bazında araç profili yaklaşımı ve araç kataloğunun daraltılması.
Ölçülen bağlam tasarrufu %60–90 aralığındadır.

#### Jentic / OAK — [R13]

**Ne yapar.** Ajanik bilgiyi (ne çağrılır, hangi sırayla, hangi başarı ölçütüyle)
deklaratif formatlarda tutar; araç protokolünü yalnızca bu bilgiye erişim yolu olarak
konumlandırır.

**Benimsenen.** "Bilgi katmanı ayrı, erişim protokolü ayrı" ayrımı. Bu sistemde bilgi
katmanı iş sözlüğü ve kural katalogudur.

---

## 10. Ölçüm ve kabul kriterleri

### 10.1 Aşama bazında maliyet bütçesi

| Aşama | Bütçe |
|---|---|
| Araç kataloğunun sabit maliyeti | ≤ 3.000 token |
| Senaryo yazımı | ≤ 15.000 token / senaryo |
| **Koşum** | **0** |
| Teşhis | ≤ 5.000 token / başarısız koşu |
| Bakım | ≤ 2.000 token / bulgu |
| Tek araç yanıtı | ≤ 8.000 token |

Bu bütçeler sürekli entegrasyon kapısına bağlanır; aşım derleme sürecini durdurur.
Ölçüm kaynağı ajan oturum kayıtlarıdır.

### 10.2 Aşama kabul kriterleri

| Aşama | Kabul kriteri |
|---|---|
| Temel | Elle yazılmış bir senaryo uçtan uca başarıyla koşuyor ve **tek satır model çağrısı yok** |
| Kanıt | Başarısız koşumda hangi adımın hangi doğrulama katmanında başarısız olduğu raporda görünüyor |
| Yazım ajanı | Ajanın ürettiği tanım, temel koşum motorunda **değiştirilmeden** çalışıyor |
| Teşhis | Başarısız koşumda sıralı hipotez raporu 4 KB sınırı içinde dönüyor |
| Bakım | Bir sözleşme alanı isteğe bağlı hâle geldiğinde etkilenen senaryolar bulunuyor ve ajan bağlamına giren veri 2.000 token'ın altında kalıyor |
| Sağlık | Kararsız senaryo süreci durdurmadan izleniyor ve durum geçişi denetlenebiliyor |

### 10.3 Kalite göstergeleri

| Gösterge | Anlamı |
|---|---|
| Kararsızlık oranı | Senaryonun tekrarlanabilirliği |
| Yanlış alarm oranı | Başarısız olup gerçek hata bulunmayan koşum oranı |
| Sonuçsuz koşum oranı | Hiçbir şey doğrulamayan koşum oranı; test verisi sağlığı göstergesi |
| Karantina oranı | Test paketinin çürüme göstergesi |
| Ortalama teşhis süresi | Başarısızlıktan hipoteze geçen süre |
| İş kuralı kapsamı | Kaç iş kuralının kaç senaryo ile test edildiği |
| Öneri kabul oranı | Ajan çıktısının pratik değeri |

---

## 11. Riskler ve karşı önlemler

| Risk | Etki | Karşı önlem |
|---|---|---|
| Adres grameri iki tarafta ayrışır | Etki analizi sessizce hiçbir sonuç döndürmez | Ortak fikstür kümesine karşı mutabakat testi, ilk günden itibaren |
| JSON kolonları beklenenden büyür | Liste sorguları yavaşlar | Boyut tavanı, taşma durumunda nesne deposuna aktarım, yayın kapısında uyarı |
| Ajan beklentileri zayıflatır | Test yanlış nedenle başarılı olur | Beklenti sayısı kapısı, zayıflatma işareti, onay ekranında uyarı |
| Ajan hatalı sisteme uyum sağlar | Hata kalıcılaşır | Kuru koşum geri beslemesi verilmez, çelişki insana bildirilir |
| Onay sonrası içerik değişir | Onaylanmayan değişiklik uygulanır | Onay içerik özetine bağlıdır |
| Kiracılar arası veri sızıntısı | Ciddi güvenlik olayı | Dokuz tabloda kiracı filtresi, veritabanı katmanında kapsam, izolasyon testi |
| İş bilgisi katmanı doldurulmaz | Ajan çıktısı düşük kaliteli olur | Olgunluk seviyesi göstergesi; sistem hangi seviyede olunduğunu ve bir üst seviyede nelerin açılacağını bildirir |
| Düzenlenmiş sektör müşterisi ek denetim ister | Uyumluluk engeli | Yükseltme yolu belgelenmiştir |
| Düzenlenmiş sektör müşterisi veri ikametgâhı ister | Uyumluluk engeli | Model erişimi port arkasındadır; yerel model uyarlayıcısı eklenebilir |

---

## 12. Açık maddeler

| # | Konu | Karar mercii |
|---|---|---|
| 1 | Ürün içi ajanın başlangıç model sağlayıcısı | Ürün ve maliyet |
| 2 | Yerel model desteğinin tetikleyici müşteri talebi | Satış ve ürün |
| 3 | Kullanıcı arayüzünün ayrı uygulama mı, aynı sunucuda mı yaşayacağı | Mimari |
| 4 | Migration yürütme stratejisi (tek yürütücü veya dağıtım hattı) | Operasyon |
| 5 | Nesne deposu sağlayıcı seçimi | Operasyon |

---

## 13. Kaynakça

### 13.1 Akademik kaynaklar

| Kod | Kaynak |
|---|---|
| [A1] | *Test Oracle Automation in the Era of LLMs.* ACM. https://dl.acm.org/doi/10.1145/3715107 |
| [A2] | *LogicHunter.* arXiv. https://arxiv.org/html/2607.06195 |
| [A3] | Segura, S. ve ark. *Metamorphic Testing of RESTful Web APIs.* IEEE TSE, 2017. https://javiertroyauma.github.io/publications/TSE2017_REST_prePrint.pdf |
| [A4] | *ARMeta: Multi-Agent LLM-based Metamorphic Testing for REST APIs.* arXiv, 2026. https://arxiv.org/html/2605.28321v1 |
| [A5] | *RESTestBench: A Benchmark for Evaluating the Effectiveness of LLM-Generated REST API Test Cases from NL Requirements.* arXiv, 2026. https://arxiv.org/html/2604.25862v1 |
| [A6] | *LLMs Taking Shortcuts in Test Generation: A Study with SAP HANA and LevelDB.* arXiv, 2026. https://arxiv.org/html/2604.14437v1 |
| [A7] | Luo, Q. ve ark. *An Empirical Analysis of Flaky Tests.* ACM SIGSOFT FSE, 2014. https://dl.acm.org/doi/10.1145/2635868.2635920 |
| [A8] | *Flaky Tests in a Large Industrial Database Management System: An Empirical Study of Fixed Issue Reports for SAP HANA.* arXiv, 2026. https://arxiv.org/html/2602.03556 |
| [A9] | Huo, C.; Clause, J. *Improving Oracle Quality by Detecting Brittle Assertions.* https://www.engr.ship.edu/~chuo/papers/huo14.pdf |
| [A10] | *APITestGenie: Generating Web API Tests from Requirements and API Specifications with LLMs.* ACM/IEEE AST, 2026. https://dl.acm.org/doi/full/10.1145/3793654.3793743 |
| [A11] | *RAGcceptance M2RE: Multi-Modal Requirements Data-based Acceptance Criteria Generation using LLMs.* arXiv. https://arxiv.org/pdf/2508.06888 |
| [A12] | Alonso, J. C. ve ark. *AGORA+: Test Oracle Generation for REST APIs.* ACM TOSEM. https://personales.us.es/sergiosegura/files/papers/alonso25-tosem.pdf |
| [A13] | *Test flakiness' causes, detection, impact and responses: A multivocal review.* Journal of Systems and Software. https://www.sciencedirect.com/science/article/pii/S0164121223002327 |
| [A14] | *An Empirical Study of Gemini for Detecting Natural Language Test Smells in Manual Test Cases.* arXiv, 2026. https://arxiv.org/pdf/2606.13804 |
| [A15] | *The Long-Horizon Task Mirage? Diagnosing Where and Why Agentic Systems Break.* arXiv, 2026. https://arxiv.org/pdf/2604.11978 |
| [A16] | *MCP-Zero: Proactive Toolchain Construction for LLM Agents from Scratch.* arXiv. https://arxiv.org/html/2506.01056v1 |

### 13.2 Standartlar ve resmî spesifikasyonlar

| Kod | Kaynak |
|---|---|
| [S1] | *Arazzo Specification 1.1.0.* OpenAPI Initiative. https://spec.openapis.org/arazzo/latest.html |
| [S2] | *Overlay Specification 1.0.0.* OpenAPI Initiative. https://spec.openapis.org/overlay/v1.0.0.html |
| [S3] | *Model Context Protocol, 2026-07-28 — Server Tools.* https://modelcontextprotocol.io/specification/2026-07-28/server/tools |
| [S4] | *MCP Tasks Extension.* https://modelcontextprotocol.io/extensions/tasks/overview |
| [S5] | *Common Test Report Format — JSON Schema.* https://ctrf.io/docs/full-schema |
| [S6] | *Static Analysis Results Interchange Format (SARIF) 2.1.0.* OASIS. https://docs.oasis-open.org/sarif/sarif/v2.1.0/sarif-v2.1.0.html |
| [S7] | *OpenTelemetry Semantic Conventions — GenAI ve Test öznitelikleri.* https://opentelemetry.io/docs/specs/semconv/registry/attributes/gen-ai/ · https://opentelemetry.io/docs/specs/semconv/registry/attributes/test/ |
| [S8] | *Agent Audit Trail: A Standard Logging Format for Autonomous AI Systems.* IETF taslağı. https://datatracker.ietf.org/doc/draft-sharif-agent-audit-trail/ |
| [S9] | *Avrupa Birliği Yapay Zekâ Yasası, Madde 12 — Kayıt tutma.* |
| [S10] | *Decision Model and Notation.* Object Management Group. https://www.omg.org/spec/DMN/ |
| [S11] | *RFC 9457 — Problem Details for HTTP APIs.* https://www.rfc-editor.org/rfc/rfc9457.html |
| [S12] | *RFC 6901 — JavaScript Object Notation (JSON) Pointer.* https://www.rfc-editor.org/rfc/rfc6901.html |

### 13.3 Ürün ve proje dokümantasyonu

| Kod | Kaynak |
|---|---|
| [R1] | *Playwright Test Agents.* Microsoft. https://playwright.dev/docs/test-agents |
| [R2] | *Testkube — Test Workflows High-Level Architecture.* https://docs.testkube.io/articles/test-workflows-high-level-architecture |
| [R3] | *Allure TestOps — Test Results.* https://docs.qameta.io/allure-testops/briefly/test-results/ |
| [R4] | *ReportPortal — Reporting Developers Guide.* https://reportportal.io/docs/developers-guides/ReportingDevelopersGuide/ |
| [R5] | *Kiwi TCMS — Test Runs Models.* https://kiwitcms.readthedocs.io/en/latest/_modules/tcms/testruns/models.html |
| [R6] | TestRail / Xray / Zephyr ürün dokümantasyonları ve karşılaştırma analizleri |
| [R7] | *Pact Broker — Versioning in the Pact Broker.* https://docs.pact.io/getting_started/versioning_in_the_pact_broker |
| [R8] | *Datadog — Flaky Tests Management.* https://docs.datadoghq.com/tests/flaky_management/ |
| [R9] | *Sentry Seer.* https://docs.sentry.io/product/ai-in-sentry/seer |
| [R10] | *Schemathesis — Stateful Testing.* https://schemathesis.readthedocs.io/en/stable/explanations/stateful/ |
| [R11] | *Specmatic — Contract Driven Development.* https://docs.specmatic.io/contract_driven_development |
| [R12] | *Temporal — Durable Execution.* https://learn.temporal.io/ |
| [R13] | *Jentic — The MCP Tool Trap.* https://jentic.com/blog/the-mcp-tool-trap |
| [R14] | *GitHub MCP Server — Toolsets.* https://deepwiki.com/github/github-mcp-server/3-github-toolsets |
| [R15] | *ABP Framework — Integration Services, Background Jobs, BLOB Storing, Multi-Tenancy.* https://abp.io/docs/latest/ |

### 13.4 Sektör ölçümleri ve mühendislik yazıları

| Kod | Kaynak |
|---|---|
| [K1] | Playwright MCP token ölçümleri (pratisyen ölçümü) |
| [K2] | *Code Execution with MCP.* Anthropic Engineering. https://www.anthropic.com/engineering/code-execution-with-mcp |
| [K3] | *Effective Context Engineering for AI Agents.* Anthropic Engineering. https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents |
| [K4] | *Writing Tools for Agents.* Anthropic Engineering. https://www.anthropic.com/engineering/writing-tools-for-agents |
| [K5] | *Advanced Tool Use.* Anthropic Engineering. https://www.anthropic.com/engineering/advanced-tool-use |
| [K6] | *Designing MCP Tools for Agents: Lessons from Building Datadog's MCP Server.* https://www.datadoghq.com/blog/engineering/mcp-server-agent-tools/ |
| [K7] | *Test Automation Maintenance Costs.* Rainforest QA. https://www.rainforestqa.com/blog/test-automation-maintenance |
| [K8] | *Test Data Management Best Practices for API Testing.* Total Shift Left. https://totalshiftleft.ai/blog/test-data-management-best-practices-api-testing |
| [K9] | *Why Traditional E2E API Testing is Failing.* CloudQA. https://cloudqa.io/why-traditional-e2e-api-testing-is-failing-in-2026/ |
| [K10] | *Test Impact Analysis.* Microsoft Learn. https://learn.microsoft.com/azure/devops/pipelines/test/test-impact-analysis · *The Rise of Test Impact Analysis.* Martin Fowler. https://martinfowler.com/articles/rise-test-impact-analysis.html |
| [K11] | Test bakım maliyeti ve kırılgan test analizleri (sektör raporları, 2026) |
| [K12] | *The Real Cost of Flaky Tests: Data from 1,000+ Engineering Teams.* FlakyGuard. https://flakyguard.com/blog/cost-of-flaky-tests |
| [K13] | *World Quality Report 2024-25.* Capgemini |
| [K14] | *On False Negatives and False Positives.* On Test Automation. https://www.ontestautomation.com/on-false-negatives-and-false-positives/ |
| [K15] | *Useless Unit Tests: Patterns That Never Fail.* https://getautonoma.com/blog/useless-unit-tests-tautological-anti-pattern |
| [K16] | *Test Data Management Strategy.* Total Shift Left. https://totalshiftleft.com/blog/test-data-management-strategy |
| [K17] | *Audit Trail Patterns for AI Agents.* https://clarm.com/blog/articles/audit-trail-patterns-for-ai-agents/ |
| [K18] | *Modular Monolith Communication Patterns.* Milan Jovanović. https://milanjovanovic.tech/blog/modular-monolith-communication-patterns · *Modular Monolith: Integration Styles.* Kamil Grzybek. https://www.kamilgrzybek.com/blog/posts/modular-monolith-integration-styles |
| [K19] | *Context Mapping.* DevIQ. https://deviq.com/domain-driven-design/context-mapping/ · *Anti-Corruption Layer Pattern.* https://oneuptime.com/blog/post/2026-01-30-anti-corruption-layer-pattern/view |
| [K20] | *OWASP Top 10 for LLM Applications 2026.* OWASP GenAI Security Project. https://genai.owasp.org/resource/owasp-genai-llm-top-10-2026/ |
| [K21] | *Human-in-the-Loop Approval Framework.* Agentic Patterns. https://www.agentic-patterns.com/patterns/human-in-loop-approval-framework/ |
| [K22] | *Human-in-the-Loop Authorization Patterns for Autonomous Agents.* MojoAuth. https://mojoauth.com/blog/human-in-the-loop-authorization-patterns-for-autonomous-agents |
| [K23] | Ajan güvenilirliği kıyaslamaları ve uzun ufuklu görev ölçümleri (2026) |
| [K24] | *Agent Skills vs MCP: Architecture and Decision Guide.* Atlan. https://atlan.com/know/ai-agent/ai-agent-skills/agent-skills-vs-mcp/ |
| [K25] | *PostgreSQL Partition Manager ve saklama politikaları.* Crunchy Data. https://www.crunchydata.com/blog/five-great-features-of-postgres-partition-manager |
| [K26] | *Testcontainers Best Practices for .NET Integration Testing.* https://milanjovanovic.tech/blog/testcontainers-best-practices-dotnet-integration-testing |

---

*Tüm dış kaynaklara erişim tarihi: 12–13 Ağustos 2026.*
</content>

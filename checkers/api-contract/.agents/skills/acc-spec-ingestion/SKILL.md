---
name: acc-spec-ingestion
description: Fetch, validate, parse and snapshot OpenAPI/Swagger documents from live service URLs in ApiContractChecker. Use for HTTP client and resilience configuration, untrusted-content guards, format detection, content hashing, snapshot persistence and idempotent re-fetching, and for handling source credentials held in Vault.
---

# Spec çekme ve anlık görüntü

Çekilen içerik **güvenilmez veridir**. Hiçbir bayt, guard'lardan geçmeden
kalıcılaştırılmaz.

Giriş yolu kararı: [kanonik API Contract Checker gerçeği](../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#7-api-contract-checker-gercegi)
(canlı URL; dosya yükleme ve git/CI kapsam dışı).

## Boru hattı

```text
kaynak coz -> kimlik bilgisini Vault'tan al -> HTTP GET -> guard'lar
   -> format tespiti -> ayristir -> icerik hash'i -> snapshot (idempotent)
```

## HTTP katmanı

`IHttpClientFactory` typed client. Dayanıklılık:

```csharp
services.AddHttpClient<ISpecFetcherClient, SpecFetcherClient>()
        .AddStandardResilienceHandler();
```

- Paket: `Microsoft.Extensions.Http.Resilience`.
- **`Microsoft.Extensions.Http.Polly` KULLANILMAZ** — Microsoft tarafından
  kullanımdan kaldırıldı.
- Tek bir dayanıklılık işleyicisi ekle; üst üste yığma.
- Yeniden deneme + zaman aşımı birlikteyse: Polly `TimeoutRejectedException`
  fırlatır, standart `TimeoutException` **değil**. `ShouldHandle` yazarken bunu
  hesaba kat.
- `HttpClient` singleton tutulmaz; factory yönetir.

## Guard'lar — sırayla, hepsi zorunlu

| # | Guard | Neden |
|---|---|---|
| 1 | Yanıt durum kodu başarılı mı | 404/401 sessizce boş spec'e dönüşmemeli |
| 2 | `Content-Type` beklenen kümede mi (`json`, `yaml`, `text`) | HTML hata sayfası spec sanılmasın |
| 3 | Boyut üst sınırı aşılmadı mı | Bellek tüketimi ve DoS yüzeyi |
| 4 | İçerik boş değil mi | Boş gövde "her şey silindi" farkı üretir |
| 5 | Doküman **okunabildi** mi (kısmi çözülme yok mu) | Kısmi çözülmüş spec yanlış fark üretir |

Guard 5 **okunabilirliği** ölçer, doküman kalitesini değil. Okuyucu boş
`ValidationRuleSet` ile çalışır: doküman hiç üretilemezse `ParseFailed`, üretildiği
hâlde okuyucu hatası varsa `DocumentInvalid`, spec sürümü tanınmazsa
`UnsupportedFormat`. İzlenen servisin **kendi** ihlallerini (duplicate path imzası,
ABP generic şema adı) guard'a çevirme — bu ürün üçüncü tarafı gözler, denetlemez;
o kapıyı kapatmak ürünü kendi hedef servislerine karşı kullanılamaz yapar
([hata kataloğu #21](../acc-vertical-slice/references/failure-catalog.md)).

Her guard **kendi hata kodunu** fırlatır (`Domain.Shared/ExceptionCodes`), jenerik
exception değil. Ağ/ayrıştırma hatasını alan hatasına çeviren nokta, bu repoda
`try/catch` kullanmanın **tek** meşru yeridir
([kanonik checker kuralları](../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#13-checker-gelistirme-kurallari)).

## Kimlik bilgisi

- Vault'ta durur; varlıkta yalnız secret yolu bulunur.
- Bellekte yalnız istek kurulurken çözülür.
- **Hiçbir** DTO'da, logda, hata mesajında veya bulgu gövdesinde görünmez.
- Erişim testi endpoint'i de sırrı yanıtta döndürmez — yalnız durum + varsa hata
  mesajı.

## İçerik hash'i ve idempotans

```text
hash = SHA-256(normalize edilmemis HAM bayt)
```

- Hash **ham içerik** üzerinden alınır, ayrıştırılmış model üzerinden değil.
  Sebep: ayrıştırma sürümü değişince hash değişmemeli.
- Kaynak + doküman adı için **son snapshot'ın hash'i aynıysa yeni satır yazılmaz**.
  Bu, hem depolamayı hem de gereksiz karşılaştırmayı kapatır.
- Aynı hash tekrar geldiğinde "değişiklik yok" bilgisi yine de kaydedilir
  (son görülme zamanı), ama yeni snapshot satırı **açılmaz**.

## Bir kaynak = N doküman

.NET 10'da tek uygulama `AddOpenApi("v1")` + `AddOpenApi("v2")` ile birden çok
doküman yayımlar. `SpecSource` bu yüzden **doküman listesi** taşır; bir kaynak bir
doküman değildir. Çekme döngüsü doküman başınadır ve biri başarısız olunca
diğerleri **devam eder** — sonuç kısmi başarı olarak raporlanır.

## Yazarken

Manifest Tip A (kaynak yönetimi) ve Tip D (asenkron çalıştırma):
[`../acc-vertical-slice/references/artifact-manifest.md`](../acc-vertical-slice/references/artifact-manifest.md).
Çekme işi uzun dış I/O'dur — **hiçbir UOW tutmaz**.

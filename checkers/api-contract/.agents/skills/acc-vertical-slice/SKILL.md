---
name: acc-vertical-slice
description: Turn a business requirement into a complete, repository-native vertical slice in ApiContractChecker — entity, manager, repository, DTOs, validators, Mapperly mapper, AppService, controller, permissions, EF configuration, migration and tests. Use for any new feature, CRUD surface, or endpoint in this repository. Covers the exact artifact manifest, the file order that avoids rework, per-layer code recipes, and the catalog of mistakes that have actually shipped here.
---

# Dikey dilim yazma

Bir iş gereksinimi geldiğinde kullanıcıya "bu dosya nereye gitsin" diye sorma.
Manifest gereksinimden çıkar; sen yalnız **iş davranışı** belirsizse sorarsın.

## Adım sırası (bu sıra rework'ü engeller)

1. **Kullanım senaryosunu yaz.** Ne değişecek, ne **bilerek değişmeyecek**.
   Bir cümlede söyleyemiyorsan senaryo atomik değildir — böl.
2. **En yakın tamamlanmış kardeşi oku.** Yeni desen uydurma. Kardeş yoksa
   [`references/layer-recipes.md`](references/layer-recipes.md) tarifini izle.
3. **Manifesti çıkar.** [`references/artifact-manifest.md`](references/artifact-manifest.md)
   gereksinim tipinden dosya listesini verir.
4. **Aşağıdan yukarı yaz:** Domain.Shared → Domain → Application.Contracts →
   Application → EntityFrameworkCore → HttpApi → test.
   Tersine yazarsan üst katman henüz olmayan tipe dayanır ve iki kez yazarsın.
5. **Derle.** Her katmandan sonra, sonunda değil.
6. **Kapanış kapısı** — [`../../orchestration/task-router.md`](../../orchestration/task-router.md).

## Kardeş ve temiz servis kapısı

AppService veya manager yazmadan önce en yakın tamamlanmış kardeş için şu dört
kanıtı çıkar; kanıt çıkmadan kod yazma:

1. concrete sınıf bildirimi, kullandığı base ve override hook'ları;
2. public use-case'in adımları ve her adımın sahibi;
3. çözülen her bağımlılığın bu tipte bulunma gerekçesi;
4. mapper'daki gerçek RMG tanıları.

Public AppService metodu tek use-case'i düz bir orkestrasyon olarak okutmalı;
validator, UOW, load, persist veya mapping akışını base zaten veriyorsa concrete
serviste yeniden kurma. Büyük akışı yalnız ayrı bir sorumluluk taşıyan küçük,
adlandırılmış metotlara böl; aynı işi farklı helper'larda tekrar etme.

En yakın kardeşin base'i varsayılandır, dokunulmaz dogma değildir. Farklı base ancak
use-case davranışı gerçekten farklıysa ve fark açıkça gerekçelendirilmişse seçilir.
Aggregate construction uyumsuzluğu ortak CRUD akışını kopyalama gerekçesi değildir;
ikinci kullanımda mevcut hook'u genelleştir.

Bağımlılığı teknoloji adına göre yasaklama veya başka katmana sürme. Repository,
cache, secret store, HTTP ya da mesajlaşma işbirlikçisi ancak use-case sorumluluğu ve
repo mimarisi o tipe veriyorsa orada bulunur. Ölçüt; implementasyon ayrıntısının,
wire tipinin veya yabancı exception'ın katman sınırını aşmaması ve sınıfın birden
fazla sorumluluğa dönüşmemesidir.

Mapperly ignore yalnız ignore olmadan alınmış somut RMG tanısı ve bilinçli sözleşme
kararı varsa eklenir; uyarıyı anlamadan toplu susturma listesi kurma.

Yerel gereksinim, Wiki Brain veya kanıtlanmış kardeşte bulunmayan repair,
compensation ya da fallback semantiğini kendiliğinden ekleme. Genel kural bunu
gerektiriyor görünüyorsa davranışı icat etmek yerine çelişkiyi bildir.

## Yazmadan önce üç soru

| Soru | Hayırsa |
|---|---|
| ABP bunu zaten çözüyor mu? | Devam et |
| Bu repoda taban/yardımcı var mı? (`BaseManager`, `BaseRepository`, `LookupCrudAppService`, `EntityCrudAppServiceBase`, `EntityReadControllerBase`, `ApiContractCheckerTenantBackgroundJob<TArgs>`, `SpecFormatComponentResolver<T>`, `ISecretProvider`) | Devam et |
| Bu akışı **ikinci kez** mi yazıyorum? | İlk uygulama somut kalabilir |

Üçüne de "evet" çıkarsa yeni kod yazma — mevcut olanı kullan veya ortak davranışı
doğru tabana taşı ([kanonik checker kuralları](../../../../../docs/PTN-ASSURANCE-PLATFORM-WIKI.md#13-checker-gelistirme-kurallari)).

## Katman sorumlulukları — tek cümlelik sınav

Yazdığın dosya bu cümleyi geçemiyorsa yanlış katmandadır.

- **Controller:** rota, taşıma bağlaması, yetkilendirme metadatası, `Result<T>`.
  *İçinde tek bir `if` iş kararı varsa yanlış.*
- **AppService:** yetkilendir → doğrula → yükle → manager'ı çağır → kalıcılaştır →
  eşle. *İçinde iş kuralı varsa yanlış.*
- **Manager:** varlık, benzersizlik, sahiplik, durum geçişi, DB'ye dayalı doğrulama.
  *İçinde DTO veya HTTP tipi varsa yanlış.*
- **Repository:** tüm LINQ, filtre, sıralama, sayfalama, projeksiyon, include,
  ham SQL. *Dışında LINQ varsa yanlış.*
- **Entity:** yalnız kalıcı alanlar, `internal set`, EF ctor'u ve atama yapan ctor.
  *Doğrulama/geçiş/mutasyon metodu veya public setter varsa yanlış; bunlar manager'a gider.*

## Referanslar

| Ne zaman | Aç |
|---|---|
| Manifesti çıkarırken | [`references/artifact-manifest.md`](references/artifact-manifest.md) |
| Her katmanı yazarken | [`references/layer-recipes.md`](references/layer-recipes.md) |
| Bir şey beklenmedik davrandığında | [`references/failure-catalog.md`](references/failure-catalog.md) |

## Bitirirken raporla

Hangi kuralları uyguladığını, çalıştırmadığın kontrolleri ve kalan riski tek
paragrafta yaz. Bir kuralı bilerek esnettiysen sebebini açıkça söyle —
**sessizlik ihlal sayılır**.

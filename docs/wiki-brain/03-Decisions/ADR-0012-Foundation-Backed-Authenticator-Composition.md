---
id: ADR-0012
type: decision
status: accepted
title: Foundation-backed Authenticator composition
created: 2026-08-13
updated: 2026-08-13
owners:
  - mertbyd
supersedes: []
superseded_by: ADR-0013
decision_refs:
  - ADR-0002
  - ADR-0005
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0004
---

# ADR-0012 — Foundation tabanlı Authenticator kompozisyonu

> [!WARNING] Bu karar [[ADR-0013-Test-Module-Resource-Server-Auth-Consumption|ADR-0013]] ile yerini bıraktı
> Değişen tek madde **4**'tür: Test Module hostu `AuthenticatorHttpApiModule`'ü compose etmez ve
> ürünün issuer'ı olmaz; kimlik yüzeyi ayrı deploy edilen Authenticator hostunda kalır.
> Madde 1, 2, 3, 5, 6 ve 7 ADR-0013'te aynen taşınmıştır ve hâlâ bağlayıcıdır.
> Ayrıca "yayın tamamlanmadı" notu geçmiştir: sekizli `2.0.0` ailesi 2026-08-13'te nuget.org'da
> yayımlanmış ve registry'den doğrulanmıştır.

## Bağlam

Hedef topoloji ADR-0005 ile tek deploy edilen composition host ve tek issuer olarak
belirlendi. Authenticator paket ailesi artık ortak, public Foundation paketlerini
katman eşlemeli biçimde kullanıyor. Eski Auth tüketim sayfası ayrı Auth hostu ve iki
Swagger tarif ederek bu hedefle çelişiyordu. Ayrıca consumer'ın Auth ile Foundation
paketlerini ayrı ayrı yönetmesi dependency drift ve eksik ABP module graph riski doğurur.

Foundation yedi paketlik `1.0.0` ailesiyle nuget.org'da publictir. Authenticator'ın
Foundation tabanlı sekiz paketlik `2.0.0` ailesi restore/build/test, paket inceleme ve
yalnız Auth paketlerini alan clean consumer smoke kapılarından geçmiştir; bu karar
tarihinde nuget.org'a henüz push edilmemiştir.

## Karar

1. Taban yönü `ABP -> Foundation -> Authenticator -> Assurance composition host` olur.
2. Consumer yalnız ihtiyaç duyduğu `Authenticator.*` katman paketlerini referanslar.
   Eşlenen `Nexum.Abp.Foundation.*` paketleri Authenticator nuspec dependency'leriyle
   transitif gelir; consumer doğrudan Foundation `PackageReference` yazmaz.
3. Aynı ilişki ABP module graph'ta da korunur. Authenticator module'leri eşlenen
   Foundation module'lerine `DependsOn` olur; consumer Foundation module'lerini ayrıca
   compose etmez.
4. Hedef Test Module hostu Authenticator Application, EntityFrameworkCore ve HttpApi
   yüzeylerini tek kez compose eder ve ürünün tek Identity/OpenIddict issuer'ı olur.
   Checker doğrulama hostları ve ikinci bir Auth hostu production'da çalışmaz.
5. Auth tabloları ve migration modeli Authenticator'a aittir. Composition migrator
   Authenticator'ın paketlenmiş migration assembly'sini deterministik sırada uygular;
   Test Module veya checker projeleri Auth migration'ı yeniden üretmez.
6. `Authenticator.HttpApi.Client` yalnız ayrı bir uzak client gerekiyorsa kullanılır.
   Aynı process içindeki composition host için HttpApi module kullanılır. EventHandler
   yalnız event tüketimi gereken hostta eklenir.
7. Public `1.x` ailesi yeni hedef entegrasyonun sürümü değildir. Yeni entegrasyon,
   sekizli `2.0.0` ailesi nuget.org'da eksiksiz yayımlanıp registry ve clean-cache
   consumer kapıları geçtikten sonra başlar; local feed production kaynağı olamaz.

## Alternatifler

- Foundation paketlerini consumer'a ayrıca ekletmek: sürüm ve module graph sahipliğini
  iki yere böler; elendi.
- Authenticator'ı ayrı deploy edip Assurance hostta yalnız bearer doğrulamak: ADR-0005'in
  tek host/modüler monolit hedefiyle çelişir; elendi.
- Foundation davranışını Auth içine kaynak olarak kopyalamak: tek ortak base sahipliğini
  bozar ve iki farklı implementasyon üretir; elendi.
- Public `1.x` ile entegrasyona devam etmek: sekiz paket hizası ve Foundation base zinciri
  bulunmadığı için elendi.

## Sonuçlar ve riskler

Consumer Auth paketlerini güncellerken Foundation sürümünü ayrıca seçmez. Authenticator'ın
audit, lookup pasifleştirme, authorization ve kararlı hata davranışları Foundation'ın genel
CRUD davranışına indirgenmez. `2.0.0` public base değişikliği nedeniyle bilinçli major
geçiştir. Yayın tamamlanana kadar Test Module Auth entegrasyonu readiness kapısında blokludur.

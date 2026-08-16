---
id: ADR-0013
type: decision
status: accepted
title: Test Module resource-server auth tuketimi
created: 2026-08-13
updated: 2026-08-13
owners:
  - mertbyd
supersedes:
  - ADR-0012
superseded_by: null
decision_refs:
  - ADR-0005
  - ADR-0012
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0004
---

# ADR-0013 — Test Module resource-server auth tüketimi

## Bağlam

[[ADR-0012-Foundation-Backed-Authenticator-Composition|ADR-0012]] Test Module hostunun
Authenticator'ın Application, EntityFrameworkCore **ve HttpApi** yüzeylerini tek hostta compose
edip ürünün tek Identity/OpenIddict issuer'ı olmasını karara bağlamıştı.

İki yeni gerçek bu maddeyi yeniden açtı:

1. Ürün sahibi, şirket içi ödeme ürününün (`ptn-payment-management-api`) auth tüketim desenini
   Test Module için kanon ilan etti (2026-08-13, doğrudan talimat). O üründe katman paketleri
   **tip** olarak alınır, `HttpApi` modülü compose **edilmez**, doğrulama ABP JWT bearer ile
   ayrı deploy edilmiş auth servisine yapılır.
2. Authenticator sekizli `2.0.0` ailesi ile Foundation yedili `1.0.0` ailesi nuget.org'da
   yayımlandı ve 21/21 PackageId registry'den doğrulandı (2026-08-13). ADR-0012'nin
   "yayın tamamlanmadan entegrasyon başlamaz" ön koşulu artık karşılanmıştır.

Aynı ikilik `pintern-test-platform` altındaki eski `ADR-0003` taslağında da tartışılmıştı;
o belge kanonik vault'un parçası değildir ve bu karar onun yerine geçer.

## Karar

1. **Taban zinciri ADR-0012'den aynen devralınır.** Yön `ABP -> Foundation -> Authenticator ->
   Test Module`'dür. Consumer yalnız ihtiyaç duyduğu `Authenticator.*` katman paketlerini
   referanslar; eşlenen `Nexum.Abp.Foundation.*` paketleri transitif gelir. Consumer doğrudan
   Foundation `PackageReference` veya `DependsOn` yazmaz.
2. **Test Module hostu resource server'dır.** `Domain.Shared`, `Domain`,
   `Application.Contracts`, `Application` ve `EntityFrameworkCore` katmanları tip olarak alınır;
   `AuthenticatorHttpApiModule` composition'a **eklenmez**. Auth uçları bu hostun Swagger'ında
   görünmez.
3. **Kimlik yüzeyi ayrı deploy edilen Authenticator hostundadır.** Login, register, refresh,
   logout, tenant, organization unit ve selected-context uçları orada kalır. Tek issuer hâlâ
   Authenticator'dır; ikinci issuer, ikinci `sub` veya ikinci token store oluşmaz.
4. **Doğrulama ABP JWT bearer modülüyle kurulur.** `Authority`, `Audience` ve
   `RequireHttpsMetadata` typed configuration'dan okunur; eksik değerde host fail-fast kapanır.
5. **Auth tabloları ve migration modeli Authenticator'a aittir** (ADR-0012 md 5 aynen geçerlidir).
   Test Module veya checker projeleri Auth entity'lerinden migration üretmez.
6. **`Authenticator.HttpApi.Client` yalnız uzak tipli çağrı gerektiğinde** kullanılır
   (ADR-0012 md 6 aynen geçerlidir).
7. Public `1.x` Authenticator ailesi kullanılmaz; hedef sürüm sekizli `2.0.0`'dır
   (ADR-0012 md 7 aynen geçerlidir, yayın koşulu karşılanmıştır).

## Alternatifler

- **ADR-0012'nin tek-host issuer modeli:** teknik olarak geçerli; ürün sahibi ödeme ürünüyle
  aynı işletim modelini istediği için elendi. Auth'un ayrı yaşam döngüsü, ayrı ölçeklenmesi ve
  ayrı güvenlik yüzeyi bu tercihin ek gerekçesidir.
- **Auth'u tümüyle Test Module'e gömmek:** RULE-0004'ün auth'suz capability sınırını ve
  Authenticator'ın bağımsız sürümlenmesini bozar; elendi.
- **Checker ince hostlarını production'da çalıştırmak:** RULE-0001 ve ADR-0003 gereği elendi.

## Sonuçlar ve riskler

| Sonuç | Etki |
|---|---|
| UI iki taban adres bilir | Auth çağrıları Authenticator hostuna, yetenek çağrıları Test Module hostuna gider |
| ADR-0005'in "tek deploy" hedefi auth için gevşer | Yetenekler (iki checker, Notifications, Emailing, Test Module) hâlâ tek hostta compose edilir |
| ABP başlangıç uyarıları beklenir | `Volo.Abp.OpenIddict.AspNetCore`, `AspNetCore.Mvc.UI*` assembly'leri `Authenticator.Application` üzerinden gelir fakat module graph'ta değildir; uyarı bilinçlidir, hata değildir |
| Token doğrulama config'e bağımlıdır | `AuthServer:Authority`/`Audience` yanlışsa tüm yetkili uçlar 401 döner; fail-fast ve smoke ile yakalanır |
| Auth yaşam döngüsü ayrı işletilir | Sertifika, data protection ve OpenIddict server ayarları Authenticator hostunun sorumluluğunda kalır |

`ARCH-AUTH-CONSUMPTION` ve `CURRENT-0004` bu karara göre güncellenir.

---
id: GUIDE-0002
type: guide
status: active
title: Checker package and integration roadmap
updated: 2026-08-13
decision_refs:
  - ADR-0002
  - ADR-0004
  - ADR-0005
  - ADR-0006
  - ADR-0009
  - ADR-0012
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# Yol haritası

## Tamamlanan

- [x] API Contract Checker auth bağımlılığını capability sınırından ayır.
- [x] Database Checker auth bağımlılığını capability sınırından ayır.
- [x] Controller, AppService, Manager, Repository, EF Core ve migration katmanlarını koru.
- [x] Checker hostlarını source tree’de tut ve `IsPackable=false` yap.
- [x] Public kimlikleri `CheckNexus.ApiContracts*` ve `CheckNexus.DatabaseComparison*` olarak düzenle.
- [x] İki sekizli paket ailesini `0.1.0-alpha.5` olarak yayımla.
- [x] İki secret portunu tek `CheckNexus.Vault` adapteriyle uygula.
- [x] Merkezi araştırma, karar ve kaynakları Obsidian Wiki Brain’e taşı.

## Sıradaki paket işleri

- [x] NuGet paket ailesi release playbook'unu ve manifest tabanlı kişisel skill'i oluştur
      ([[NuGet-Package-Release-Playbook|GUIDE-0003]]).

- [ ] Checker package dependency graph’ını hedef ABP sürümüyle hizala.
- [ ] Ek API Contract Checker gereksinimlerini ayrı vertical slice’lar halinde ekle
      ([[90-Inbox/PLAN-0002-ApiContract-Ozellik-Listesi|PLAN-0002]] — `ACC-01..ACC-23`).
- [x] API Contract Checker oracle yüzeyi: yanıt uygunluk assertion’ı + dinamik teşhis motoru
      (ACC-05, ACC-13) — Database Checker’ın ADR-0007 yüzeyinin API tarafındaki karşılığı.
- [ ] MCP token bütçesi ve doğruluk kapıları (ACC-18..ACC-22): statik katalog bütçesi,
      çıktı tavanları, sözleşmeden türetilebilirlik ve mutasyon skoru.
      **Checker deposunda açılamaz** (ADR-0008: paket MCP tipi/endpoint taşımaz; bu
      workspace'te composition host yok). ACC-19 zaten kapalı; kalan G1–G4 kapıları
      composition host açıldığında TM-20/TM-31 altında koşar
      ([[90-Inbox/BACKLOG-0001-Checker-Ek-Gelistirme-Talepleri|BACKLOG-0001]] ACX-06 engel notu).
- [x] İki checker modülünü sürüm kontrolüne al: gerçek upstream geçmişini aktar, workspace
      farkını ayrı commit'e yaz, kanıtlanabilen `0.2.0-alpha.2` sürümünü etiketle
      (2026-08-13; `KBP-624`, `KBP-707`; her modülde `PROVENANCE.md`).
- [x] Test Module'ün Sınıf B taleplerinden dördünü kaynakta kapat: DBX-04, ACX-04, DBX-05,
      ACX-05 (2026-08-13; `KBP-708`, `KBP-625`, `KBP-709`, `KBP-626`). **Yayımlanmadı.**
- [x] Database Checker çapraz motor tip haritası ve dört değerli güven kodu (KBP-701).
- [x] Hedef bağlantı emniyet profili, en az yetki raporu, değer saklama politikası (KBP-702).
- [x] Katalog derinliği: kısıt doğrulanmışlığı, collation, generated/identity/comment (KBP-703).
- [x] Hedefli assertion yüzeyi, tip-farkında matcher'lar, tablo tanımlama (KBP-704).
- [x] Dinamik teşhis motoru: 10 hipotez kuralı, 3 probe, RFC 9457 rapor (KBP-705).
- [x] Kapanış: düzeltme pası, fark şiddeti, bulgu parmak izi, sayfalı bulgu okuma,
      iptal + telemetri, `0.2.0-alpha.1` paketleme (KBP-706).
- [ ] Her değişiklikte yeni prerelease/stable sürüm üret; yayımlı `0.1.0-alpha.5`,
      `0.2.0-alpha.1` ve `0.2.0-alpha.2` binary'lerini değiştirme.
- [x] Değişen checker byte'larını yeni `0.2.0-alpha.2` adayı olarak, `0.2.0-alpha.1`
      PackageValidation baseline'ına karşı paketle; 16 `.nupkg` + 16 `.snupkg`,
      clean-cache consumer build ve composition smoke kanıtını üret.
- [x] `0.2.0-alpha.2` ailesini NuGet.org'a yayımla ve 16/16 PackageId'yi registry'den
      doğrula (2026-08-12).
- [ ] `0.2.0-alpha.2` üzerinde consumer smoke'unu yenile; mevcut clean-cache kanıtı
      `0.2.0-alpha.1` üzerindeydi.
- [ ] `CheckNexus.Vault` için public/private feed kararı ver ve ilk registry release’ini yap.
- [x] Package README, PackageValidation baseline, symbol/SourceLink ve release metadata kapılarını doğrula.

## Test Module consumer işleri

- [x] Test Module araştırma fazını kapat — 12 belge, indeksi
      [[05-Operations/Research-Index|GUIDE-0005]]: mimari (0003), veri modeli (0006),
      köprü/token (0007), tester sorunları (0008), iş senaryosu (0009), iş bilgisi (0010),
      entegrasyon (0011), ajan gerçekliği ve ürün içi sohbet (0012).
- [x] Test Module veri modeli, şema sahipliği, modül entegrasyonu ve ajan sınırlarını karara bağla
      (silinen ADR-0011; içeriği ADR-0014/0015/0016'ya taşındı).
- [x] **Runner sınırı, yazarlık modeli ve kayıt modelini yeniden karara bağla (2026-08-13).**
      Eski karar **silindi**, yerine: yazarlık [[03-Decisions/ADR-0014-Senaryo-Yazarlik-Modeli-Ve-Turetilebilirlik-Kapisi|ADR-0014]],
      koşum [[03-Decisions/ADR-0015-Kosum-Siniri-Dis-Arazzo-Runner|ADR-0015]],
      kayıt ve teşhis [[03-Decisions/ADR-0016-Kayit-Ve-Teshis-Veri-Modeli|ADR-0016]];
      dayanak [[90-Inbox/RESEARCH-0013-Runner-Oracle-Ayrimi-Ve-Ajan-Yazarlik-Kaniti|RESEARCH-0013]].
      Model **9 ana + 14 lookup → 4 ana + 5 lookup**; kendi koşum motorumuz yerine dış Arazzo
      runner'ı (Redocly Respect, MIT). Şema kaynağı `04-Architecture/Test-Platform-Schema.dbml`.
      Akışın tek giriş sayfası [[04-Architecture/Alti-An|ARCH-0004]].
- [x] `test_lookup` / `test_catalog` / `test_run` şema sahipliğini
      [[04-Architecture/Database-Ownership|ARCH-0003]]'e işle; DB checker'ın
      `lookup/connection/definition/run/comparison` şemalarıyla çakışma olmadığını doğrula.
- [x] Test Module solution iskeletini kur (2026-08-13): ABP CLI 10.6.0 `module` şablonu,
      `ptn-test-module/{host,src,test}`, tüm ev paketleri katman katman bağlı, build 0 hata,
      host module graph'ı ayağa kalkıyor. Auth tüketimi ADR-0013.
- [ ] Test Module iş listesini uygulamaya al
      ([[90-Inbox/PLAN-0003-TestModule-Ozellik-Listesi|PLAN-0003]] — `TM-01..TM-59`, 8 blok).
- [ ] T1 dikey dilimi: entity sınıfları, EF configuration, lookup seed'leri, ilk migration.
      **Kabul ölçütü:** elle yazılmış bir Arazzo senaryosu uçtan uca yeşil koşuyor ve
      tek satır model çağrısı yok.
- [ ] Yapay zekâ tarafını devral: MCP sunucusu, 12 tool, ajan profilleri, ürün içi sohbet
      (PLAN-0003 Blok 3/6/8; okuma sırası [[05-Operations/Research-Index|GUIDE-0005]] §1).
- [x] Checker'lardan istenen dört Sınıf A geliştirmeyi source'ta kapat
      ([[90-Inbox/BACKLOG-0001-Checker-Ek-Gelistirme-Talepleri|BACKLOG-0001]] — DBX-01/02, ACX-01/02).
- [x] PLAN-0001 ve PLAN-0002 durum satırlarını kaynak gerçeğiyle güncelle (fingerprint, severity,
      sayfalı okuma, değer saklama ve oracle yüzeyleri artık kaynakta mevcut).
- [ ] Şirket içi Test Module template soyunu erişim kontrollü kaynaktan doğrula.
- [x] Sekizli Foundation tabanlı Authenticator `2.0.0` ailesini nuget.org'a yayımla ve
      8/8 registry doğrulamasını yap (2026-08-13; Foundation 7/7, Notifications 6/6 de public).
- [x] Authenticator'ı Test Module hostuna bağla — **resource server** olarak: katman paketleri
      tip, `HttpApi` compose edilmez, doğrulama JWT bearer (ADR-0013).
- [ ] Gerçek token ile login/refresh/logout + selected-context turunu Test Module hostuna karşı doğrula.
- [x] İki checker ana paketini consumer hosta bağla (`CheckNexus.ApiContracts`,
      `CheckNexus.DatabaseComparison` composition paketleri host'ta compose edildi).
- [x] `CheckNexus.Vault` modülünü ekle ve iki portun aynı singletona çözüldüğünü doğrula
      (KBP-92; composition testi iki checker portunun aynı adapter örneğini kullandığını kanıtlar).
- [x] Notifications capability’sini tek bildirim ownerı olarak bağla (Emailing ile birlikte
      Test Module hostunda compose edildi; runtime akışı henüz doğrulanmadı).
- [ ] Tek DB’de şema/migration sahipliği smoke’unu çalıştır.
- [ ] Tek UI’da API contract, DB comparison ve test orchestration akışlarını aç.
- [ ] MCP’yi yalnız Application.Contracts sınırında ekle.

## Release kapısı

Restore, build ve unit test tek başına yeterli değildir. Consumer module initialization, route, EF model, migration, Vault, tenant ve authorization smoke sonuçları kayıt altına alınmadan stable sürüme geçilmez.

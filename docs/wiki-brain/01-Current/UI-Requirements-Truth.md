---
id: CURRENT-0007
type: current
status: active
title: UI gereksinim analizi — aktorler, ekranlar, kapilar ve baslamayi engelleyenler
updated: 2026-08-17
decision_refs:
  - ADR-0013
  - ADR-0014
  - ADR-0016
  - ADR-0019
  - ADR-0020
  - ADR-0023
rule_refs:
  - RULE-0004
  - RULE-0005
  - RULE-0006
  - RULE-0007
  - RULE-0008
---

# UI gereksinim analizi

> Bu sayfa *"ne çizilecek"* değil, **"UI'nin karşılamak zorunda olduğu davranışsal
> gereksinimler nelerdir ve bugün hangileri karşılanamaz"** sorusunu cevaplar. Ekran–uç
> eşlemesi [[04-Architecture/UI-Endpoint-Screen-Matrix|ARCH-0006]]'da, ajan yüzeyi
> [[04-Architecture/UI-Agent-Experience|ARCH-0005]]'te.

## 1. Ürünün UI'ya dayattığı üç değişmez

Bunlar tasarım tercihi değil; ihlal edilirse ürün yanlış çalışır.

| # | Değişmez | Kaynak | UI'daki karşılığı |
|---|---|---|---|
| 1 | **Ajan hakem değildir** | RULE-0005 | Hiçbir ekran model çıktısını "geçti/kaldı" olarak göstermez. Hüküm yalnız `TestOutcomeStatusCodes`'tan gelir |
| 2 | **Ajan tahmin etmez** | RULE-0007 | Ajan girdisi olan hiçbir alan serbest metin değildir; seçim kapalı kümedendir |
| 3 | **Türetilemeyen assertion yayınlanamaz** | RULE-0006 | Adım formunda "beklenti ekle" zorunludur; `assertionPaths` ≥ 1 |

**Sonuç:** UI'nin en riskli ekranı sohbet değil, **onay ekranıdır**. Onayın kolaylaştığı
her yerde kullanıcı denetimi bırakır; sektör ölçümü bunu "aşırı güven" olarak adlandırır
([[90-Inbox/RESEARCH-0017-Ajan-Arayuzu-Desenleri-Ve-Referans-Uygulamalar|RESEARCH-0017]] §4).

---

## 2. Aktörler ve işleri

| Aktör | Ne yapar | Gereken izin kümesi |
|---|---|---|
| **Senaryo yazarı** | Kural/profil yükler, ajanla konuşur, adım onaylar, taslağı kalıcılaştırır | `Bridge.ManageSources` · `Bridge.Ground` · `Bridge.Knowledge` · `Bridge.Profile` · `Scenarios.Create` · `Scenarios.Update` |
| **Onaylayan** (ayrı kişi) | Kapıları okur, yayınlar, karantina yönetir | `Scenarios.Publish` **+** `Scenarios.Approve` · `Scenarios.Quarantine` |
| **Koşum operatörü** | Koşum tetikler, iptal eder, ihraç eder, ortam bağlar | `Runs.Trigger` · `Runs.Cancel` · `Runs.Export` · `Runs.ManageEnvironments` · `Runs.SandboxReset` |
| **Okuyucu / paydaş** | Pano, rapor, bulgu, sağlık | `Runs.View` · `Scenarios` · `Lookups` |
| **Checker yöneticisi** | Spec kaynağı, DB bağlantısı, lookup CRUD | `ApiContractChecker.*` · `DatabaseChecker.*` |
| **Sistem yöneticisi** | Kullanıcı, rol, tenant | `<AUTH_ORIGIN>` — Test Module'de **değil** (ADR-0013) |

> [!CAUTION] Yazar ile onaylayan aynı kişi olamamalı — ama backend bunu zorlamıyor
> `Publish` iki izni birlikte ister (`Approve` + `Publish`) fakat **aynı kullanıcıya ikisi
> birden verilebilir**. Görev ayrılığı bugün yalnız rol dağıtımı disiplinidir. UI bunu
> "güvenlik kontrolü" gibi sunmamalı; en fazla uyarı gösterebilir.

---

## 3. Ekran envanteri — 24 ekran, dört alan

| # | Ekran | Alan | Birincil uç(lar) | Öncelik |
|---|---|---|---|---|
| 1 | Portal panosu | assurance | `runs` · `findings` · `scenario-health` · `coverage` | P0 |
| 2 | Koşum listesi | assurance | `GET runs` | P0 |
| 3 | Koşum detayı + adım zaman çizelgesi | assurance | `GET runs/{id}` · `report` | P0 |
| 4 | Bulgu listesi ve filtre | assurance | `GET findings` | P0 |
| 5 | Teşhis raporu (RFC 9457 + hipotezler) | assurance | `report` · `bridge/explain` | P1 |
| 6 | HAR görüntüleyici | assurance | `runs/{id}/har` | P2 |
| 7 | Artefakt indirme (Ctrf/JUnit/Sarif) | assurance | `export` · `artifacts/{format}` | P1 |
| 8 | Senaryo sağlığı (pass/fail/flaky, p95) | assurance | `scenario-health` | P1 |
| 9 | **Kuru koşum çelişki kartı** | assurance | `dry-run-contradiction` | **P0** |
| 10 | Senaryo listesi ve durum filtresi | authoring | `GET scenarios` | P0 |
| 11 | **Malzeme yükleme** (kural + profil) | authoring | `authoring/business-rules` · `profile-packs` | **P0** |
| 12 | **Ajan sohbet paneli** | authoring | `agent/sessions/*` | **P0** |
| 13 | **Kapalı soru kartı** | authoring | SSE `input_required` → `sessions/{id}/answer` | **P0** |
| 14 | **Adım onay kartı** | authoring | SSE `approval_required` → `sessions/{id}/step` | **P0** |
| 15 | DB adım editörü (kapalı matcher) | authoring | `sessions/{id}/database-step` | P1 |
| 16 | Arazzo belge önizleme + diff | authoring | `GET sessions/{id}` · `compile-preview` | P1 |
| 17 | **Yayın kapısı ekranı** | authoring | `evaluate-publication` → `publish` | **P0** |
| 18 | Karantina / zamanlama | authoring | `quarantine` · `schedule` | P2 |
| 19 | Spec kaynak + snapshot zaman çizelgesi | api-contract | `sources` · `snapshots` | P1 |
| 20 | Kontrat check ve fark kartları | api-contract | `checks/*` | P1 |
| 21 | DB bağlantı + erişim testi | database | `database-connections/*` | P1 |
| 22 | Şema keşfi ve tablo tanımı | database | `schema-discovery/*` | P1 |
| 23 | Ortam bağlama | settings | `environments/*` | P0 |
| 24 | Lookup yönetimi (checker CRUD + TM salt-okunur) | settings | `/api/lookups/*` | P2 |

**P0 = ilk sürümde olmadan ürün anlatılamaz.** Dokuz P0 ekranının **beşi ajan hattındadır**;
yani UI'nin ilk sürümü esasen *"yükle → konuş → onayla → yayınla"* hattıdır.

---

## 4. Kapalı sözlükler — UI bunları uydurmaz, çevirir

UI'da açılır liste, rozet veya durum etiketi olan her şey backend'deki bir kapalı kümeden
gelir. **Hiçbiri UI'da yeniden tanımlanmaz.**

| Sözlük | Değerler | Nerede kullanılır |
|---|---|---|
| `TestRunStatusCodes` | `Pending` `Running` `Completed` `Cancelled` `Aborted` `TimedOut` | Koşum listesi rozeti |
| `TestOutcomeStatusCodes` | `Passed` `Failed` `Broken` `Skipped` **`Inconclusive`** | Hüküm rozeti |
| `TestScenarioStateCodes` | `Draft` `PendingApproval` `Published` `Deprecated` | Senaryo durumu |
| `TestTriggerKindCodes` | `Manual` `Scheduled` `Api` `Webhook` `ContractChange` | "Nasıl başladı" sütunu |
| `ScenarioGateCodes` | 5 kapı kodu | Yayın ekranı |
| `PtnVerdictCodes` | `Confirmed` `Likely` `Possible` `RuledOut` `Inconclusive` | Teşhis kartı |
| `PtnOutcomeCodes` | 23 kod | Bulgu ayrıntısı |
| `PtnDatabaseMatcherCodes` | 11 matcher | DB adım editörü |
| `PtnOpenQuestionCodes` | 5 kod + `NOT_BOUND:` öneki | Kapalı soru kartı |
| `AgentMomentCodes` | 6 an | Sohbet başlatma |
| `PtnToolCodes` | 12 kod (10 discoverable) | Tool rozetleri |
| `RunArtifactFormatCodes` | `Ctrf` `JUnit` `Sarif` | İhracat menüsü |

> [!IMPORTANT] `Inconclusive` bir hata değildir ve öyle gösterilemez
> *"Ön koşul sağlanmadı; ana yol hiç koşmadı, **hiçbir şey doğrulanmadı**"* demektir.
> Kırmızı `Failed` ile aynı renkte gösterilirse kullanıcı var olmayan bir hatayı kovalar.
> Aynı ilke `Unavailable` ve `NOT_BOUND` için de geçerlidir: *"kanıt toplanamadı"* denir,
> *"yetki yok"* denmez (ADR-0019 §C).

**Lookup'ların iki kaynağı var:** kod sabitleri (`Domain.Shared`) ve veritabanı satırları
(`/api/test-module/lookups/*`). UI **veritabanı satırlarını** gösterir (yerelleştirilmiş
`name`/`description` oradadır), kod sabitlerini yalnız dallanma mantığında kullanır.

---

## 5. Karşılanması zorunlu davranışsal gereksinimler

| # | Gereksinim | Neden | Kabul ölçütü |
|---|---|---|---|
| G-01 | Sohbet akışı `input_required`/`approval_required` geldiğinde **durur** ve giriş kutusu kilitlenir | Ajan cevap uyduramaz (RULE-0007) | Kilitliyken `POST /messages` hiç çağrılmaz |
| G-02 | Kapalı sorunun **her biri tam bir kez** cevaplanır | Sunucu sayı eşleşmesi arar | Eksik cevapla gönder butonu pasif |
| G-03 | Onay kartı **dört şeyi** gösterir: ne, neden, ne değişecek, nasıl geri alınır | RESEARCH-0012 §5A.5 | Dördü de boş geçilemez |
| G-04 | Yayın **yalnız** `evaluate-publication` yeşilse denenir | Kapı kodları görünmeden `publish` "sihir" gibi başarısız olur | `publish` butonu kapı sonucuna bağlı |
| G-05 | Kuru koşum kırmızısında **assertion zayıflatma seçeneği yoktur** | RESEARCH-0012 §3.1–3.2 | Ekranda yalnız iki seçenek |
| G-06 | Tur/token bütçesi sohbet başlığında **canlı** görünür | RESEARCH-0012 §3.3 | `completed` olayından güncellenir |
| G-07 | Yazarlık oturumu **TTL sayacı** gösterir (30 dk) | Cache düşünce belge kaybolur | `TtlMs` görünür, %80'de uyarı |
| G-08 | Ortam formunda **sır/parola alanı yoktur** | KBP-112 secret sınırı | Yalnız `secretRef` referansı |
| G-09 | `Result<T>` zarfı **tek yerde** açılır | RULE-0002 (UI) | Feature kodu zarfı görmez |
| G-10 | Çakışan dört `difference-kinds` imzası **çağrılmaz** | Belirsiz route eşleşmesi | İstemcide engellenir |
| G-11 | `POST /api/emailing/emails` son kullanıcıya **açılmaz** | Auth metadata'sı yok | Ekran yok |
| G-12 | Ajan hata metni kullanıcıya **teknik ayrıntı vaat etmez** | ADR-0023 §F redaction | Jenerik metin + destek kimliği |
| G-13 | Her ekran **izinle** koşullanır, gizlenen buton için istek atılmaz | ABP policy | 403 alınması UI hatasıdır |
| G-14 | `Anonymous` webhook ucu UI'dan **hiç çağrılmaz** | Paylaşılan sır sunucu tarafıdır | İstemcide tanımlı değil |

---

## 6. Bugün UI'yi başlatmayı engelleyenler

> [!CAUTION] Dördü **kurulum/kod** blokajıdır; ürün kararı değildir
> Bunlar kapanmadan yazılan UI, ekranı olan ama çalışmayan bir kabuktur.

| # | Engel | Kanıt | Kime ait |
|---|---|---|---|
| ~~E-1~~ | ✅ **Kapandı (2026-08-17, `f23ee3c`)** — on bir capability port'u `[ExposeServices]` aldı; `CapabilityPortWiringTests` gerçek modül grafiğinde hepsini çözüyor | [[90-Inbox/AUDIT-0005-Backend-Teslim-Denetimi\|AUDIT-0005]] B-1 | — |
| ~~E-2~~ | ✅ **Kapandı (2026-08-17, `c7c7773`)** — profil mührünü sunucu üretir: `validate` zaten profil paketini yüklüyordu, aday artık `ProfilePack.ContentFingerprint` ile mühürleniyor; istemcinin farklı hash'i `ProfileFingerprintMismatch` alır | `GroundingManager.CreatePublicationCandidate` | — |
| **E-3** | **Ajan yüzeyinde kimlik doğrulama yok**; tek paylaşılan MCP bearer'ı → tenant izolasyonu ajan sınırında kayboluyor | `ptn-test-agent/src/http/create-server.ts` · `config.ts` | Backend/ajan |
| E-4 | Host `Database:EnsureSharedAbpSchema=true` olmadan **hiç açılmıyor**; bayrak `true` olsa da Authenticator migration'ları uygulanmamışsa ayar/izin yüzeyi 500 verir | `TestModuleHttpApiHostModule.cs:256-259` | Kurulum |
| E-5 | Swagger tek noktadan alınamıyor (E-4'e bağlı) → `openapi-typescript` üretimi çalışmaz | CURRENT-0005 uyarısı | Kurulum |
| E-6 | Ajan oturumu kalıcı değil, listesi yok (`agent_sessions` hâlâ tasarım) | RESEARCH-0012 §4.4 | Ürün kararı |
| E-7 | Canlı uçtan uca koşum kanıtı yok (KBP-115) | CURRENT-0004 | Backend |

> [!NOTE] Kapanmış olanlar — 2026-08-17 kod doğrulaması
> Bu sayfanın ilk sürümü CURRENT-0001 blokaj tablosundan iki engel devralmıştı; **ikisi de
> kapanmış**tır ve kaynaktan doğrulandı:
> **izin yüzeyi** — host `AbpPermissionManagementApplication/EntityFrameworkCore/HttpApi`
> modüllerini compose ediyor (`TestModuleHttpApiHostModule.cs:74-76`);
> **`SpecFingerprint`** — sunucu `snapshot.SpecContent.CanonicalHash`'ten hesaplıyor
> (`TestScenarioAppService.cs:196,220`). CURRENT-0001'in tablosu bu satırlarda eskidir.

**E-1 kapanmadan hiçbir yazarlık, köprü veya koşum ekranı çalışmaz** — düzeltme dokuz dosyada
birer satırdır ama etkisi tüm P0 yüzeyidir. E-2 kapanmadan yayın hattı, E-3 kapanmadan ajan
ekranı üretime çıkamaz.

**Paralel başlatılabilecek işler** (engellerden bağımsız): tasarım sistemi, rota iskeleti,
zarf/hata istemcileri, kapalı sözlük çevirileri, SSE istemcisi ve sohbet durum makinesi —
hepsi sahte (mock) sunucuya karşı yazılabilir. GUIDE-0006 §6 bunu faz olarak ayırır.

---

## 7. Ölçülmemiş ama karar bekleyen dört soru

Bunlar **kod boşluğu değil ürün sorusudur**; UI tasarımı cevap olmadan ilerleyebilir ama
sürüm çıkamaz.

| # | Soru | Neden şimdi |
|---|---|---|
| S-1 | Sohbet geçmişi kalıcı olacak mı? (`agent_sessions` 9. tablo) | Denetim, token faturalandırma ve oturum sürdürme buna bağlı |
| S-2 | Otonomi seviyesi (`Observe`/`Assist`/`Act`) kiracı ayarı olarak açılacak mı? | RESEARCH-0012 §5A.3 önerdi; ayar anahtarı kodda yok |
| S-3 | `senaryo.md` mühre bağlanacak mı, yoksa yalnız prompt malzemesi mi kalacak? | ADR-0020'nin beş malzemesinden biri; bugün backend'de karşılığı yok |
| S-4 | RULE-0008'in DMN satır kapsamı kapısı UI'da gösterilecek mi? | Kural var, kod yok (CURRENT-0001 blokaj 5) |

---

## 8. Yenileme kuralı

Backend uç sayısı, izin ağacı veya kapalı sözlük değiştiğinde bu sayfa ve ARCH-0006 aynı
iş içinde güncellenir. Engel kapandığında satır **silinmez**, "kapandı" olarak işaretlenir
ve kanıt commit'i yazılır (ADR-0001).

---
id: GUIDE-0005
type: guide
status: active
title: Arastirma indeksi — hangi belge hangi soruyu cevapliyor, hangisi guncel
updated: 2026-08-13
decision_refs:
  - ADR-0001
  - ADR-0014
  - ADR-0015
  - ADR-0016
rule_refs:
  - RULE-0005
  - RULE-0006
---

# Araştırma indeksi

`90-Inbox` altında **17 araştırma belgesi, 5 plan, 4 denetim ve 1 backlog** var (2026-08-17
sayımı). Bu sayfa hangisini ne zaman açacağını söyler. **Hepsini okuma.**

> **Yetki hatırlatması (ADR-0001):** `90-Inbox` kanonik değildir. Karara bağlanan her şey
> ADR/Rule/Current sayfalarına taşınır. Bir çelişkide **ADR kazanır**.

---

## 1. Amaca göre okuma sırası

### Projeye yeni katılıyorsan

1. **[[04-Architecture/Alti-An|ARCH-0004]] — ürünün uçtan uca akışı (tek sayfa, buradan başla)**
2. [[05-Operations/Ekip-Kilavuzu|GUIDE-0004]] — sözlük, ürün resmi, kararlar
3. ADR-0014 (yazarlık) · ADR-0015 (koşum) · ADR-0016 (kayıt ve teşhis)
4. `04-Architecture/Test-Platform-Schema.dbml` — şema kaynağı

Araştırma belgelerine **girmene gerek yok**.

### Yapay zekâ / ajan tarafını devralıyorsan

| Sıra | Belge | Neden |
|---|---|---|
| 1 | **RESEARCH-0013** | **Runner-oracle ayrımı ve ajan yazarlık kanıtı** — ölçülmüş tuzaklar (B7/B8), runner taraması, geçişin getirisi |
| 2 | **RESEARCH-0015** | **Ajan gerçeklikleri** — halüsinasyonun kök sebebi, context rot (18/18 model), yerel model F1 tablosu, kanıt zinciri, köprünün on kuralı |
| 2b | **RESEARCH-0016** | **Generic/dinamik köprü** — yetkilendirme teşhisinin ürünleşmiş şekli (GCP/AWS/Zanzibar), semantik katman ölçümü (+17/+23 puan), yazma kümesi üç yolu, progressive disclosure, analyzer + paket modeli. Kararı **ADR-0019**; uygulaması **PLAN-0004** |
| 3 | **RESEARCH-0014** | **Yazarlık hattı ve belirsizlik yönetimi** — assertion neden LLM'den gelmez, Example Mapping soru formatı, iki mod |
| 4 | **RESEARCH-0003** | Temel tez: model neden koşum döngüsünde değil |
| 3 | **RESEARCH-0012** | Ajan yetenek gerçekliği — ölçülmüş sınırlar, ürün içi sohbet |
| 4 | **RESEARCH-0007** | Köprü katmanı ve token ekonomisi — 20 özellik, ölçülmüş kazançlar |
| 5 | **RESEARCH-0009** | İş senaryosu testi — koşullu akış, `Inconclusive`, değişmez kalıpları |
| 6 | ADR-0014 + RULE-0005 + RULE-0006 | Ajan sınırlarının resmî hali |

Bu belgeler yapay zekâ tarafının tamamıdır. Diğerleri checker motorlarıyla ilgilidir.

### UI yazacaksan

| Sıra | Belge | Neden |
|---|---|---|
| 1 | **[[01-Current/UI-Requirements-Truth\|CURRENT-0007]]** | Gereksinimler, kapalı sözlükler, engeller — buradan başla |
| 2 | **[[04-Architecture/UI-Agent-Experience\|ARCH-0005]]** | Ajan yüzeyinin tam sözleşmesi |
| 3 | **[[04-Architecture/UI-Endpoint-Screen-Matrix\|ARCH-0006]]** | Ekran–uç–izin matrisi |
| 4 | **[[03-Decisions/ADR-0025-Ui-Yigini-Ve-Uc-Kokenli-Yuzey-Siniri\|ADR-0025]]** | Yığın kararı (`proposed`) |
| 5 | **[[05-Operations/UI-Build-Guide\|GUIDE-0006]]** | Kurulum ve faz planı |
| 6 | **RESEARCH-0017** | Dış tarama: protokoller, kütüphaneler, UX desenleri |
| 7 | RESEARCH-0012 §4–5A | Ürün içi sohbet ajanı, izin kademeleri, onay ekranı kuralları |

Ajan ekranını yazmadan önce **RULE-0005 · RULE-0006 · RULE-0007** okunur; ajanın
yapamadıkları UI'nin de yapamadıklarıdır.

### Checker motorlarına dokunacaksan

RESEARCH-0001, 0002, 0004, 0005 + PLAN-0001, PLAN-0002.

### Veri modeli / şema işi yapacaksan

**ADR-0016 + `Test-Platform-Schema.dbml`.** Gerekçe arıyorsan RESEARCH-0013 §2 (standart
yakınsaması) ve RESEARCH-0006/0008 (tarihsel).

### Koşum motoruna / runner'a dokunacaksan

**ADR-0015 + RESEARCH-0013 §1-2.** Kendi runner'ımızı yazmıyoruz; nedeni orada.

---

## 2. Belge kataloğu

### Durum kodları

| Kod | Anlamı |
|---|---|
| 🟢 **Aktif** | Güncel referans; hâlâ okunması gereken gerekçe burada |
| 🔵 **Karara bağlandı** | İçeriği ADR'ye taşındı; belge gerekçe arşivi olarak duruyor |
| ⚪ **Uygulandı** | Önerdiği şey kodda; tarihsel kayıt |

### Checker motorları

| # | Belge | Cevapladığı soru | Durum |
|---|---|---|---|
| 0001 | DatabaseChecker Genişletme Analizi | DB checker nasıl genişletilmeli? 14 öneri (E-01..E-14) | ⚪ Uygulandı — assertion, teşhis, fingerprint, severity `0.2.0-alpha.2` ile public |
| 0002 | DbChecker Motor Yetenek Haritası | "Piyasanın en iyisi" ne demek? Yetenek eksenleri | ⚪ Kısmen uygulandı — tip haritası ve katalog derinliği yapıldı; lint ve FK grafiği açık |
| 0004 | Hata Teşhis Motoru | Teşhis nasıl dinamik olur? Kural + sonda + sıralama | ⚪ Uygulandı — 10 kural, 3 sonda, RFC 9457, iki checker'da da |
| 0005 | ApiContract Oracle ve MCP Bütçe Mimarisi | API tarafının oracle yüzeyi ve bütçe kapıları | ⚪ Oracle uygulandı; **MCP bütçe kapıları (ACC-18..22) açık** |

### Test Module — yapay zekâ tarafı

| # | Belge | Cevapladığı soru | Durum |
|---|---|---|---|
| **0003** | MCP Senaryo Testi Mimarisi | **Model neden koşum döngüsünde olmamalı?** Dört an, Arazzo seçimi, token ekonomisi | 🟢 **Aktif** — temel tez, ADR-0014'ün dayanağı |
| 0006 | Test Module Global Tarama ve Veri Modeli | 12 sistem incelemesi; senaryo/koşum/kanıt modeli | 🔵 Karara bağlandı → **ADR-0016 §B** |
| **0007** | Köprü Katmanı ve Token Ekonomisi | Köprü hangi özelliklerle token ucuzlatır? 20 madde | 🟢 **Aktif** → PLAN-0003 Blok 6 |
| 0008 | Tester Sorunları Kapsama Matrisi | Dünyada tester'ı ne yakıyor? 13 sorun, kapsama analizi | 🔵 Karara bağlandı → veri modeli alanları |
| **0009** | İş Senaryosu Testi | Koşullu akış, önkoşul, `Inconclusive`, M-1..M-10 değişmezleri | 🟢 **Aktif** → PLAN-0003 Blok 7 |
| **0010** | İş Bilgisinin Ajana Aktarımı | Ajan iş kurallarını nereden öğrenir? Dört katman, kural kataloğu | 🟢 **Aktif** → PLAN-0003 Blok 8 |
| 0011 | Modül Entegrasyon Deseni | Test Module checker'larla nasıl konuşur? | 🔵 Karara bağlandı → **ADR-0015 §F** |
| **0012** | Ajan Yetenek Gerçekliği ve Ürün İçi Sohbet | **Ajan kaliteyi gerçekten artırır mı?** Ölçülmüş sınırlar, referans uygulamalar | 🟢 **Aktif** → ADR-0014 |
| **0013** | **Runner-Oracle Ayrımı ve Ajan Yazarlık Kanıtı** | **Kendi runner'ımızı yazmalı mıyız? Ajan testi işe yarıyor mu? Geçişin getirisi ne?** | 🟢 **Aktif** — ADR-0014/0015/0016'nın **tek dayanak kaydı** |
| **0014** | **Senaryo Yazarlık Hattı ve Belirsizlik Yönetimi** | **Arazzo hatasız nasıl yazılır? Assertion nereden gelir? Ajan ne sorar, nasıl sorar?** | 🟢 **Aktif** → **ADR-0017** |
| **0015** | **Ajan Gerçeklikleri ve Checker Köprüsü** | **Halüsinasyon neden olur? Bağlam neden kopar? Ollama yapabilir mi? 403'ün sebebi nasıl kanıtla bulunur? Yazarlık için generic yetenek katmanı ne?** | 🟢 **Aktif** → **ADR-0018 + RULE-0007/0008** |
| **0017** | **Ajan Arayüzü Desenleri ve Referans Uygulamalar** | **UI'yi yazarken hangi protokol, kütüphane ve UX deseni işe yarar? AG-UI benimsenmeli mi? Tipler nereden gelir? Arazzo için editör yazılmalı mı?** | 🟢 **Aktif** → **ADR-0025** |

### Denetimler

| # | Belge | Cevapladığı soru | Durum |
|---|---|---|---|
| **0004** | **UI Öncesi Wiki Gerçeklik Denetimi** | **Wiki'nin hangi iddiaları koda ve Git'e karşı tutmuyor?** Depo sınırı, ajan runtime, UI mimarisi, sayım tutarsızlıkları | 🟢 **Aktif** — altı karar sahibini bekliyor |
| **0005** | **Backend Teslim Denetimi** | **UI ve ajan geliştiricisine devretmeden önce backend'de hata, eksik veya yanlış var mı?** 1 Blocker, 6 Risk, 6 Nit; denetim anında build 0 hata / test 383/383 | 🟢 **Aktif** — **B-1, R-1, R-2, R-4 kapandı (2026-08-17); test 392/392.** Kalan: feed kimliği, ajan yüzeyi kimlik doğrulaması, N-5 |

### Planlar ve backlog

| Belge | İçerik | Durum |
|---|---|---|
| PLAN-0001 | DB Checker özellik listesi (`DBC-xx`) | Blok 0–2 uygulandı; Blok 3–6 açık |
| PLAN-0002 | API Checker özellik listesi (`ACC-xx`) | Blok 0–3 uygulandı; Blok 4 (MCP bütçe) açık |
| **PLAN-0003** | **Test Module özellik listesi (`TM-01..TM-59`)** | **Tamamı açık** — uygulanacak iş listesi |
| BACKLOG-0001 | Checker'lardan istenen ek geliştirmeler | Sınıf A kapandı (`0.2.0-alpha.2`); Sınıf B/C açık |

---

## 3. Yapay zekâ tarafını devralan ekip için özet

### Karara bağlanmış olanlar — tartışma kapalı

| Karar | Nerede |
|---|---|
| Koşum ve yargı anlarında model **yok** | **RULE-0005**, RESEARCH-0003 |
| Hakem her zaman checker, asla model | **RULE-0005** |
| Türetilemeyen assertion yayınlanamaz; `assertion_count > 0` | **RULE-0006** |
| **Kendi koşum motorumuz yok** — dış Arazzo runner (Respect, MIT) | **ADR-0015 §A** |
| **DB assertion'ı bir Arazzo adımıdır**; runner'a plugin yazılmaz | **ADR-0015 §C** |
| Response uygunluğu HAR'dan **her** adım için; DB assertion **koşum sırasında** | **ADR-0015 §D** |
| Kayıt sahibi tek: Respect kontrolleri `warn`, checker `error` | **ADR-0015 §E** |
| Model **4 ana tablo + 5 lookup**; ortam tablosu yok | **ADR-0016** |
| Ajanın girdileri: kurallar + sözleşme + şema. Çalışan sistemi **görmez** | **ADR-0014 §A** |
| `kurallar.md` tablo değil, MCP `Resource` | **ADR-0014 §A** |
| İki belge saklanır: `source_document` (onaylanan) + `compiled_document` (koşan) | **ADR-0014 §C** |
| MCP yüzeyi composition host'ta | ADR-0008 |
| Senaryo formatı Arazzo, yama formatı Overlay | RESEARCH-0003 §5.2, RESEARCH-0009 §2.3 |
| Dört kademeli izin modeli, kademe 4 zorunlu onay | **RULE-0005** |
| Otonomi seviyesi kiracı ayarı | **RULE-0005** |
| Onay içerik hash'ine bağlı | **RULE-0005**, ADR-0014 |
| `dryRun` başarısızlığı ajana düzeltme yetkisi vermez | **RULE-0005**, ADR-0014 §E |
| Telemetri sözlüğü OpenTelemetry GenAI | **RULE-0005** |
| Hash zincirli denetim logu v1'de **yok** | **RULE-0005** |

### Açık kalan işler — ekibin çalışacağı alan

| # | İş | Kaynak |
|---|---|---|
| TM-17..20 | Yazım ajanı, MCP tool'ları, onay akışı | PLAN-0003 Blok 3 |
| TM-32..40 | Köprü katmanı ve token bütçe kapıları | PLAN-0003 Blok 6 |
| TM-51..59 | İş bilgisi katmanı (sözlük, kural kataloğu, etki ayak izi) | PLAN-0003 Blok 8 |
| ACC-18..22 | API checker MCP bütçe ve doğruluk kapıları | PLAN-0002 Blok 4 |
| — | Ürün içi sohbet ajanı (`agent_sessions`) | **RULE-0005**, RESEARCH-0012 §4 |
| F0–F5 | **UI portalı** — 24 ekran, altı faz | [[05-Operations/UI-Build-Guide\|GUIDE-0006]] §6 |
| E-1..E-7 | UI'yi engelleyen kurulum/kod blokajları | [[01-Current/UI-Requirements-Truth\|CURRENT-0007]] §6 |

### Ölçüm yükümlülüğü

RESEARCH-0007 §7 ve RESEARCH-0012 §1 gereği her iddia ölçülmelidir:

| Ölçüt | Hedef |
|---|---|
| Tool kataloğu statik maliyeti | ≤ 3.000 token |
| Yazım anı (A) | ≤ 15.000 token / senaryo |
| **Koşum anı (B)** | **0** |
| Teşhis anı (C) | ≤ 5.000 token / kırmızı koşu |
| Bakım anı (D) | ≤ 2.000 token / bulgu |
| Tek tool yanıtı | ≤ 8.000 token |

`agent_sessions` tablosu bu ölçümlerin kanıtıdır.

---

## 4. Bilinen çelişkiler — ADR kazanır

| Belge | Belgedeki eski hali | Geçerli olan |
|---|---|---|
| RESEARCH-0006 §5.2, §5.3–5.6, §9 | Şema adları `testlookup` / `testcatalog` / `testrun` | **`test_lookup` / `test_catalog` / `test_run`** (ADR-0016 §A) |
| RESEARCH-0006 §5 | 18+ tablo, `finding_links` ve `scenario_health` ayrı tablo | **9 ana tablo + 14 lookup**; ilgili kavramlar owned jsonb'ye toplandı (ADR-0016 §B) |
| RESEARCH-0006 §5.7 | `run_steps` aylık partition | **v1'de partition yok**; parçalı silme + 50M satır eşiği (ADR-0016 §B) |
| RESEARCH-0009 §2.3 | `scenario.dryRun` sonucu ajana geri beslenir | **`dryRun` başarısızlığı ajana düzeltme yetkisi vermez** (**RULE-0005**, RESEARCH-0012 §3.1) |
| RESEARCH-0012 §4.2 | Üç kademeli izin modeli | **Dört kademe**; sandbox yazımı kademe 3 (RESEARCH-0012 §5A.2, RULE-0005) |
| RESEARCH-0012 §4.4 | `agent_sessions` 9. tablo olarak eklenecek | **ADR-0016'da yok** — model 4 ana + 5 lookup. Sohbet kalıcılığı **açık ürün sorusudur** (CURRENT-0007 S-1) |
| RESEARCH-0012 §4.6 | "Ajan oturum açan kullanıcı adına çalışır; ABP izinleri ve tenant filtresi geçerli" | **Kodda yok.** Ajan yüzeyi kimliksiz, MCP'ye tek paylaşılan bearer ile bağlanıyor (AUDIT-0004 §D) |
| ARCH-0007 §5A | Test Module `Volo.Abp.Identity`/`TenantManagement` yükler; UI `/api/identity/*`'ı oraya gönderir | **Resource server**; auth controller sayısı **0**, kimlik `<AUTH_ORIGIN>`'de (ADR-0013, AUDIT-0004 §B3) |
| ARCH-0007 §2 | Tek `gen:api`, tek `schema.d.ts` | **Üç köken**; ajan yüzeyi OpenAPI yayınlamıyor (ADR-0025 §C) |

### ADR-0014/0015/0016 ile doğan yeni çelişkiler (2026-08-13)

> **ADR-0011 dosyası silinmiştir.** Aşağıdaki "Eski ADR-0011 §X" satırları yalnız tarihsel
> kayıttır; o belgeyi aramayın, geçerli kararlar sağ sütundadır.

| Belge | Belgedeki eski hali | Geçerli olan |
|---|---|---|
| Eski ADR-0011 §A | Üç şema, `test_catalog` geniş | `test_catalog` **tek tablo** (ADR-0016 §A) |
| Eski ADR-0011 §B | 9 ana tablo + 14 lookup | **4 ana tablo + 5 lookup** (ADR-0016 §B) |
| Eski ADR-0011 §B | `scenario_executions.Steps` owned jsonb | Adım kaydı **yok**; HAR artefaktı (ADR-0015, ADR-0016) |
| Eski ADR-0011 §B | `agent_sessions` tablosu | **Yok**; ölçüm `test_scenarios.authored_by_agent` + `agent_model_ref` (ADR-0014 §F) |
| RESEARCH-0003, 0006 | Kendi adım koşum motorumuz | **Dış runner** (ADR-0015 §A) |
| RESEARCH-0006 §8 | Oracle koşum sırasında çağrılır | Response uygunluğu **HAR'dan**, DB assertion **Arazzo adımı** (ADR-0015 §D) |
| RESEARCH-0010 | İş bilgisi veritabanı tablolarında | **Git + MCP `Resource`**; koşuda `rules_fingerprint` (ADR-0014 §A) |
| RESEARCH-0009 §2.3 | `x-checknexus-branch` uzantısı | Arazzo'nun kendi `onSuccess`/`onFailure` + `criteria` + `goto` mekanizması yeterli |

Bu çelişkiler belgelerde **düzeltilmez** — araştırma belgesi o günkü düşünceyi kaydeder.
Geçerli olan her zaman ADR'dir.

---

## 5. Bakım kuralı

Bir araştırma belgesi ADR'ye taşındığında:

1. Bu indekste durumu 🔵 yapılır ve hangi ADR bölümüne gittiği yazılır
2. Belge **silinmez** — gerekçe arşivi olarak kalır (ADR-0001)
3. Belge ile ADR çelişirse **ADR kazanır**; çelişki bu sayfaya not edilir

Yeni araştırma eklendiğinde bu indekse satır eklenir. İndekste görünmeyen belge
**yok sayılır**.
</content>

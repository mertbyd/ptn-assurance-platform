---
id: RESEARCH-0012
type: research
status: draft
title: Ajan yetenek gercekligi — olculmus siniriler, tasarimi degistiren uc bulgu ve urun ici sohbet ajani
updated: 2026-08-12
decision_refs:
  - ADR-0007
  - ADR-0008
rule_refs:
  - RULE-0004
---

# Ajan yetenek gerçekliği ve ürün içi sohbet ajanı

> [!NOTE] Kararlar RULE-0005'dedir
> §3 ve §5A'daki kararlar
> **RULE-0005**'e
> taşındı. **§4.2'deki üç kademeli izin modeli geçersizdir** — geçerli olan §5A.2'deki
> **dört kademeli** modeldir. Ölçüm verileri (§1) ve referans uygulama analizi (§5A) hâlâ
> aktif referanstır; yapay zekâ tarafını devralan ekibin okuma listesinde birinci sıradadır.

> Kanonik değildir. İki soruyu cevaplar:
> **(1)** Ajan kullanmak test kalitesini gerçekten artırır mı — ölçülmüş verilerle, hayalperest olmadan?
> **(2)** Ürün içinde bir sohbet ajanı nasıl kurulur, ne yapabilir, ne yapamaz?
>
> Kanıt sınıfları: **K1** çalışan kod · **K2** hakemli/akademik ölçüm · **K3** sektör ölçümü.

---

## 0. Özet hüküm

Ajan **belirli koşullarda** kaliteyi artırır, **belirli koşullarda düşürür**. Fark tesadüf değil,
ölçülmüş üç değişkene bağlı:

| Değişken | Etkisi |
|---|---|
| **Gereksinim ayrıntısı** | Belirsiz gereksinimde mutasyon skoru **26–40 puan** düşüyor |
| **Kodun eğitim verisinde olup olmaması** | Özel kod tabanında skor **%2,4–10,3**, insan tabanı **%30,4** |
| **Görev uzunluğu** | Tek adımda %80–90, çok adımlı zincirde **%18–24** |

Bizim mimarimiz üçünde de **doğru tarafta** duruyor — ama bu tesadüf değil, tasarım kararlarının
sonucu. Ve ölçümler **üç yerde tasarımın sertleştirilmesi** gerektiğini gösteriyor (§3).

---

## 1. Ölçülmüş gerçeklik

### 1.1 LLM'ler test üretirken kestirme yapıyor (SAP HANA + LevelDB, K2)

Dört model (GPT-5, Claude 4 Sonnet, Qwen3-Coder, Gemini 2.5 Pro) iki kod tabanında sınandı.

| Kod tabanı | Mutasyon skoru | Not |
|---|---|---|
| **LevelDB** (açık kaynak, eğitim verisinde) | **%100** (hepsi) | Ama **neredeyse birebir kopyalama** kanıtı bulundu |
| **SAP HANA** (özel, eğitim verisinde **yok**) — yalnız kaynak kod | **%2,39 – 10,25** | |
| SAP HANA — bağımlılık bağlamı da verilince | %10,60 – 25,14 | Bağlam **%150'ye kadar** iyileştirme |
| **İnsan tabanı** | **%30,41** | |

**Tespit edilen üç kestirme:**

1. **Ezber, genelleme değil.** Açık kaynak kodda mükemmel skor, ama gerçek muhakeme değil kopyalama.
2. **Derlenebilirlik uğruna kaliteyi feda etme.** Derleyici geri bildirim döngüsünde modeller
   **assertion'ları kaldırıp boş test gövdeleri üretti**. GPT-5 SAP HANA'da %99 derleme başarısına
   ulaştı ama bu *"kodun gerçek anlamda muhakeme edildiğini yansıtmadı."*
3. **Yapısal bilgi yokluğu.** Özel kod tabanında var olmayan API çağrıları uydurdu, gerekli
   header'ları atladı.

**Yazarların sonucu:** LLM'ler *"sağlam dünya modellerinden yoksun"*, yüzeysel sezgileri kullanıyor.

### 1.2 Gereksinim ayrıntısı belirleyici (RESTestBench, K2)

Üç REST servisi, 106 doğrulanmış gereksinim (belirsiz ve kesin olmak üzere iki ayrıntı düzeyi),
228 gereksinim-hizalı mutasyon.

| Koşul | Mutasyon skoru |
|---|---|
| **Kesin** gereksinim | %13 – 92 (sınır modeller yüksek) |
| **Belirsiz** gereksinim | **26–40 puan düşüş**; en güçlü modeller %49–54'te toplanıyor |
| Llama 3.1 8B, belirsiz gereksinim | **%2** |
| Hiçbir model, belirsiz gereksinimde | **%90'ı geçemedi** |

### 1.3 ⚠️ En kritik bulgu: bozuk sisteme karşı iyileştirme oracle'ı bozuyor (K2)

Aynı çalışma: test üreteci **hatalı (mutasyona uğramış) bir implementasyonla** etkileşince
etkinlik **sistematik olarak düştü**. Modeller **assertion'ları hatalı davranışa uyacak şekilde
uyarladı**.

> Dört model için, **hatalı implementasyona karşı iyileştirme yapmak, hiç etkileşmeden tek seferde
> üretmekten daha kötü sonuç verdi.**

**Bu bulgu bizim `scenario.dryRun` tasarımımıza doğrudan tehdittir** (§3.1).

### 1.4 Maliyet-etkinlik sürprizi (K2)

| Model | Mutasyon skoru | Koşu başına maliyet |
|---|---|---|
| **GPT-5 Nano** | **%70** | **$0,41** |
| Sonnet 4.5 | %65 | $10,13 |

Küçük model **daha iyi skor**, **25 kat ucuz**. İyileştirme döngüsü toplam maliyeti 2–4 kat artırıyor.

### 1.5 Ajan güvenilirliği görev uzunluğuyla çöküyor (K3)

| Görev tipi | Başarı |
|---|---|
| Tek adım | %80 – 90 |
| **Çok adımlı, uygulamalar arası zincir** | **%18 – 24** |

Matematik acımasız: adım başına **%85 güvenilir** bir ajan, **10 adımda** uçtan uca yalnız
**~%20** başarılı olur.

Baskın hata modları: **alt-planlama hataları** ve **felaket unutma**; erken sapmalar sonraki
adımlara yayılıyor.

**Ve bizim için en önemli sayı:** 580 senaryoluk bir kıyaslamada **guardrail'ler hataların
%19,9'unu kurtardı** — yapılandırılmış yaklaşımlar basit müdahalelerden üstün.

### 1.6 Yerel modeller (K3)

Tool çağırmada en kararlı yerel aile Qwen3; `Llama-3-Groq-70B-Tool-Use` BFCL'de **%90,76**.
Ama RESTestBench'te **Llama 3.1 8B belirsiz gereksinimde %2**.

**Yorum:** "tool çağırabiliyor" ile "iyi test yazabiliyor" **aynı şey değil**. BFCL tek çağrının
biçimsel doğruluğunu ölçüyor; test kalitesi muhakeme istiyor.

---

## 2. Bu veriler bizim mimarimize ne diyor?

### 2.1 Doğrulanan kararlar

| Bulgu | Bizim kararımız | Sonuç |
|---|---|---|
| Gereksinim ayrıntısı 26–40 puan fark yaratıyor | **K-3 iş bilgisi katmanı** (sözlük, karar tabloları, yolculuklar) | ✅ En büyük bahsimiz ölçümle doğrulandı |
| Özel kod tabanında skor 1/3'e düşüyor | Modele **kod okutmuyoruz**; sözleşme + şema + kural veriyoruz | ✅ Yapısal bilgi boşluğunu bilgi katmanıyla dolduruyoruz |
| Çok adımlı zincirde %18–24 | Ajan görevi **5–8 tool çağrısı**, uzun ufuk yok | ✅ Kısa görev tasarımı |
| Guardrail'ler hataların %19,9'unu kurtarıyor | **Dört deterministik kapı** | ✅ Kapı ağırlıklı tasarım |
| Modeller assertion silerek derlenebilirlik kovalıyor | `assertion_count` + `ValidateScenarioAssertions` | ⚠️ Yeterli değil (§3.2) |
| Küçük model 25× ucuz, benzer skor | **Sağlayıcı portu** | ✅ Kapı açık |

### 2.2 Bizim yapısal avantajımız

Bu çalışmaların çoğu **kod okuyup unit test yazan** modelleri ölçüyor. Bizim görevimiz farklı:

| Onların görevi | Bizim görevimiz |
|---|---|
| Kod tabanını anla | **Anlama — sözleşme + şema + kural verilmiş** |
| Beklenen davranışı çıkar | **Karar tablosundan oku** |
| Assertion uydur | **M-1..M-10 kalıp kataloğundan seç** |
| Doğruluğu kimse kontrol etmiyor | **Dört kapı + insan onayı** |

SAP HANA çalışmasının *"bağlam eklemek skoru %150'ye kadar iyileştirdi"* bulgusu tam olarak
bizim bilgi katmanımızın yaptığı şeydir.

---

## 3. ⚠️ Tasarımı DEĞİŞTİREN üç bulgu

### 3.1 `dryRun` geri beslemesi ajana **otomatik** verilemez

**Bulgu (§1.3):** Modeller bozuk sisteme karşı iyileştirme yaparken assertion'ları **hataya uyacak
şekilde** değiştiriyor. Dört modelde bu, hiç etkileşmemekten **daha kötü** sonuç verdi.

**Bizdeki risk:** Akış şuydu — ajan senaryo yazar → `dryRun` koşar → sonuca göre düzeltir.
Uygulamada gerçek bir hata varsa ajan **senaryoyu hataya uydurur** ve test sonsuza kadar yeşil kalır.
Bu, tam olarak kaçınmaya çalıştığımız "sessiz yanlış güven" durumudur.

**Karar:**

| Eski | Yeni |
|---|---|
| `dryRun` sonucu ajana geri beslenir, ajan düzeltir | `dryRun` **başarısızlığı ajana düzeltme yetkisi vermez** |

**Yeni kural:**
1. `dryRun` adımı kırmızıysa sonuç **çelişki bildirimi** olarak döner:
   *"Adım 5 başarısız. İki olasılık var: (a) senaryo yanlış, (b) uygulamada hata var. Karar insanın."*
2. Ajan **assertion'ı zayıflatarak** ilerleyemez.
3. İnsan seçer: *"senaryoyu düzelt"* veya *"bu gerçek bir hata, senaryo doğru — bug kaydı aç"*.
4. `dryRun` sonrası **assertion sayısı azalmışsa** yayın kapısı bunu **reddeder**.

**Ek veri alanı:** `scenarios.versions[].dryRunConflict` — çelişkinin nasıl çözüldüğü kaydedilir.

### 3.2 Assertion zayıflaması tespit edilmeli

**Bulgu (§1.1):** Modeller derlenebilirlik uğruna assertion siliyor, boş gövde üretiyor.

**Bizdeki karşılığı:** Onarım önerisi (moment D) bir `successCriteria` bloğunu **kaldırarak**
testi yeşile döndürebilir.

**Karar — üç kontrol:**

| Kontrol | Kural |
|---|---|
| **Minimum assertion** | Her senaryo adımının ≥ 1 beklentisi olmalı; `assertion_count = 0` yayınlanamaz |
| **Zayıflama tespiti** | Yama önerisi assertion **sayısını azaltıyorsa** veya bir matcher'ı daha gevşek olanla değiştiriyorsa (`equals → isNotNull` gibi) **ayrı uyarı** ile işaretlenir |
| **Onay metni** | Zayıflatan yama için insana özel uyarı: *"Bu yama doğrulamayı zayıflatıyor. Kabul edilirse şu kontrol kaybolur: …"* |

**Ek veri alanı:** `heal_proposals[].weakensAssertion bool` + `weakenedChecks []`

### 3.3 Ajan turu sayısı sert sınırlanmalı

**Bulgu (§1.5):** %85 güvenilir ajan 10 adımda ~%20'ye düşüyor.

**Karar:** An bazında **sert tur sınırı** ve aşımda **başarısızlık**, sessiz devam değil.

| An | Maks tur | Aşımda |
|---|---|---|
| A — yazım | **8** | Taslak "eksik" olarak insana döner |
| C — teşhis | **4** | Ham teşhis raporu gösterilir |
| D — bakım | **5** | Öneri üretilmez, bulgu insana bildirilir |
| Sohbet | **10** | Oturum kapatılır, özet bırakılır |

**Ek veri alanı:** `agent_sessions.turn_limit_hit bool`

---

## 4. Ürün içi sohbet ajanı

### 4.1 Ne olacak

Test Module arayüzünde bir **sohbet paneli**. Kullanıcı iş diliyle konuşur; ajan senaryoları,
kuralları ve koşuları inceler, önerir ve **izinli eylemleri tetikler**.

```
Tarayici                          Test Module Host
┌────────────────┐               ┌─────────────────────────────────┐
│ Sohbet paneli  │ ── HTTP ────► │ AgentSessionAppService          │
│                │               │   └─► Agent Runner              │
│                │               │         ├─► Model saglayici     │
│                │               │         │   (port arkasinda)    │
│                │               │         └─► MCP tool'lari       │
│                │               │             (in-process)        │
└────────────────┘               └─────────────────────────────────┘
```

**Kritik:** ajan runner MCP tool'larını **süreç içinde** çağırır — HTTP yok, ağ yok.
Tool'lar zaten aynı AppService'leri çağırıyor, yani doğrulama ve yetkilendirme tek yerde.

### 4.2 Ajanın yapabilecekleri — üç kademe

| Kademe | Eylem | Onay | Örnek |
|---|---|---|---|
| **1. Serbest okuma** | Sorgu, listeleme, açıklama | Yok | *"BR-014 kuralını göster"*, *"dün ne kırıldı"* |
| **2. Kayıtlı eylem** | Geri alınabilir, denetlenen | Yok ama **kayıt** | *"gece koşusunu tetikle"*, *"kuru koşum yap"* |
| **3. Onay bekleyen** | Kalıcı etki | **İnsan onayı zorunlu** | Senaryo yayınlama, yama uygulama |

**Kademe 2 neden onaysız:** koşu tetiklemek **geri alınabilir** (iptal edilebilir), **yıkıcı değil**
(sandbox'ta koşuyor) ve **denetleniyor** (kim tetikledi kayıtlı). Her tetikleme için onay istemek
sohbeti kullanılamaz hale getirir.

**Kademe 3'ün sınırı sert:** `scenario.save` **yalnız** `PendingApproval` durumuna yazabilir.
`Published`'a yazma yolu ajanın tool kataloğunda **yok** — kandırılsa bile yapamaz.

### 4.3 Tetikleme akışı

```
Kullanici: "bilet senaryolarini staging de kostur"
   │
   ├─► scenario.search(tags:["booking"])          [kademe 1]
   │      ← 7 senaryo bulundu
   │
   ├─► "7 senaryo bulundum. staging ortaminda kosturayim mi?"
   │      (ajan ozet verir, kullanici onaylar — UX geregi, izin geregi degil)
   │
   ├─► run.trigger(scenarioIds, envKey:"staging")  [kademe 2]
   │      ← taskId: t-8842, durum: working
   │
   ├─► [MCP Tasks ile yoklama]
   │      ← 7/7 tamamlandi: 6 gecti, 1 Inconclusive
   │
   └─► "6 gecti. 1 tanesi Inconclusive — o saatte sefer bulunamadi,
        yani hicbir sey dogrulanmadi. Veri kumesini kontrol etmek ister misin?"
```

### 4.4 Yeni tablo: `agent_sessions`

Sohbet ajanı **kalıcı oturum** gerektiriyor. Bu, modeldeki **9. tablo**.

| Alan | Tip | Açıklama | Örnek |
|---|---|---|---|
| `id` | uuid | Oturum kimliği | `s-4471…` |
| `tenant_id` | uuid | **ZORUNLU** — oturum listesi doğrudan sorgulanıyor | |
| `user_id` | uuid | Ajan kimin adına çalıştı | `ayse-id` |
| `profile_code` | varchar(32) | Hangi ajan profili | `Chat` / `Authoring` / `Diagnosis` / `Maintenance` |
| `status_id` | uuid FK | Oturum durumu | → `Completed` |
| `started_at`, `ended_at` | timestamptz | | |
| `turn_count` | int | Kaç tur döndü | `6` |
| `turn_limit_hit` | bool | Tur sınırına takıldı mı | `false` |
| `input_tokens` | int | Girdi token'ı | `18420` |
| `output_tokens` | int | Çıktı token'ı | `3180` |
| `model_ref` | varchar(64) | Hangi model kullanıldı | `claude-opus-5` |
| `related_scenario_id` | uuid? | İlgili senaryo | |
| `related_run_id` | uuid? | İlgili koşu | |
| `messages` | jsonb | Konuşma (owned; 32 KB tavanı, aşarsa blob) | |
| `tool_calls` | jsonb | Hangi tool'lar çağrıldı, sonuçları | |

`INDEX(tenant_id, started_at)` · `INDEX(user_id, started_at)` · `INDEX(related_scenario_id)`

**Üç sebep:**
1. **Denetim** — *"koşuyu kim tetikledi"* sorusu cevaplanabilmeli
2. **Maliyet** — kiracı başına token faturalandırması
3. **Sürdürme** — oturum kapanıp açılınca devam edebilme

### 4.5 Maliyet ölçümü ve bütçe

| Ölçüt | Hedef | Aşımda |
|---|---|---|
| Sohbet oturumu | ≤ 25.000 token | Oturum kapanır, özet bırakılır |
| Kiracı başına günlük | Ayar ile sınırlı | Uyarı, sonra durdurma |
| Tur sayısı | Profil başına (§3.3) | Başarısızlık, sessiz devam yok |

`agent_sessions` token alanları bu bütçelerin kanıtıdır — RESEARCH-0007 §7'deki "ölçülmeyen iddia
temennidir" ilkesinin uygulaması.

### 4.6 Güvenlik

| Risk | Önlem |
|---|---|
| Prompt injection (senaryo metnine gömülü talimat) | Ajanın yazma yolu yok; kademe 3 insan onayı ister |
| Aşırı yetki | Tool kataloğunda yayınlama/onaylama tool'u **yok** |
| Kiracı sızıntısı | Ajan oturum açan kullanıcı adına çalışır; ABP izinleri ve tenant filtresi geçerli |
| Maliyet kaçağı | Tur sınırı + token bütçesi + oturum tavanı |
| Ham veri sızıntısı | `redactionMode: None` varsayılan; kanıt maskeli |

---

## 5. Hüküm: ajan kaliteyi artırır mı?

**Evet — ama koşullu, ve koşulları biz zaten sağlıyoruz.**

| Koşul | Ölçülmüş etki | Bizde |
|---|---|---|
| Gereksinim/bilgi ayrıntılı olmalı | 26–40 puan fark | ✅ K-3 bilgi katmanı |
| Model kodu ezberlememiş olmalı | Özel kodda 1/3 skor | ✅ Koda hiç bakmıyoruz |
| Görev kısa olmalı | 10 adımda %20'ye düşüş | ✅ 5–8 tool çağrısı, sert tur sınırı |
| Guardrail olmalı | Hataların %19,9'u kurtarılıyor | ✅ Dört kapı |
| Bozuk sisteme uydurmamalı | İyileştirme etkinliği düşürebiliyor | ⚠️ **§3.1 ile sertleştirildi** |
| Assertion zayıflamamalı | Modeller assertion siliyor | ⚠️ **§3.2 ile sertleştirildi** |

**Ajanın gerçek katkısı nerede:**

| Katkı | Gerçekçi beklenti |
|---|---|
| Senaryo **taslağı** üretme | Yüksek — iskelet + doğru bağlama, insan rötuşlar |
| Terim/tablo/operasyon **eşleme** | Yüksek — bilgi katmanı varsa |
| Teşhis **anlatımı** | Yüksek — checker cevabı zaten üretiyor |
| Etkilenen senaryo **yaması** | Orta — dar girdi, dar çıktı |
| **Hata bulma** | **Düşük** — hatayı ajan değil, deterministik oracle bulur |

**Son satır önemli:** ajan bizde **hata bulmuyor**. Hata bulan şey iki checker ve iş değişmezleri.
Ajan sadece **o kontrolleri yazmayı hızlandırıyor**. Bu ayrım korunduğu sürece ölçümlerdeki
olumsuz bulguların çoğu bizi vurmuyor.

---

## 5A. Referans uygulamalar ve standartlar (derin tarama)

§4'teki ilk tasarım ilk ilkelerden türetilmişti. Bu bölüm onu **fiilen çalışan ürünlere ve
yayımlanmış standartlara** karşı sınar ve dört yerde düzeltir.

### 5A.1 Sentry Seer — aynı temel, iki iş akışı

Sentry'nin üretimdeki hata ayıklama ajanı Seer, iki farklı ürün yüzeyini **tek ajan
mimarisi** üzerinde çalıştırıyor (K3):

> *"Seer Agent ve Autofix, aynı temel üzerine kurulmuş iki iş akışıdır — aynı veri,
> aynı ajan mimarisi."* Autofix **dar ve tanımlı** bir probleme odaklı; Seer Agent
> istenildiği kadar geniş olabiliyor.

Ve kritik ürün kararı: **kuruluşlar, insan devreye girmeden önce Seer'in ne kadar ileri
gideceğini kontrol edebiliyor.**

Seer üretim telemetrisi (hata, iz, log, metrik) üzerine kurulu; bu **çalışma zamanı bağlamı**
statik kod analiziyle güvenilir şekilde teşhis edilemeyecek hataları çözebilmesini sağlıyor.

**Bizim için üç ders:**

| Ders | Bizdeki karşılığı |
|---|---|
| Tek ajan temeli, çok iş akışı | Dört an (A/C/D/Sohbet) **tek runner** üzerinde; ayrı ajan uygulamaları yazmıyoruz |
| Otonomi seviyesi **kiracı ayarı** | Yeni ayar: `AgentAutonomyLevel` (§5A.3) |
| Çalışma zamanı telemetrisi > statik analiz | Bizim karşılığımız iki checker'ın canlı gözlemi; aynı tez |

### 5A.2 Dört kademeli izin modeli — üçlü model düzeltildi

§4.2'de üç kademe önermiştim. Üretimde yaygınlaşan model **dört kademe** ve ayrım
**işlem kategorisine göre değil, geri alınabilirlik ve etki yarıçapına göre** yapılıyor (K3):

| Kademe | Tanım | Gözetim modu | Bizdeki örnek |
|---|---|---|---|
| **1 — Salt okuma** | Dış dünyada yan etkisi yok | **Kesintisiz** — *"bunları kapıya bağlamak yalnız onay yorgunluğu üretir"* | `knowledge.lookup`, `run.get`, `scenario.search` |
| **2 — Geri alınabilir** | Taslak, iç durum değişikliği, temiz geri alınabilir | Serbest ama **her eylem geri alınabilecek bağlamla loglanır** | `scenario.dryRun`, `run.trigger` (iptal edilebilir) |
| **3 — Dış sisteme dokunan** | Üçüncü tarafa/dışarı etki | **Kuyruğa alınır** veya güven sinyaline bağlanır | Hedef sisteme yazan sandbox seed'i |
| **4 — Geri alınamaz** | Kalıcı, geri dönüşü yok | **Zorunlu insan onayı.** *"Yüksek güven, bir ajana denetimsiz geri alınamaz eylem hakkı satın almaz"* | Senaryo yayınlama, yama uygulama, karantina kaldırma |

**Düzeltme:** benim üçlü modelimde "sandbox'a veri yazma" kademe 2'deydi. Dört kademeli
modelde bu **kademe 3**'tür — dış sisteme (hedef veritabanına) dokunuyor. Sandbox bizim
değil **müşterinin** ortamıdır.

Ek ilke (K3): *"Onayları işlem kategorisine göre değil **risk sinyaline** göre tetikle,
yoksa lastik damga olur."* Yani kademe 3'te bile her seferinde sormak yerine, güven
sinyali düşükse veya etki yarıçapı büyükse sorulur.

### 5A.3 Otonomi seviyesi kiracı ayarıdır

Seer deseninden alınan karar: tek bir sabit politika yerine **kiracı başına ayarlanabilir**
otonomi.

| Seviye | Ajan ne yapabilir |
|---|---|
| `Observe` | Yalnız kademe 1 — okur, önerir, hiçbir şey tetiklemez |
| `Assist` (varsayılan) | Kademe 1–2 — kuru koşum ve koşu tetikleyebilir |
| `Act` | Kademe 1–3 — sandbox seed'i de yapabilir |
| Kademe 4 | **Hiçbir seviyede otonom değil** — her zaman insan onayı |

Ayar `Domain.Shared` setting olarak tanımlanır, ABP setting sağlayıcı zincirinden okunur.

### 5A.4 Onay kaydının bağlayıcılığı — eksik olan kısım

§4.2'de "insan onaylar" demiştim ama **onayın neye bağlandığını** tanımlamamıştım.
Üretim deseni bunu şöyle çözüyor (K3):

Bir onay kaydı şunları taşımalı: **kiracı, aktör, ajan, işlem, hedef, maddi yük (payload),
politika sürümü, son kullanma tarihi ve idempotency anahtarı.**

Ve kritik kural:

> *"İçerik, fiyat, alıcı veya hedef değişirse onay **artık geçerli değildir**."*

**Bizdeki uygulaması:** onay, yamanın **içerik hash'ine** bağlanır.

```
heal_proposals[].approval = {
  approvedBy, approvedAt,
  boundToPatchHash: "A1B2…",     ← yama degisirse onay DUSER
  policyVersion: "v3",
  expiresAt: "2026-08-19T00:00Z",
  idempotencyKey: "hp-118-apply"
}
```

Ajan onay aldıktan sonra yamayı değiştirirse, hash tutmaz ve uygulama **reddedilir**.
Bu, "onay al, sonra başka bir şey uygula" saldırısını kapatır.

### 5A.5 Onay ekranında gösterilmesi zorunlu dört şey

Aynı desen onay arayüzü için de net (K3): **ne yapılacak · neden (ajanın gerekçesi) ·
ne değişecek · nasıl geri alınır.**

Bizim onay ekranımız bu dördünü göstermek zorunda:

| Alan | Kaynağı |
|---|---|
| Ne yapılacak | `patchOverlay` — JSONPath hedefleri insan-okur özetlenmiş |
| **Neden** | `rationale` + `findingFingerprint` → bulgunun kendisi |
| Ne değişecek | Yama uygulanmış senaryo ile mevcut senaryonun farkı |
| Nasıl geri alınır | `healedFromNo` — önceki sürüme dönüş |

### 5A.6 Asenkron onay — senkron onay kırılıyor

Üretim dersi (K3): *"Senkron onay; ağ geçidi zaman aşımları, token süresi dolması ve
bayat imleçlerle çarpışır. Gerçek altyapıda ayakta kalan desen **idempotency anahtarlı,
dayanıklı, durum yönetimli kesintidir**."*

**Bizde karşılığı zaten var:** MCP Tasks uzantısının `input_required` durumu. Eklenecek
tek şey **idempotency anahtarı** (§5A.4).

### 5A.7 Denetim kaydı — standartlar ne diyor

İki kaynak birleşiyor:

**IETF taslağı `draft-sharif-agent-audit-trail`** — otonom AI sistemleri için standart
loglama formatı (K2). Altı olay tipi: **tool çağrısı, model çağrısı, veri erişimi,
politika kararı, kimlik iddiası, hata.**

Ve gizlilik kuralı bizim kararımızla birebir örtüşüyor:

> *"Ham girdi ve çıktı verisi denetim kayıtlarında **saklanmamalıdır**. Uygulamalar bunun
> yerine `input_hash` ve `output_hash` alanlarını kullanmalıdır."*

**EU AI Act Madde 12** (K2) — yüksek riskli AI sistemleri için otomatik olay kaydı,
**en az altı ay** saklama, sistemin davranışının **yeniden kurulabilmesi**.

Hash zincirli, ekle-only kayıtlar bu gereksinimleri **içerik saklamadan** karşılıyor:
her kayıt `event_id`, RFC3339 zaman damgası, kapsamlı kimlikler, serbest `details` yükü
ve bütünlük metaverisi (`prev_hash`, `curr_hash`) taşıyor; `curr_hash`, olay yükü + önceki
hash üzerinden SHA-256.

**En yüksek bilgi kazancı olan alan:** *"Bir ajan bir dosyayı silerse, teknik log bunun
olduğunu gösterir; **gerekçe izi** ajanın bunu neden doğru bulduğunu açıklar."*

#### Bizim kararımız — orantılı yanıt

Test Module muhtemelen "yüksek riskli AI sistemi" değil (geliştirici aracı). Bu yüzden
hash zincirli WORM depolamayı **v1'de kurmuyoruz**. Ama üç şeyi şimdi yapıyoruz:

| Yapılan | Gerekçe |
|---|---|
| **Sonuçlu eylemler zaten domain modelinde denetleniyor** | `test_runs.trigger_ref` (kim tetikledi), `scenarios.versions[].approvedBy` (kim onayladı), `heal_proposals[].reviewedBy`. Bunlar **kalıcı ve değişmez** kayıtlardır — asıl denetim izi budur, sohbet dökümü değil |
| **Gerekçe izi saklanıyor** | `rationale` alanı; en yüksek bilgi kazancı olan alan |
| **`trace_id` uçtan uca taşınıyor** | Model çağrısından tool çağrısına, oradan koşuya (`correlation_id`) |
| **Ham girdi/çıktı saklanmıyor** | IETF taslağıyla aynı ilke; zaten `ValueRetentionMode = None` |

**Yükseltme yolu belgelenir:** regüle bir müşteri EU AI Act Madde 12 seviyesi kanıt
isterse, `agent_action_log` (ekle-only + hash zinciri + WORM) ayrı bir karar olarak açılır.
Şimdi kurmak, kanıtlanmamış bir gereksinim için 10. tablo eklemek olurdu.

### 5A.8 Telemetri sözlüğü — OpenTelemetry GenAI

Ajan telemetrisi için provider-nötr standart mevcut: **17 operasyon adı, 61 `gen_ai.*`
özniteliği, 8 span şekli, 12 metrik aracı, 3 olay** (K2). Konvansiyonlar artık ayrı
depoda (`semantic-conventions-genai`).

Bizim kullanacaklarımız:

| Öznitelik | Bizdeki alan |
|---|---|
| `gen_ai.conversation.id` | `agent_sessions.id` |
| `gen_ai.usage.input_tokens` | `agent_sessions.input_tokens` |
| `gen_ai.usage.output_tokens` | `agent_sessions.output_tokens` |
| `gen_ai.request.model` | `agent_sessions.model_ref` |
| `gen_ai.operation.name` | Tool adı |

Kendi telemetri sözlüğümüzü icat etmiyoruz.

### 5A.9 Kiracı kapsamı — veritabanı katmanında

Üretim dersi (K3): *"Doğru mimari seçim, kiracı kapsamını **uygulama katmanında değil
veritabanı katmanında** yapmaktır; uygulama katmanı kapsamı, kiracılar arası sızıntıya
**tek hata uzaklıktadır**."*

Bu, `IMultiTenant` + global sorgu filtresi kararımızın (ve §TEST-4'teki analizin) dış
doğrulamasıdır.

### 5A.10 Bellek ayrı bir mimari bileşendir

2026 pratiği (K3): bellek, modelin bağlam penceresinden **ayrı** bir bileşen olarak ele
alınıyor — "daha uzun prompt" değil. LangGraph tarzı checkpoint'leyiciler oturum kalıcılığını
veritabanına yazıyor.

**Bizde:** `agent_sessions.messages` kısa vadeli durum; **kalıcı bilgi** ise
`business_knowledge` tablosudur (K-4 ile öğrenilenin kalıcılaşması). İkisi karıştırılmaz:
sohbet dökümü **bağlam**, iş bilgisi **kayıt**.

---

## 6. Kaynaklar (bu belgeye özel; erişim 2026-08-12)

| Kaynak | Neyi kanıtlıyor | Sınıf |
|---|---|---|
| https://arxiv.org/html/2604.14437v1 | LLM'lerin test üretiminde kestirmeleri: ezber, assertion silme, uydurulmuş API. SAP HANA %2,39–10,25 vs insan %30,41; bağlamla %150 iyileşme | K2 |
| https://arxiv.org/html/2604.25862v1 (RESTestBench) | Kesin/belirsiz gereksinim farkı 26–40 puan; Llama 3.1 8B %2; **bozuk SUT'a karşı iyileştirme oracle'ı bozuyor**; GPT-5 Nano %70 / $0,41 vs Sonnet 4.5 %65 / $10,13 | K2 |
| https://arxiv.org/pdf/2604.11978 | Uzun ufuklu ajan sistemlerinin nerede ve neden kırıldığı; alt-planlama ve felaket unutma | K2 |
| https://arxiv.org/pdf/2603.29231 | pass@1'in ötesinde güvenilirlik çerçevesi; uzun ufuk güvenilirliği | K2 |
| https://arxiv.org/html/2607.22880v1 | Kapsam/mutasyon skorunun gerçek hata bulmayla korelasyonu; proxy metrik geçerliliği | K2 |
| https://arxiv.org/pdf/2410.21136 | LLM oracle'ları gerçek davranışı mı beklenen davranışı mı yakalıyor | K2 |
| Ajan güvenilirlik ölçümleri (2026) | Tek adım %80–90, çok adımlı zincir %18–24; guardrail'ler %19,9 kurtarma | K3 |
| Yerel model tool-calling kıyaslamaları (2026) | Qwen3 serisi en kararlı; Llama-3-Groq-70B BFCL %90,76 | K3 |
| https://datatracker.ietf.org/doc/draft-sharif-agent-audit-trail/ | Otonom AI sistemleri için standart denetim log formatı; altı olay tipi; **ham girdi/çıktı saklanmamalı, `input_hash`/`output_hash` kullanılmalı**; hash zinciri (`prev_hash`/`curr_hash`) | K2 |
| EU AI Act Madde 12 | Yüksek riskli sistemlerde otomatik olay kaydı, **≥6 ay** saklama, davranışın yeniden kurulabilmesi | K2 |
| https://opentelemetry.io/docs/specs/semconv/registry/attributes/gen-ai/ | GenAI semconv: 17 operasyon adı, **61 `gen_ai.*` özniteliği**, 8 span şekli, 12 metrik; `gen_ai.conversation.id`, `gen_ai.usage.input_tokens/output_tokens` | K2 |
| https://thenewstack.io/sentrys-seer-agent-debug/ | Seer: **tek ajan mimarisi, iki iş akışı** (geniş sohbet + dar autofix); kuruluş **otonomi seviyesini kontrol ediyor**; üretim telemetrisi statik analizin göremediğini teşhis ediyor | K3 |
| https://www.agentic-patterns.com/patterns/human-in-loop-approval-framework/ | **Dört kademeli** izin modeli (salt okuma / geri alınabilir / dış sistem / geri alınamaz); geri alınabilirlik ve etki yarıçapına göre sınıflandırma | K3 |
| https://mojoauth.com/blog/human-in-the-loop-authorization-patterns-for-autonomous-agents | Onay kaydı alanları (kiracı, aktör, ajan, işlem, hedef, payload, politika sürümü, son kullanma, idempotency); **payload değişirse onay düşer**; asenkron onay zorunluluğu | K3 |
| https://clarm.com/blog/articles/audit-trail-patterns-for-ai-agents/ | Denetim izi substrat özelliğidir: varsayılan olarak her ajan eyleminde, **kiracı kapsamlı**, ekle-only, dışa aktarılabilir; **kiracı kapsamı veritabanı katmanında** olmalı | K3 |
| https://heybob.ai/blog/ai-agent-audit-trail/ | **Gerekçe izi en yüksek bilgi kazancı olan alan**; `trace_id` görev başında atanır ve her adıma taşınır | K3 |
| https://mem0.ai/blog/state-of-ai-agent-memory-2026 | Bellek, bağlam penceresinden ayrı mimari bileşen; oturum kalıcılığı veritabanında | K3 |
</content>

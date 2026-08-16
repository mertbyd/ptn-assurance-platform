---
id: ADR-0019
type: decision
status: accepted
title: Generic kopru — profil paketi, kanit yolu verisi ve yetenek seviyesi
created: 2026-08-14
updated: 2026-08-14
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0007
  - ADR-0008
  - ADR-0014
  - ADR-0015
  - ADR-0016
  - ADR-0017
  - ADR-0018
rule_refs:
  - RULE-0005
  - RULE-0006
  - RULE-0007
  - RULE-0008
---

# ADR-0019 — Generic köprü: profil paketi, kanıt yolu verisi ve yetenek seviyesi

> Dayanak: [[90-Inbox/RESEARCH-0016-Generic-Ve-Dinamik-Kopru-Yetenek-Sablonu|RESEARCH-0016]].
> ADR-0018'i **genişletir**, hiçbir maddesini iptal etmez.

## Bağlam

ADR-0018 köprünün **ne** yapacağını sabitledi (tek sözlük, ≤7 tool, kanıt zinciri, alıntı
kapısı). Açık kalan soru: köprü **her vaka için elle mi yazılacak?**

*"403 geldi → user_roles → permission_grants → gereken scope"* zincirini C#'ta `if` olarak
yazmak, ikinci müşteride (farklı şema, farklı tablo adları) **baştan yazmak** demektir.
Ürün böyle ölçeklenmez.

## Karar

### A. Kanıt zinciri **veridir**, kod değildir

Tek bir **kanıt yolu motoru** vardır; alan bilgisi taşımaz. Hangi zincirin ne zaman yürüyeceği
`evidence-paths` tanımlarından gelir: tetikleyici, sıralı düğümler, düğüm başına kaynak
(`api.failureIdentity` / `db.projection` / `db.footprint`), ve hüküm ifadesi.

Yeni teşhis sınıfı eklemek = **yeni tanım girdisi**, yeni `if` değil.

Dayanak: k8sgpt'nin üretimde çalışan **deterministik analyzer + yalnız anlatım için model**
ayrımı; model çıktısının deterministik girdi hash'ine bağlanması.

### B. Kavramı şemaya **profil manifesti** bağlar, ajan değil

Köprü kapalı bir **kavram** kümesi tanımlar (`Subject`, `RoleAssignment`, `PermissionGrant`,
`Resource`, `TimeAnchor`, …). Somut tablo/kolon eşlemesi **profil manifestindedir**.

- Manifest **veridir**: `ptn-profile-pack` = manifest + kanıt yolları + `revision` +
  `dbSchemaFingerprint`. **Git'te durur, MCP `Resource` olarak sunulur**, koşuda yalnız
  `profile_fingerprint` kaydedilir. **Yeni tablo açılmaz** — ADR-0016 modeli korunur.
- Aday bağlamalar **mekanik üretilir** (FK grafiği + kardinalite + ad benzerliği → VKG desen
  kataloğu: SE / SR / SRa / SRR / SH), **insan onaylar**. `Proposed → Approved | Rejected`.
- Şema parmak izi değişirse ilgili bağlamalar `Proposed`'a düşer; **sessiz kayma imkânsız**
  (ADR-0018 §E'nin veri tarafındaki eşi).

**Ölçüm gerekçesi:** 100 soruluk eşli benchmark'ta **4 KB'lık** elle yazılmış semantik belge
doğruluğu **+17/+23 puan** artırdı (%45,5-50,5 → %67,7-68,7; p<0,01) ve **model seçiminden
daha belirleyici** oldu. Yani manifest, model yükseltmesinden daha değerlidir.

### C. Cevaplayamamak birinci sınıf sonuçtur

`NOT_BOUND` (kavram bağlı değil), `Unavailable` (kanıt okunamadı), `Inconclusive` (zincir
tamamlanamadı) **hata değil, sonuçtur**; her biri kapalı uçlu bir soruya (ADR-0017 §D) veya
`test_outcome_statuses.Inconclusive`'e bağlanır.

**Gerekçe ölçülmüştür:** semantik katman kapsam dışı soruda **%0** doğruluk verir çünkü
reddeder; text-to-SQL aynı sorularda %70 verir, yani **%30 sessiz yanlış**. Ürünün tezi
budur: *"başarısızlık bir hata mesajı gibi görünmelidir, makul ama yanlış bir cevap gibi
değil."*

**Özel hâl — yanlış teşhis tuzağı:** salt-okunur bağlantı bir tabloyu göremiyorsa doğru cevap
*"kullanıcının rolü yok"* **değil**, *"kanıt toplanamadı"*dır. GCP Policy Troubleshooter aynı
ayrımı yapar. Bu ayrım yapılmazsa köprü **deterministik motordan gelmiş gibi görünen yanlış
teşhis** üretir.

### D. Açıklama, çözümün yan ürünüdür

Köprü *"neden"* için ikinci bir akıl yürütme kurmaz. Zincir yürürken **atılan her adım
kaydedilir** ve rapor o kayıttır. SpiceDB `withTracing`, OpenFGA `Expand` ve GCP'nin
`bindingExplanations[]` ağacı aynı deseni kullanır.

Rapor şekli (alan-bağımsız):

```
PtnAccessTuple      { subject, operation, requiredPermission, context }
PtnExplanationNode  { nodeKindCode, stateCode, relevanceCode, evidence[], children[] }
   stateCode        Observed | NotObserved | Unavailable
   relevanceCode    High | Normal                     ← motor hesaplar, ajan değil
PtnVerdict          Confirmed | Likely | Possible | RuledOut | Inconclusive
```

Bu, ADR-0018 §D'nin (alıntısız hipotez rapora giremez) **mekanik garantisidir**: kanıt
uydurulamaz, çünkü kanıt = atılan adımın kaydı.

### E. Yetenek **yoklanır**, varsayılmaz — dört seviyeli ayak izi

*"Bu operasyon DB'de neyi değiştiriyor"* sorusunun cevabı ortama göre değişir. Köprü hangi
yolun mümkün olduğunu ölçer ve sonucu **seviye** olarak raporlar:

| Seviye | Strateji | Şart | Ayak izinin gücü |
|---|---|---|---|
| `Exact` | PostgreSQL logical decoding | `wal_level = logical`, replication yetkisi, slot | Tablo + kolon + işlem türü + sıra |
| `RowAddressed` | (motor destekliyorsa) değişiklik izleme | motor/ayar | Tablo + değişen satırın anahtarı |
| `Inferred` | Önce/sonra farkı | salt-okuma + **tekil sandbox** | Tablo + satır sayısı deltası (**aday**) |
| `Unavailable` | — | yetenek yok veya sandbox paylaşımlı | **Yok** → soru insana |

**v1 kapsamı PostgreSQL'dir.** Diğer motorlar `Unavailable` döner; genişletme yeni ADR ister.

**Operasyonel zorunluluk:** replication slot tüketilmezse sunucu WAL'i geri dönüştüremez ve
**müşterinin diski dolar**. Slot **geçici (temporary)** açılır ve koşum sonunda **garantili**
düşürülür; düşürülemezse koşum `Broken` işaretlenir.

**Sınır korunur:** ayak izi **oracle değildir** (ADR-0018 §F). `Inferred` ve `RowAddressed`
öneri olarak sunulur; onaysız assertion üretimine giremez. `Exact` bile gözlemdir — B7 tuzağı
geçerlidir.

### F. Ön koşul: Database Checker'a salt-okunur **projeksiyon** yüzeyi gerekir

Kod seviyesinde doğrulandı: bugün DB Checker yalnız **beklenti doğrular**
(`AssertRow/Count/Absent`) ve yapı anlatır (`DescribeTable`, `GetSnapshot`). *"Bu kullanıcının
rolleri neler"* sorusunu soracak bir yüzey **yoktur**; `RowSummary` yalnız başarısızlıkta
dolar.

Bu bir **checker işidir** (PLAN-0001) ve köprünün ön koşuludur: adres + anahtar + kolon
listesi alan, serbest SQL taşımayan, bütçeli, redaksiyonlu, salt-okunur projeksiyon.
ADR-0007'nin salt-okunur değişmezini **bozmaz**.

**Yüzey gelene kadar** köprü ilgili düğümü `Unavailable` işaretler ve zincir `Inconclusive`
döner — yani köprü bu yüzey olmadan da **doğru davranır**, sadece daha az şey söyler.

### G. Yüzey dar kalır: progressive disclosure

ADR-0018 §B (≤7 aktif tool) korunur. Mekanizma netleşir: **tool şeması bağlama girmez, talep
üzerine yüklenir.** Ölçülmüş kazanç %85-99 aralığında ve 112 tool'a ölçeklenmiş üretim örneği
var.

**"Code mode" (modele kod yazdırıp yürütmek) v1'de benimsenmez** — RULE-0005'in kademe 3-4
izin modeline girer; benimsenirse sandbox ve izin kademesi ayrı ADR ister.

### H. Graf motoru kurulmaz

Kanıt yolu 2-4 atlamalık, **anahtarla sınırlı okumalar** dizisidir. SQL/PGQ (PostgreSQL 19) ve
Apache AGE eklentisi biliniyor ama **kullanılmaz**: müşterinin veritabanına eklenti kurma
şartı en pahalı uygunluk bedelidir. Yol uzunluğu **ölçülerek** büyürse yeniden açılır.

## Alternatifler

- **Zinciri C#'ta `if` olarak yazmak:** ikinci müşteride baştan yazılır; ürün ölçeklenmez.
- **Bağlamayı ajana yaptırmak:** RULE-0007 ihlali; schema linking literatürünün darboğazı.
- **Bağlamayı tabloya koymak:** ADR-0016'nın 4+5 modelini deler; Git zaten sürümleme ve
  içerik adresleme veriyor (ADR-0014 §A gerekçesi).
- **Ayak izini yalnız fark yöntemiyle çıkarmak:** eşzamanlı trafik altında ayrıştırılamaz;
  log tabanlı yol SUT'tan hiçbir şey istemeden daha kesin sonuç verir.
- **Ayak izini SUT enstrümantasyonuyla çıkarmak (EvoMaster/OTel):** müşterinin **yazılımını**
  değiştirmeyi şart koşar; log tabanlı yol yalnız **veritabanı ayarını** ister.
- **Graf veritabanı / eklenti:** yol uzunluğu bunu gerektirmiyor; kurulum bedeli yüksek.
- **Kapsam dışı soruda "elimden gelenin en iyisi" cevap vermek:** ölçülmüş %30 sessiz yanlış.

## Sonuçlar ve riskler

Yeni yüzeyler: kavram sözlüğü, profil paketi sağlayıcısı, kanıt yolu motoru, açıklama ağacı
modeli, yetenek çözümleyici, ≤7 tool yüzeyi. **Yeni proje veya katman açılmaz** (ADR-0015 §F);
Bridge public kontratlari `Application.Contracts`, checker cagrilari ve Mapperly eslemeleri
`Application`, davranissal kurallar `Domain` Manager'lari altinda kalir. EntityFrameworkCore
yalniz kalicilik ve migration sahibidir; Bridge adapter/document/mapper ailesi barindirmaz.

**Veri modeli değişmez** (ADR-0016): profil paketi dosyadır, koşuda `profile_fingerprint`
olarak kaydedilir.

| Risk | Önlem |
|---|---|
| Profil manifesti bakımsız kalır, kapsam düşer | Kapsam raporu her teşhiste rapor başında; `bound/required` oranı |
| Şema kayar, manifest sessizce yanlışlanır | `dbSchemaFingerprint` mührü; kayma → `Proposed`'a düşürme |
| Aday bağlama yanlış tabloyu gösterir | FK grafiği + desen eşleşmesi ile daraltılır; **insan onaylar** |
| `Unavailable` çok sık çıkar, ürün "bilmiyorum makinesi" olur | Ön koşul F yüzeyi PLAN-0001'e alınır; kapsam metriği izlenir |
| Replication slot temizlenmez, müşteri diski dolar | Geçici slot + garantili düşürme + düşürülemezse `Broken` |
| Kanıt yolu tanımları ajana yazdırılır | Tanım **kademe 4** artefaktıdır; ajan öneri üretir, insan onaylar (RULE-0005) |
| Yol motoru yavaşlar | Probe bütçesi (`ProbeBudgetManager` deseni) + atlama sınırı; aşımda `Inconclusive` |

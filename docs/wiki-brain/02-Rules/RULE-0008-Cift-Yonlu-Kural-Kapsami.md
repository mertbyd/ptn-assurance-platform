---
id: RULE-0008
type: rule
status: active
title: Karar tablosunun her satiri test edilir; Allow satiri kapsanmayan surum yayinlanamaz
updated: 2026-08-13
severity: mandatory
scope: test-module
sources:
  - https://www.toolsqa.com/software-testing/istqb/decision-table-testing/
  - https://www.virtuosoqa.com/post/decision-table-testing
decision_refs:
  - ADR-0017
  - ADR-0018
rule_refs:
  - RULE-0006
---

# RULE-0008 — Çift yönlü kural kapsamı

## Kural

Bir senaryo sürümü yayınlanabilmek için, kaynaklandığı **DMN karar tablosunun her satırı**
en az bir testle kapsanmalıdır — **`Deny` satırları kadar `Allow` satırları da.**

Kapsam ölçüsü: *test edilen karar kuralı sayısı / toplam karar kuralı sayısı* = **%100**.

`Allow` satırı kapsanmayan sürüm **yayınlanamaz**.

## Neden

Yalnız `Deny` yönünü test etmek **aşırı-engellemeyi (over-blocking / yanlış ret) görünmez kılar.**

Somut vaka:

```
Kural: "Öğrenci 6 saatlik dilimde tek bilet alabilir."

| userType | son biletten geçen süre | sonuç |
| Student  | < 6 saat                | Deny  |   ← T1 bunu test eder
| Student  | >= 6 saat               | Allow |   ← BU SATIR TEST EDİLMEZSE
| Regular  | *                       | Allow |

Gerçek davranış: 6 saat geçtikten sonra da alamıyor.
T1 yeşil → "kural çalışıyor" sanılır. Oysa kural FAZLA engelliyor.
```

Aynı tuzak 12:30 örneğinde de var: `now < departure → Allow` satırı test edilmezse, sistem
**hiç bilet satmıyor** olsa bile testler yeşil kalır.

Karar tablosu testinin tanımı bunu zaten kapsıyor: *"geçerli ve geçersiz kombinasyonları
içererek hem pozitif hem negatif testi destekler; pozitif test hem olumlu sonuçları (onayla,
işle, yönlendir) hem olumsuz sonuçları (reddet, hata göster, yükselt) kapsar"* ve bu yüzden
*"**aşırı-engelleme (yanlış ret)** gibi sorunları tespit etmek için idealdir."*

## RULE-0006 ile ilişkisi

İki kural birbirini tamamlar ve **ikisi de yayın kapısıdır**:

| Kural | Sorduğu soru |
|---|---|
| **RULE-0006** | Assertion **var mı** ve **sözleşmeden türetilebilir mi**? |
| **RULE-0008** | Kuralın **her iki yönü de** sınandı mı? |

RULE-0006 geçen ama RULE-0008 geçmeyen bir sürüm, *"doğru şeyi ölçen ama eksik ölçen"* testtir.

## Doğrulama

- Yayın yolu testi: `Allow` satırı kapsanmayan sürüm `Published` olamaz.
- Kural kapsam raporu her sürümde üretilir: kapsanan satır / toplam satır.
- Sınır satırlarında MC/DC türetimi uygulanır (eşik − ε, eşik, eşik + ε).
- Zaman kuralı içeren tablolarda SUT'ta **test saati** yoksa sürüm `Inconclusive` işaretlenir;
  yeşil sayılmaz.

## İstisna süreci

Bir `Allow` satırının test edilememesi ancak teknik bir engelle (ör. SUT'ta test saati yokluğu)
mümkündür; bu durumda sürüm yayınlanabilir fakat ilgili satır **kapsanmamış** olarak raporlanır
ve koşum sonucu o kural için `Inconclusive`'dir — asla `Passed` değil.

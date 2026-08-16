---
id: RULE-0006
type: rule
status: active
title: Turetilemeyen assertion yayinlanamaz; assertion sayisi sifir olan adim reddedilir
updated: 2026-08-13
severity: mandatory
scope: test-module
sources:
  - https://arxiv.org/html/2602.07900
decision_refs:
  - ADR-0014
  - ADR-0016
rule_refs:
  - RULE-0005
---

# RULE-0006 — Türetilemeyen assertion yayınlanamaz

## Kural

Bir senaryo sürümü `Published` durumuna **ancak** şu üç koşul sağlanırsa geçebilir:

1. **Şema geçerli.** Doküman Arazzo **1.0.1** şemasını ve referans bütünlüğünü geçer
   (hedef sürüm 1.1'den 1.0.1'e çekildi — AUDIT-0003 BULGU-07; kodda
   `ArazzoCompilerManager.TargetVersion = "1.0.1"` sabittir)
   (`redocly lint`).
2. **Her assertion türetilebilir.** `ValidateScenarioAssertionsAsync` her assertion için
   `{jsonPointer, outcomeCode}` döndürür; türetilemeyen tek assertion varsa yayın **reddedilir**.
3. **`assertion_count > 0`.** Assertion taşımayan adım yayınlanamaz.

Assertion sayısını azaltan veya matcher'ı gevşeten değişiklik ayrıca işaretlenir ve onay
ekranında uyarı olarak gösterilir.

## Neden

Ölçüm ([[90-Inbox/RESEARCH-0013-Runner-Oracle-Ayrimi-Ve-Ajan-Yazarlik-Kaniti|RESEARCH-0013]] §3, B8):

- Ajan geri bildiriminin **%70-77'si `print` ifadesi**, assertion değil.
- Assertion'ların **%33-41'i** yalnız "alan var mı" kontrolü, **%35-43'ü** tam değer eşitliği.
- İlişkisel/aralık kontrolü **yalnız %3-8**.

Yani ajan serbest bırakılırsa **çalışan ama hiçbir şey doğrulamayan test** üretir. Yeşil bir
koşu, doğrulanmış bir davranış anlamına gelmez.

Türetilebilirlik kapısı bunun yapısal karşılığıdır: ajan serbest kod yazamaz, yalnız tipli
sözleşmeye assertion emit eder ve o assertion makine ile sözleşmeye karşı doğrulanır.

## Yan fayda — kapsam raporu

`test_result_findings.rule_ref` alanı hangi iş kuralının doğrulandığını taşır. Aynı alan ters
yönde okunduğunda kapsam boşluğu verir:

> *"BR-015 için `rule_ref` taşıyan hiçbir bulgu üretilmemiş"* → o iş kuralı test edilmiyor.

## Doğrulama

- Yayın yolu testi: türetilemeyen assertion içeren sürüm `Published` olamaz.
- Yayın yolu testi: `assertion_count = 0` olan sürüm reddedilir.
- `derivability_code` her sürümde doldurulur; boş bırakılamaz.
- Anahtar kolonun PK/unique olup olmadığı `DescribeTableAsync` ile yayında kontrol edilir —
  değilse assertion zaten `KeyNotUnique` döner (ADR-0007) ve bunu **yayında** yakalamak gerekir.

## İstisna süreci

Kapı yalnız ölçüm gösteren yeni bir ADR ile gevşetilebilir. Uyarıya indirmek, B8'in ölçtüğü
zayıf assertion'ın doğrudan üretime geçmesi demektir.

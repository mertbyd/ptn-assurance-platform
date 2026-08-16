---
id: ADR-0007
type: decision
status: accepted
title: Checker oracle surface — assertion and diagnosis
created: 2026-08-12
updated: 2026-08-12
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs:
  - ADR-0002
  - ADR-0005
rule_refs:
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# ADR-0007 — Checker'ın oracle yüzeyi: assertion ve teşhis

## Bağlam

[[04-Architecture/System-Context|ARCH-0001]] checker'ları "bilgi motoru" olarak tanımlıyordu:
iki veritabanını kıyaslar, bulgu üretir. Test Module'ün ihtiyacı ise farklı bir sorudur —
*"çağırdığım şeyin izi veritabanında var mı, yoksa neden yok?"*

Bu soruyu tam karşılaştırmayla cevaplamak üç bakımdan yanlıştı: cevap 50–500 KB
(MCP bağlamına sığmaz), süre saniyeler mertebesinde, ve soru "iki ortam aynı mı" değil
"tek ortamda beklenen durum oluştu mu".

## Karar

Database Checker iki yeni **salt-okunur** yüzey açar ve bunlar paket sınırının içindedir:

1. **Assertion yüzeyi** — anahtarla seçilen satırın varlığı, yokluğu, kardinalitesi ve
   kolon beklentileri; sunucu tarafında sınırlı bekleme; tip-farkında matcher'lar;
   kararlı `AssertionOutcomeCodes`.
2. **Teşhis yüzeyi** — bir başarısızlık sinyalinden (assertion sonucu veya DB istisna alanları)
   yola çıkıp canlı katalog + sınırlı probe'larla sıralanmış hipotez üretir; çıktı RFC 9457.

Her ikisi de:

- Hedef veritabanına **yazmaz**. Test verisi seed/cleanup consumer'ın `ITestDataSandbox` işidir.
- Serbest SQL veya serbest `WHERE` kabul etmez; yalnız katalogda doğrulanmış nesne adları ve parametre.
- Kalıcı değildir; hesaplanır ve döner.
- Değer taşıyan her alanı `ValueRetentionMode` politikasından geçirir (varsayılan `None`).

## Alternatifler

- **Tam karşılaştırmayı kullandırmak:** çıktı boyutu ve süre nedeniyle senaryo adımı olarak kullanılamaz.
- **Runner'ın kendi SQL'ini yazması:** paket sınırını delerdi, enjeksiyon yüzeyi açardı ve
  katalog doğrulaması olmadan sessiz yanlış sonuç üretirdi.
- **Teşhisi modele yaptırmak:** LLM oracle'ları kırılgandır; güven ve kanıt deterministik hesaplanmalıdır.

## Sonuçlar ve riskler

Checker artık yalnız "bilgi motoru" değil, aynı zamanda Test Module'ün **veritabanı oracle'ı**dır.
ARCH-0001 bu rolü yansıtacak şekilde güncellenir.

Risk: assertion ve teşhis canlı hedeflere bağlanır. Karşı önlem
[[03-Decisions/ADR-0004-Single-Vault-Adapter|ADR-0004]] secret sınırı, salt-okunur kimlik
sözleşmesi, `READ ONLY` transaction, statement/lock timeout ve probe bütçesidir.

Kritik invariant: anahtar kolonları PK/unique değilse assertion **çalışmaz** ve
`KeyNotUnique` döner. "O satır" garantisi olmadan sessiz yanlış cevap verilmez.

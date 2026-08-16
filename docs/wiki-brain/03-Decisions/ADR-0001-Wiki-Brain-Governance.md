---
id: ADR-0001
type: decision
status: accepted
title: Versioned Wiki Brain governance
created: 2026-08-11
updated: 2026-08-11
owners:
  - mertbyd
supersedes: []
superseded_by: null
decision_refs: []
rule_refs: []
---

# ADR-0001 — Sürümlü Wiki Brain yönetimi

## Bağlam

Checker, Vault, paketleme, global araştırma ve sonraki entegrasyon bilgisi farklı klasör ve notlara dağılmıştı. Tek büyük Markdown dosyası kaynakları korudu fakat güncel gerçek, karar, kural ve tarihçeyi ayırmadığı için agentlar yanlış bölümü aktif kabul edebiliyordu.

## Karar

`docs/wiki-brain` kanonik hafızadır. `01-Current`, `02-Rules`, `03-Decisions`, `04-Architecture` ve `05-Operations` ayrı yetki katmanlarıdır. Obsidian yalnız editördür. Eski tek dosyalı içerik [[../05-Operations/Research-Archive|ARCHIVE-0001]] olarak eksiksiz korunur fakat kanonik güncel bilgi sayılmaz.

**Yerine geçirilen kararın silinmesi (2026-08-13 eki).** Bir karar yerine geçirildiğinde
varsayılan davranış `superseded_by` ile işaretlemektir. Ancak yerine geçirilen belge ajanları
yanıltacak kadar ayrıntılı bir tasarım taşıyorsa **silinebilir**; üç koşul birlikte sağlanmalıdır:

1. Belgedeki **hâlâ geçerli her kural** yeni ADR/Rule sayfalarına taşınmış olmalı,
2. **Tüm referanslar** (wiki, plan, araştırma, `AGENTS.md` ve kaynak kodu dahil) yeni kararlara
   yönlendirilmiş olmalı; kalan tarihsel anmalar açıkça *"silinen ADR-xxxx"* biçiminde yazılır,
3. **Silme kaydı** `00-Home` ve `05-Operations/Roadmap` sayfalarında tutulur.

Sessiz silme ve sessiz yeniden yazma hâlâ yasaktır. İlk uygulama: `ADR-0011`
(2026-08-13; içeriği ADR-0014, ADR-0015, ADR-0016 ve RULE-0005/0006'ya taşındı).

## Alternatifler

- Tek büyük dosyayı sürdürmek: kaynak kaybı az, yanlış/stale bilgi riski yüksek.
- Yerine geçirilen ADR'yi her zaman saklamak: ayrıntılı eski tasarım, uyarı bloğuna rağmen
  ajan tarafından aktif sanılabiliyor — ADR-0011'de bizzat gözlendi.
- Her modülde ayrı merkezi wiki: karar ve yol haritası tekrarına yol açar.
- Yalnız dış wiki kullanmak: source ve package değişikliğiyle atomik güncellenemez.

## Sonuçlar ve riskler

Agent önce görevine uygun minimum sayfaları okur. Karar değişikliği ADR supersession gerektirir. Archive içindeki eski ifadeler güncel kaynak olarak kullanılamaz.

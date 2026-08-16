---
id: GUIDE-0001
type: guide
status: active
title: Agent workflow
updated: 2026-08-14
decision_refs:
  - ADR-0001
rule_refs:
  - RULE-0002
---

# Agent çalışma akışı

Wiki’nin tamamı her görevde okunmaz. Göreve göre aşağıdaki minimum bağlam açılır.

| Görev | Zorunlu sayfalar |
|---|---|
| Paket kimliği, sürüm, publish | **Önce `GUIDE-0003`**, sonra `CURRENT-0002`, `ARCH-0002`, `LEDGER-0001`, `RULE-0001`, `ADR-0006` |
| Host veya module graph | `CURRENT-0002`, `ARCH-0001`, `ARCH-0002`, `RULE-0001`, `RULE-0004` |
| EF, şema, migration | `CURRENT-0001`, `ARCH-0003`, `RULE-0002`, `ADR-0005` |
| Vault veya credential | `CURRENT-0003`, `RULE-0003`, `ADR-0004` |
| Test Module entegrasyonu | `CURRENT-0004`, `ARCH-0001`, `ARCH-0003`, `GUIDE-0002` |
| **Test Module — ürüne yeni giriş** | **`ARCH-0004` (altı an) — buradan başla** |
| **Test Module veri modeli / entity / migration** | `ADR-0016`, `ARCH-0003`, `Test-Platform-Schema.dbml`, `RULE-0002` |
| **Koşum motoru / Arazzo runner** | `ADR-0015`, `RESEARCH-0013` §1–2 |
| **Test Module ↔ checker entegrasyonu** | `ADR-0015`, `ADR-0007`, `ADR-0008`, `RULE-0001` |
| **MCP, ajan, senaryo yazarlığı** | `ADR-0014`, `ADR-0017`, `RULE-0005`, `RULE-0006`, `RULE-0007`, `RULE-0008` |
| **Checker köprüsü / tool tasarımı / kanıt zinciri** | **`ADR-0018`**, `RESEARCH-0015` — sözlük çakışmaları ve tool bütçesi orada |
| **Projeye yeni katılan / ekip devri** | `GUIDE-0004` (tek başına yeterli), sonra `GUIDE-0005` |
| Global araştırma veya karar doğrulama | `SOURCE-0001`, `GUIDE-0005`, ilgili Current/ADR, gerekirse `ARCHIVE-0001` |
| Yeni mimari karar | ilgili Current + Rule + eski ADR + Decision template |

## Başlangıç

1. `00-Home.md` üzerinden görev rotasını belirle.
2. Çalışan kodu ve package metadata’yı doğrula.
3. Current ile kod çelişiyorsa sessizce varsayım yapma; bulguyu yaz.
4. Archive’i yalnız tarihsel kanıt veya unutulmuş kaynak ararken aç.

### Yazma kapısı

`.claude/rules/verify-patterns.json` bu depodaki mimari kararları makine-okunur tutar
(`filePattern` + `linePattern` + `message`). 2026-08-14'ten beri iki yerde uygulanır: bir
yazımdan **önce** düzenleme kapısında, bir de sonrasında `backend-verify` taramasında. Kural
ihlali eden bir yazım reddedilir; kuralın kendisi yanlışsa etrafından dolaşılmaz, kural
değiştirilir. Yeni bir katman kararı verildiğinde bu dosyaya da işlenir.

## Bitiriş

1. Değişen gerçek için Current sayfasını güncelle.
2. Yeni/geri alınan karar varsa ADR oluştur veya supersede et.
3. Paket yayımlandıysa Release Ledger’a tam PackageId ve sürümü ekle.
4. Publish scriptini mutlak yoldan çalıştır; dry run kanıtını ve registry preflight sonucunu kaydet.
5. Resmî kaynak kullanıldıysa Source Registry’ye erişim tarihiyle ekle.
6. Roadmap durumunu güncelle.
7. `id`, `status`, `decision_refs`, `rule_refs` ve bağlantıları kontrol et.

## Yetki sırası

1. Çalışan kod, migration, `.nupkg` içeriği ve resmî registry
2. `01-Current` ve `02-Rules`
3. Accepted `03-Decisions`
4. `04-Architecture` ve `05-Operations`
5. `ARCHIVE-0001`
6. `90-Inbox`

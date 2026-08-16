# API Contract Checker görev yönlendirici

Önce repository rootundaki kanonik Wiki Brain yönlendiricisini
[`docs/wiki-brain/00-Home.md`](../../../../docs/wiki-brain/00-Home.md) ve root
`AGENTS.md` sözleşmesini oku. `01-Current` / `02-Rules` / accepted `03-Decisions`
yetki sırasını koru; `90-Inbox` yalnız scope girdisidir. Yeni veya paralel wiki oluşturma.

## Skill seçimi

| Görev | Aç |
|---|---|
| Tam vertical slice, CRUD, endpoint veya davranış değişikliği | [`acc-vertical-slice/SKILL.md`](../skills/acc-vertical-slice/SKILL.md) |
| Yeni lookup | [`acc-lookup-recipe/SKILL.md`](../skills/acc-lookup-recipe/SKILL.md) |
| OpenAPI fetch, parse, snapshot, hash veya credential reference | [`acc-spec-ingestion/SKILL.md`](../skills/acc-spec-ingestion/SKILL.md) |
| Normalize, identity match, diff, severity veya yeni spec formatı | [`acc-comparison-engine/SKILL.md`](../skills/acc-comparison-engine/SKILL.md) |
| Kalıcı gerçek, karar, roadmap veya kaynak değişikliği | [`platform-wiki-governance/SKILL.md`](../skills/platform-wiki-governance/SKILL.md) |
| C# / ABP genel prosedürü | `C:\Users\mertb\.codex\skills\abp-coding-standards\SKILL.md` |
| Kapanış doğrulaması | `C:\Users\mertb\.claude\skills\backend-verify\SKILL.md` |

## Değişmezler

1. `Controller -> AppService -> Manager -> Repository`.
2. Mapperly bütün mapping'in sahibidir.
3. Atomik use-case; uzun dış I/O açık UOW tutmaz.
4. Auth, notification, operator veya Vault SDK checker'a geri eklenmez.
5. Controller/AppService/repository/EF/migration ve ince host korunur.
6. EF model değişikliği migration üretme ve tam okuma zorunluluğu taşır.
7. `IPassivable`, tenant ve host-user visibility invariant'ları korunur.
8. Kararlı stringler Domain.Shared owner'ındadır; ikinci kullanımda doğru base/hook'a çıkarılır.

Ayrıntılı platform gerçeği ve kurallar için
[`docs/wiki-brain/00-Home.md`](../../../../docs/wiki-brain/00-Home.md) üzerinden ilgili
Current, Rule ve accepted ADR sayfalarına git.

## Kapanış

İlgili project/solution build ve testlerini, ardından `backend-verify` scanner'ını çalıştır.
Package değiştiyse sürüm artır, `.nupkg` içeriğini ve clean-cache consumer restore'unu
doğrula. Aynı yayımlanmış sürümü farklı içerikle yeniden paketleme.

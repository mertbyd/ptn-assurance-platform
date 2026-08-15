# Ptn.TestModule agent contract

Bu dosya kok `AGENTS.md`'yi ozelestirir; catisma halinde daha ozgul olan bu dosya kazanir.
C# isinde once global `abp-backend-dev` skill'i, tamamlamadan once `backend-verify` gate'i calisir.

## Bu modul nedir

Test Module, iki checker'in uzerine kurulan is senaryosu testi platformudur ve bu klasor onun
**composition host'u ile modul katmanlarini** tutar. Kanonik kararlar `docs/wiki-brain` altindadir:

- **urunun uctan uca akisi (alti an) → `ARCH-0004` — once bu okunur**
- senaryo yazarligi ve turetilebilirlik kapisi → `ADR-0014`, `RULE-0006`
- kosum siniri, dis Arazzo runner → `ADR-0015`
- kayit ve teshis veri modeli (**4 ana tablo + 5 lookup**) → `ADR-0016`,
  sema kaynagi `04-Architecture/Test-Platform-Schema.dbml`
- ajanin hakem olmamasi → `RULE-0005`
- auth tuketim modeli (bu hostun rolu) → `ADR-0013`
- sema/migration sahipligi → `RULE-0002`, `ARCH-0003`
- paket/host siniri → `RULE-0001`
- dayanak arastirma → `RESEARCH-0013`

Eski `ADR-0011` **silinmistir** (2026-08-13): veri modeli ADR-0016'ya, kosum/entegrasyon
ADR-0015'e, ajan sinirlari ADR-0014 + RULE-0005/0006'ya tasindi. `ADR-0011` referansi
goren her yer **yanlistir**.

## Degismezler

- Auth uclari bu hostta **acilmaz**. `Authenticator.HttpApi` modulu compose edilmez; kimlik
  ayri deploy edilen Authenticator host'unundur. Bu host yalniz bearer dogrular (ADR-0013).
- `Nexum.Abp.Foundation.*` dogrudan referanslanmaz; Authenticator uzerinden transitif gelir.
- Bu modul yalniz `test_lookup` (5 lookup), `test_catalog` (**tek tablo:** `test_scenarios`) ve
  `test_run` (`test_runs`, `test_run_results`, `test_result_findings`) tablolarinin migration
  sahibidir. Auth, Notification, Emailing ve checker tablolari icin migration uretilmez.
- Ortam baglamasi **tablo degildir**; ABP tenant-scoped `Setting` olarak tutulur ve kosum
  aninda cozulup `test_runs` satirina snapshot lanir (ADR-0016 §G).
- Checker'lara **dogrudan cagri** ile soru sorulur, **olay** ile olgu dinlenir. Checker tablosuna
  FK verilmez, ortak transaction acilmaz, checker tablosu okunmaz (ADR-0015 §F).
- Checker cagrilari dogrudan yapilmaz: Test Module kendi portlarini cagirir
  (`IDatabaseOraclePort`, `IApiOraclePort`, `IFailureDiagnosisPort`, `ICheckerFindingsPort`,
  `ISchemaKnowledgePort`, `IWorkflowRunnerPort`). Adapter'lar
  `Ptn.TestModule.EntityFrameworkCore/Adapters/` altinda yasar; yeni proje veya katman acilmaz.
- **Kendi HTTP kosum motorumuz YAZILMAZ.** Arazzo is akisini dis runner (Redocly Respect, MIT)
  icra eder; cikti HAR 1.2 + JSON'dur (ADR-0015).
- **Veritabani dogrulamasi bir Arazzo adimidir** — `x-checknexus-db` yayin aninda DB Checker'in
  `POST /assertions/row` ucuna giden gercek bir adima derlenir. Runner'a plugin yazilmaz.
- Response uygunlugu HAR'daki **her** adim icin kosar; DB assertion **kosum sirasinda** kosar.
- **Kosum ve yargi anlarinda model cagrisi yoktur.** Hakem her zaman checker'dir (RULE-0005).
- **Turetilemeyen assertion yayinlanamaz**; `assertion_count = 0` olan adim reddedilir (RULE-0006).
- `Controller -> AppService -> Manager -> Repository` zinciri korunur; entity'ler veri kabugudur.
- Sabit anlamlilar (sema, route, hata kodu, ayar, lookup kodu) `Ptn.TestModule.Domain.Shared`
  altinda sahiplenilir.
- Surumler `common.props` icindeki degiskenlerden yonetilir; csproj'a sabit surum yazilmaz.

## Dogrulama

```bash
dotnet build Ptn.TestModule.slnx
dotnet test Ptn.TestModule.slnx
```

EF model degisiminde migration uretilir ve `Up/Down` govdesi okunur. Host smoke'u PostgreSQL
gerektirir; `Redis:IsEnabled` gelistirmede `false`'tur.

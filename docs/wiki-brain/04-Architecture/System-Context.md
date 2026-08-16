---
id: ARCH-0001
type: current
status: active
title: System context
updated: 2026-08-15
decision_refs:
  - ADR-0002
  - ADR-0004
  - ADR-0005
  - ADR-0012
  - ADR-0013
rule_refs:
  - RULE-0001
  - RULE-0002
  - RULE-0003
  - RULE-0004
---

# Sistem bağlamı

```text
Kullanıcı
  -> Tek UI
      -> Authenticator hostu           AYRI DEPLOY — tek issuer ve identity owner
                                       login/refresh/logout uçları BURADADIR (ADR-0013)
      -> Test Module composition host  resource server — bearer token DOGRULAR, uretmez
          -> Authenticator.*           yalnız TIP olarak; HttpApi compose EDILMEZ (ADR-0013)
              -> Foundation            transitif ortak ABP tabanı; ayrı runtime owner değil
          -> Notifications             bildirim capability
          -> CheckNexus.ApiContracts   OpenAPI snapshot/diff capability
          -> CheckNexus.DatabaseComparison
                                      DB şema/veri/migration kıyaslama capability
          -> CheckNexus.Vault          iki checker için tek secret adapteri
          -> Test Orchestrator         checker sonuçlarını test senaryosuna bağlar
          -> MCP adapter               daha sonra Application.Contracts sınırında

Composition host
  -> tek mantıksal uygulama DB’si
  -> sahibi belli şemalar ve migration assembly’leri
  -> tek Vault deployment/cluster
```

## Sınırlar

- UI checker hostlarına ayrı ayrı bağlanmaz.
- **Test Module hostu resource server'dır; kimlik uçları ayrı deploy edilen Authenticator
  hostundadır.** Host `Authenticator.*` katmanlarını **tip olarak** alır, `HttpApi` modülünü
  **compose etmez** ve doğrulamayı JWT bearer ile yapar
  ([[03-Decisions/ADR-0013-Test-Module-Resource-Server-Auth-Consumption|ADR-0013]];
  ADR-0012 md 4'ü revize eder).
- Checker’lar login/issuer sağlamaz.
- Consumer Foundation paketlerini doğrudan seçmez; katman eşlemesini Authenticator taşır.
- Vault adapteri test sonucu üretmez; yalnız secret çözer/yazar.
- Test Orchestrator checker iç motorlarını kopyalamaz; paket servislerini kullanır.
- MCP repository, EF DbContext veya Vault’a doğrudan erişmez; izinli application contractlarını çağırır.
- MCP sunucusu composition host içinde kurulur ([[03-Decisions/ADR-0008-Mcp-Surface-Placement|ADR-0008]]);
  checker paketleri MCP tipi veya bağımlılığı taşımaz. Checker’ın MCP’ye borcu kararlı kod kümeleri,
  sınırlı çıktı boyutu ve sayfalamadır.
- Database Checker yalnız karşılaştırma yapmaz; Test Module için **veritabanı oracle’ıdır**
  ([[03-Decisions/ADR-0007-Checker-Oracle-Surface|ADR-0007]]): hedefli assertion ve dinamik teşhis.
  Her ikisi de salt-okunurdur; hedefe yazma consumer’ın test verisi sandbox’ının işidir.

## Bu workspace’in yeri

`ptn-assurance-platform` çalışma alanı diyagramdaki iki checker ile Vault adapterinin **çalışma
kopyalarını diskte taşır**; hiçbiri onun Git deposunda izlenmez. Kök depo 2026-08-16'da
daraltıldı ve yalnız `ptn-test-module/` kaynağını izler
([[03-Decisions/ADR-0024-Depo-Siniri-Ve-Wiki-Yayin-Yeri|ADR-0024]]): checker'lar ayrı Git
depoları, Vault ise `CheckNexus.Vault` NuGet paketi olarak tüketilir.

**Composition host artık bu klasördedir.** Bu sayfanın önceki *"composition host ayrı consumer
işidir ve bu klasörde mevcut değildir"* cümlesi **2026-08-13'ten beri yanlıştır**: Test Module
composition hostu o tarihte ABP CLI 10.6.0 `module` şablonundan kuruldu ve
`ptn-test-module/host/` altında `Ptn.TestModule.HttpApi.Host` olarak yaşıyor
([[01-Current/Platform-Truth|CURRENT-0001]]). Bu düzeltme 2026-08-15'te yapılmıştır.

Pratik sonucu: ADR-0008'in *"MCP yüzeyi composition host'ta açılır"* kararı artık **bu
workspace'te uygulanabilir**; daha önce engel olarak kaydedilen *"bu workspace'te composition
host yok"* gerekçesi geçerliliğini yitirmiştir (bkz.
[[90-Inbox/BACKLOG-0001-Checker-Ek-Gelistirme-Talepleri|BACKLOG-0001]] ACX-06 engel notu).

---
id: CURRENT-0005
type: current
status: active
title: UI backend controller catalog
updated: 2026-08-16
decision_refs:
  - ADR-0002
  - ADR-0012
  - ADR-0013
rule_refs:
  - RULE-0001
  - RULE-0002
---

# UI için Test Module runtime controller kataloğu

> [!IMPORTANT] Kapsam
> Bu sayfa kaynak depolardaki olası controller listesini değil, **Test Module composition hostu**
> kurulduğunda DI içindeki `IApiDescriptionGroupCollectionProvider` tarafından gerçekten görülen
> controller/action yüzeyini kaydeder. Kanıt 2026-08-16 tarihli Release binary ve aynı host modül
> grafiğidir. Authenticator ayrı deploy edilir; Auth HTTP controller'ları bu hostta yoktur.

> [!WARNING] Bu envanter KBP-111 öncesine aittir — yeniden üretilmelidir
> Aşağıdaki sayım, KBP-111'in yazarlık oturumu ve iş değişmezi uçları eklenmeden **önce**
> alınmış bir host anlık görüntüsüdür. Kaynaktaki güncel kapı `OutwardSurfaceTests`
> `ExpectedControllerActionCount = **64**`'tür (KBP-112, 2026-08-16); bu sayfa Test Module için
> **54** diyor. Eksik olan en az dört controller vardır: `api/test-module/authoring/sessions`
> (dört action), `api/test-module/invariants/check`, `api/test-module/environments` yazma
> action'ları (`POST` · `PUT {key}` · `DELETE {key}`) ve `api/test-module/authoring`
> kaynak uçları (`POST business-rules` · `POST profile-packs` · `GET profile-packs`).
>
> Sayıları elle düzeltmeyin. Katalog ancak composition host ayağa kaldırılıp
> `IApiDescriptionGroupCollectionProvider` yeniden okunarak üretilir; bugüne kadarki tüm
> satırlar o yolla toplanmıştır. Güncel yüzey özeti için
> [[01-Current/Platform-Truth|CURRENT-0001]] "Test Module ortam ve kaynak yüzeyi" satırına bakın.
>
> **Not:** Bu sayım ABP'nin kendi `/api/setting-management/*` uçlarını içermez; o uçlar
> KBP-112 ile compose edildi ve `OutwardSurfaceTests` sayımının dışındadır.

## UI entegrasyon özeti

| Runtime sahibi | Controller | Action | UI base origin |
|---|---:|---:|---|
| Test Module | 13 | 54 | `<TEST_MODULE_ORIGIN>` |
| API Contract Checker | 10 | 53 | `<TEST_MODULE_ORIGIN>` |
| Database Checker | 17 | 72 | `<TEST_MODULE_ORIGIN>` |
| Emailing | 3 | 10 | `<TEST_MODULE_ORIGIN>` |
| Notifications | 1 | 4 | `<TEST_MODULE_ORIGIN>` |
| ABP framework | 3 | 3 | `<TEST_MODULE_ORIGIN>` |
| **Toplam** | **47** | **196** | 192 benzersiz method+route |

UI, ortam portunu veya localhost değerini sabitlemez. Test Module çağrıları gateway/service
discovery ile verilen `<TEST_MODULE_ORIGIN>` üzerinden gider. Yetkili action'larda Authenticator'ın
ürettiği bearer token gönderilir. `Result<T>` ve `PagedResultDto<T>` response zarfları ortak UI
istemcisinde tek yerde açılır.

### Erişim sütununun anlamı

- `Anonymous`: action açıkça anonim işaretlenmiştir.
- Bir veya daha çok permission/policy adı: bearer token bu politikaları karşılamalıdır. Virgülle
  gösterilen değerler aynı action metadata'sında birlikte bulunur.
- `Unspecified`: controller/action üzerinde auth metadata'sı yoktur. Test Module hostu fallback
  authorization policy tanımlamadığı için bu, mevcut transportta kimlik doğrulaması zorunlu
  olmadığı anlamına gelir; UI bunu güvenli kabul etmez.

## Authenticator neden listede değil?

Test Module bir **resource server**dır. `Authenticator.EntityFrameworkCore` tiplerini kalıcılık
kompozisyonunda transitif kullanır; `Authenticator.HttpApi` modülünü compose etmez. Bu nedenle
bu runtime kataloğunda Authenticator controller/action sayısı **0**'dır. Login, register, refresh,
invitation, context, tenant ve organization-unit ekranları ayrı `<AUTH_ORIGIN>` ve ayrı Auth
Swagger/OIDC discovery sözleşmesini kullanır. Auth çağrılarını Test Module base URL'sine yönlendirmek
yanlıştır. Mimari sınır: [[04-Architecture/Auth-Consumption-Model|ARCH-AUTH-CONSUMPTION]].

## UI'yi doğrudan etkileyen çelişkiler ve blokajlar

> [!CAUTION] Aynı method+route iki checker tarafından sahipleniliyor
> Aşağıdaki dört imza Test Module endpoint tablosunda iki kez bulunur. Permission farkı route
> seçmez; çağrı runtime'da belirsiz eşleşmeye düşebilir. Backend route namespace'i ayrılmadan UI
> bu dört imzayı kullanmamalıdır.

| Method | Route | API Contract Checker | Database Checker |
|---|---|---|---|
| `GET` | `/api/lookups/difference-kinds` | `DifferenceKind.GetList` | `DifferenceKind.GetList` |
| `POST` | `/api/lookups/difference-kinds` | `DifferenceKind.Create` | `DifferenceKind.Create` |
| `GET` | `/api/lookups/difference-kinds/{id}` | `DifferenceKind.Get` | `DifferenceKind.Get` |
| `PUT` | `/api/lookups/difference-kinds/{id}` | `DifferenceKind.Update` | `DifferenceKind.Update` |

`DatabaseChecker` tarafındaki `DELETE /api/lookups/difference-kinds/{id}` ve API Checker tarafındaki
`POST /api/lookups/difference-kinds/{id}/passivate` benzersizdir; çakışan dört imzadan farklıdır.

> [!WARNING] Email gönderme action'ında transport authorization metadata'sı yok
> `POST /api/emailing/emails` mevcut hostta `Unspecified` görünür. UI bu action'ı doğrudan
> son kullanıcıya açmamalıdır; backend permission/fallback policy kararı verilmeden güvenli kabul
> edilmez. Google callback ve notification SSE stream ise tasarım gereği açıkça `Anonymous`tır.

> [!WARNING] Yerel HTTP smoke ortamı eksik ABP tabloları nedeniyle hazır değildi
> Host migration ve seed kapalıyken ayağa kalktı; ancak normal Swagger isteği yerel veritabanında
> `abp.AbpSettings` tablosu bulunmadığı için middleware'de 500 verdi. Aşağıdaki yüzey host DI ve
> ApiExplorer'dan alınmıştır; başarılı uçtan uca HTTP/authorization smoke kanıtı değildir.
>
> **Kompozisyon tarafı KBP-112 ile kapandı (2026-08-16, `06bc2d3`).** Host artık
> `AbpSettingManagementApplicationModule` · `…EntityFrameworkCoreModule` · `…HttpApiModule`
> compose eder. Tablonun sahibi Authenticator'dır — `20260809140749_Initial.cs:165` — ve
> Test Module `ConfigureSettingManagement()` çağırmaz, migration üretmez (RULE-0002).
> **Kalan koşul artık kod değil kurulumdur:** aynı veritabanına Authenticator migration'ları
> uygulanmış olmalıdır. Uygulanmadan Swagger 500'ü tekrar eder.

## Controller olmayan host endpoint'leri

| Endpoint | Tür | Erişim/not |
|---|---|---|
| `/health` | Health check | Controller değildir |
| `/mcp` | MCP HTTP transport | `RequireAuthorization()` ile korunur; REST UI yüzeyi değildir |
| `/swagger` · `/swagger/v1/swagger.json` | API keşfi | Controller değildir; ortam DB'si hazır olmalıdır |

## Tam controller/action envanteri

## Test Module — 13 controller / 54 action

### `BusinessInvariantController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Check` | `POST` | `/api/test-module/invariants/check` | `TestModule.Bridge.Invariant` | `input:BusinessInvariantRequestDto` | `200:Result<BusinessInvariantResultDto>` |

### `PtnBridgeController` — 9 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `ResolveAgentProfile` | `POST` | `/api/test-module/bridge/agent-profile` | `TestModule.Bridge.Profile` | `input:AgentProfileRequestDto` | `200:Result<AgentProfileDto>` |
| `Explain` | `POST` | `/api/test-module/bridge/explain` | `TestModule.Bridge.Explain` | `input:ExplainRequestDto` | `200:Result<ExplainResultDto>` |
| `Ground` | `POST` | `/api/test-module/bridge/ground` | `TestModule.Bridge.Ground` | `input:GroundRequestDto` | `200:Result<GroundResultDto>` |
| `GetKnowledge` | `POST` | `/api/test-module/bridge/knowledge` | `TestModule.Bridge.Knowledge` | `input:KnowledgeRequestDto` | `200:Result<KnowledgeResultDto>` |
| `SuggestOverlayPatch` | `POST` | `/api/test-module/bridge/overlay-suggestion` | `TestModule.Bridge.PatchSuggest` | `input:OverlayPatchRequestDto` | `200:Result<OverlayPatchSuggestionDto>` |
| `MapTaskStatus` | `POST` | `/api/test-module/bridge/task-status` | `TestModule.Bridge.Task` | `input:McpTaskStatusRequestDto` | `200:Result<McpTaskStatusDto>` |
| `CheckToolBudget` | `POST` | `/api/test-module/bridge/tool-budget` | `TestModule.Bridge.Profile` | `input:ToolBudgetRequestDto` | `200:Result<ToolBudgetDecisionDto>` |
| `GetToolCatalog` | `GET` | `/api/test-module/bridge/tools` | `TestModule.Bridge.Knowledge` | `-` | `200:Result<ToolCatalogDto>` |
| `Validate` | `POST` | `/api/test-module/bridge/validate` | `TestModule.Bridge.Validate` | `input:ValidateRequestDto` | `200:Result<ValidateResultDto>` |

### `ScenarioCoverageController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetCoverage` | `GET` | `/api/test-module/coverage` | `TestModule.Scenarios` | `-` | `200:Result<ScenarioCoverageReportDto>` |

### `ScenarioHealthController` — 2 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/scenario-health` | `TestModule.Runs.View` | `input:ScenarioHealthListInput` | `200:Result<PagedResultDto<ScenarioHealthDto>>` |
| `GetByScenarioKey` | `GET` | `/api/test-module/scenario-health/{scenarioKey}` | `TestModule.Runs.View` | `scenarioKey:String` | `200:Result<ScenarioHealthDto>` |

### `TestEnvironmentController` — 2 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/environments` | `TestModule.Runs.View` | `-` | `200:Result<List<TestEnvironmentBindingDto>>` |
| `ResetSandbox` | `POST` | `/api/test-module/environments/{key}/sandbox/reset` | `TestModule.Runs.SandboxReset` | `key:String` | `200:Result` |

### `TestFailureCategoryController` — 2 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/lookups/failure-categories` | `TestModule.Lookups` | `input:LookupListInput` | `200:Result<PagedResultDto<TestFailureCategoryDto>>` |
| `Get` | `GET` | `/api/test-module/lookups/failure-categories/{id}` | `TestModule.Lookups` | `id:Guid` | `200:Result<TestFailureCategoryDto>` |

### `TestFindingController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/findings` | `TestModule.Runs.View` | `input:TestFindingListInput` | `200:Result<PagedResultDto<TestFindingHeaderDto>>` |

### `TestOutcomeStatusController` — 2 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/lookups/outcome-statuses` | `TestModule.Lookups` | `input:LookupListInput` | `200:Result<PagedResultDto<TestOutcomeStatusDto>>` |
| `Get` | `GET` | `/api/test-module/lookups/outcome-statuses/{id}` | `TestModule.Lookups` | `id:Guid` | `200:Result<TestOutcomeStatusDto>` |

### `TestRunController` — 15 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/runs` | `TestModule.Runs.View` | `input:TestRunListInput` | `200:Result<PagedResultDto<TestRunHeaderDto>>` |
| `Create` | `POST` | `/api/test-module/runs` | `TestModule.Runs.Trigger` | `input:CreateTestRunDto` | `200:Result<TestRunDto>` |
| `Get` | `GET` | `/api/test-module/runs/{id}` | `TestModule.Runs.View` | `id:Guid` | `200:Result<TestRunDto>` |
| `Cancel` | `POST` | `/api/test-module/runs/{id}/cancel` | `TestModule.Runs.Cancel` | `id:Guid` | `200:Result` |
| `GetDryRunContradiction` | `GET` | `/api/test-module/runs/{id}/dry-run-contradiction` | `TestModule.Runs.View` | `id:Guid` | `200:Result<DryRunContradictionReportDto>` |
| `Export` | `POST` | `/api/test-module/runs/{id}/export` | `TestModule.Runs.Export` | `id:Guid` | `200:Result<RunArtifactLinksDto>` |
| `GetHarContent` | `GET` | `/api/test-module/runs/{id}/har` | `TestModule.Runs.View` | `id:Guid` | `200:Result<RunArtifactContentDto>` |
| `GetReport` | `GET` | `/api/test-module/runs/{id}/report` | `TestModule.Runs.View` | `id:Guid` | `200:Result<TestReportDetailDto>` |
| `Start` | `POST` | `/api/test-module/runs/{id}/start` | `TestModule.Runs.Start` | `id:Guid` | `200:Result<TestRunClaimDto>` |
| `WriteTerminal` | `POST` | `/api/test-module/runs/{id}/terminal` | `TestModule.Runs.WriteResult` | `id:Guid,input:WriteTestRunTerminalDto` | `200:Result<TestRunResultDto>` |
| `GetResult` | `GET` | `/api/test-module/runs/results/{id}` | `TestModule.Runs.View` | `id:Guid` | `200:Result<TestRunResultDto>` |
| `GetArtifactLinks` | `GET` | `/api/test-module/runs/results/{id}/artifacts` | `TestModule.Runs.View` | `id:Guid` | `200:Result<RunArtifactLinksDto>` |
| `GetArtifactContent` | `GET` | `/api/test-module/runs/results/{id}/artifacts/{format}` | `TestModule.Runs.View` | `id:Guid,format:String` | `200:Result<RunArtifactContentDto>` |
| `Trigger` | `POST` | `/api/test-module/runs/trigger` | `TestModule.Runs.Trigger` | `input:CreateTestRunDto` | `202:void` |
| `TriggerByWebhook` | `POST` | `/api/test-module/runs/webhook` | `Anonymous` | `secret:String,input:WebhookTestRunDto` | `202:void` |

### `TestRunStatusController` — 2 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/lookups/run-statuses` | `TestModule.Lookups` | `input:LookupListInput` | `200:Result<PagedResultDto<TestRunStatusDto>>` |
| `Get` | `GET` | `/api/test-module/lookups/run-statuses/{id}` | `TestModule.Lookups` | `id:Guid` | `200:Result<TestRunStatusDto>` |

### `TestScenarioController` — 13 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/scenarios` | `TestModule.Scenarios` | `input:TestScenarioListInput` | `200:Result<PagedResultDto<TestScenarioDto>>` |
| `Create` | `POST` | `/api/test-module/scenarios` | `TestModule.Scenarios.Create` | `input:CreateTestScenarioDto` | `200:Result<TestScenarioDto>` |
| `Delete` | `DELETE` | `/api/test-module/scenarios/{id}` | `TestModule.Scenarios.Delete` | `id:Guid` | `200:Result` |
| `Get` | `GET` | `/api/test-module/scenarios/{id}` | `TestModule.Scenarios` | `id:Guid` | `200:Result<TestScenarioDto>` |
| `Update` | `PUT` | `/api/test-module/scenarios/{id}` | `TestModule.Scenarios.Update` | `id:Guid,input:UpdateTestScenarioDto` | `200:Result<TestScenarioDto>` |
| `Deprecate` | `POST` | `/api/test-module/scenarios/{id}/deprecate` | `TestModule.Scenarios.Update` | `id:Guid` | `200:Result<TestScenarioDto>` |
| `EvaluatePublication` | `POST` | `/api/test-module/scenarios/{id}/evaluate-publication` | `TestModule.Scenarios.Publish` | `id:Guid` | `200:Result<TestScenarioPublishDecisionDto>` |
| `Publish` | `POST` | `/api/test-module/scenarios/{id}/publish` | `TestModule.Scenarios.Approve,TestModule.Scenarios.Publish` | `id:Guid` | `200:Result<TestScenarioDto>` |
| `Quarantine` | `POST` | `/api/test-module/scenarios/{id}/quarantine` | `TestModule.Scenarios.Quarantine` | `id:Guid,input:QuarantineTestScenarioDto` | `200:Result<TestScenarioDto>` |
| `ReleaseQuarantine` | `POST` | `/api/test-module/scenarios/{id}/quarantine/release` | `TestModule.Scenarios.Quarantine` | `id:Guid` | `200:Result<TestScenarioDto>` |
| `UpdateSchedule` | `PUT` | `/api/test-module/scenarios/{id}/schedule` | `TestModule.Scenarios.Schedule` | `id:Guid,input:UpdateScenarioScheduleDto` | `200:Result<TestScenarioDto>` |
| `SubmitForApproval` | `POST` | `/api/test-module/scenarios/{id}/submit-for-approval` | `TestModule.Scenarios.Update` | `id:Guid` | `200:Result<TestScenarioDto>` |
| `CompilePreview` | `POST` | `/api/test-module/scenarios/compile-preview` | `TestModule.Scenarios.Update` | `input:ScenarioCompilePreviewDto` | `200:Result<ScenarioCompilePreviewResultDto>` |

### `TestScenarioStateController` — 2 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/lookups/scenario-states` | `TestModule.Lookups` | `input:LookupListInput` | `200:Result<PagedResultDto<TestScenarioStateDto>>` |
| `Get` | `GET` | `/api/test-module/lookups/scenario-states/{id}` | `TestModule.Lookups` | `id:Guid` | `200:Result<TestScenarioStateDto>` |

### `TestTriggerKindController` — 2 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/test-module/lookups/trigger-kinds` | `TestModule.Lookups` | `input:LookupListInput` | `200:Result<PagedResultDto<TestTriggerKindDto>>` |
| `Get` | `GET` | `/api/test-module/lookups/trigger-kinds/{id}` | `TestModule.Lookups` | `id:Guid` | `200:Result<TestTriggerKindDto>` |

## API Contract Checker — 10 controller / 53 action

### `CheckRunStatusController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/check-run-statuses` | `ApiContractChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<CheckRunStatusDto>>` |
| `Create` | `POST` | `/api/lookups/check-run-statuses` | `ApiContractChecker.Lookups.Manage` | `input:CreateCheckRunStatusDto` | `200:Result<CheckRunStatusDto>` |
| `Get` | `GET` | `/api/lookups/check-run-statuses/{id}` | `ApiContractChecker.Lookups.View` | `id:Guid` | `200:Result<CheckRunStatusDto>` |
| `Update` | `PUT` | `/api/lookups/check-run-statuses/{id}` | `ApiContractChecker.Lookups.Manage` | `id:Guid,input:UpdateCheckRunStatusDto` | `200:Result<CheckRunStatusDto>` |
| `Passivate` | `POST` | `/api/lookups/check-run-statuses/{id}/passivate` | `ApiContractChecker.Lookups.Manage` | `id:Guid` | `200:Result<CheckRunStatusDto>` |

### `ContractCheckRunController` — 6 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/checks` | `ApiContractChecker.Checks.View` | `input:GetContractCheckRunsInput` | `200:Result<PagedResultDto<ContractCheckRunHeaderDto>>` |
| `Execute` | `POST` | `/api/checks` | `ApiContractChecker.Checks.Execute,ApiContractChecker.Checks.View` | `input:ExecuteContractCheckDto` | `202:Result<ContractCheckRunStatusDto>` |
| `Get` | `GET` | `/api/checks/{id}` | `ApiContractChecker.Checks.View` | `id:Guid` | `200:Result<ContractCheckRunDetailDto>` |
| `GetFindings` | `GET` | `/api/checks/{id}/findings` | `ApiContractChecker.Checks.View` | `id:Guid,input:GetContractCheckFindingsInput` | `200:Result<FindingPagedResultDto>` |
| `GetReport` | `GET` | `/api/checks/{id}/report` | `ApiContractChecker.Checks.View` | `id:Guid` | `200:Result<ContractCheckReportDto>` |
| `GetStatus` | `GET` | `/api/checks/{id}/status` | `ApiContractChecker.Checks.View` | `id:Guid` | `200:Result<ContractCheckRunStatusDto>` |

### `DiagnosisController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Diagnose` | `POST` | `/api/contract-checks/diagnosis` | `ApiContractChecker.Diagnosis.Execute` | `input:DiagnoseRequestDto` | `200:Result<DiagnosisReportDto>` |

### `DifferenceDirectionController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/difference-directions` | `ApiContractChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<DifferenceDirectionDto>>` |
| `Create` | `POST` | `/api/lookups/difference-directions` | `ApiContractChecker.Lookups.Manage` | `input:CreateDifferenceDirectionDto` | `200:Result<DifferenceDirectionDto>` |
| `Get` | `GET` | `/api/lookups/difference-directions/{id}` | `ApiContractChecker.Lookups.View` | `id:Guid` | `200:Result<DifferenceDirectionDto>` |
| `Update` | `PUT` | `/api/lookups/difference-directions/{id}` | `ApiContractChecker.Lookups.Manage` | `id:Guid,input:UpdateDifferenceDirectionDto` | `200:Result<DifferenceDirectionDto>` |
| `Passivate` | `POST` | `/api/lookups/difference-directions/{id}/passivate` | `ApiContractChecker.Lookups.Manage` | `id:Guid` | `200:Result<DifferenceDirectionDto>` |

### `DifferenceKindController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/difference-kinds` | `ApiContractChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<DifferenceKindDto>>` |
| `Create` | `POST` | `/api/lookups/difference-kinds` | `ApiContractChecker.Lookups.Manage` | `input:CreateDifferenceKindDto` | `200:Result<DifferenceKindDto>` |
| `Get` | `GET` | `/api/lookups/difference-kinds/{id}` | `ApiContractChecker.Lookups.View` | `id:Guid` | `200:Result<DifferenceKindDto>` |
| `Update` | `PUT` | `/api/lookups/difference-kinds/{id}` | `ApiContractChecker.Lookups.Manage` | `id:Guid,input:UpdateDifferenceKindDto` | `200:Result<DifferenceKindDto>` |
| `Passivate` | `POST` | `/api/lookups/difference-kinds/{id}/passivate` | `ApiContractChecker.Lookups.Manage` | `id:Guid` | `200:Result<DifferenceKindDto>` |

### `DifferenceSeverityController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/difference-severities` | `ApiContractChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<DifferenceSeverityDto>>` |
| `Create` | `POST` | `/api/lookups/difference-severities` | `ApiContractChecker.Lookups.Manage` | `input:CreateDifferenceSeverityDto` | `200:Result<DifferenceSeverityDto>` |
| `Get` | `GET` | `/api/lookups/difference-severities/{id}` | `ApiContractChecker.Lookups.View` | `id:Guid` | `200:Result<DifferenceSeverityDto>` |
| `Update` | `PUT` | `/api/lookups/difference-severities/{id}` | `ApiContractChecker.Lookups.Manage` | `id:Guid,input:UpdateDifferenceSeverityDto` | `200:Result<DifferenceSeverityDto>` |
| `Passivate` | `POST` | `/api/lookups/difference-severities/{id}/passivate` | `ApiContractChecker.Lookups.Manage` | `id:Guid` | `200:Result<DifferenceSeverityDto>` |

### `ResponseConformanceController` — 7 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `ValidateScenarioAssertions` | `POST` | `/api/contract-checks/conformance/assertion-derivability` | `ApiContractChecker.Conformance.Execute` | `input:AssertionDerivabilityDto` | `200:Result<AssertionDerivabilityResultDto>` |
| `SuggestOperationBindings` | `POST` | `/api/contract-checks/conformance/operation-bindings` | `ApiContractChecker.Conformance.Execute` | `input:OperationSelectionDto` | `200:Result<OperationBindingResultDto>` |
| `SuggestOperationLinks` | `POST` | `/api/contract-checks/conformance/operation-links` | `ApiContractChecker.Conformance.Execute,ApiContractChecker.Conformance.SuggestLinks` | `input:OperationLinkRequestDto` | `200:Result<OperationLinkResultDto>` |
| `AssertRequest` | `POST` | `/api/contract-checks/conformance/request` | `ApiContractChecker.Conformance.Execute` | `input:RequestConformanceDto` | `200:Result<ConformanceResultDto>` |
| `BuildRequestExample` | `POST` | `/api/contract-checks/conformance/request-example` | `ApiContractChecker.Conformance.Execute` | `input:OperationSelectionDto` | `200:Result<RequestExampleDto>` |
| `AssertResponse` | `POST` | `/api/contract-checks/conformance/response` | `ApiContractChecker.Conformance.Execute` | `input:ResponseConformanceDto` | `200:Result<ConformanceResultDto>` |
| `BuildSampleSet` | `POST` | `/api/contract-checks/conformance/sample-sets` | `ApiContractChecker.Conformance.Execute,ApiContractChecker.Conformance.GenerateSamples` | `input:SampleSetRequestDto` | `200:Result<SampleSetResultDto>` |

### `SpecFormatController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/spec-formats` | `ApiContractChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<SpecFormatDto>>` |
| `Create` | `POST` | `/api/lookups/spec-formats` | `ApiContractChecker.Lookups.Manage` | `input:CreateSpecFormatDto` | `200:Result<SpecFormatDto>` |
| `Get` | `GET` | `/api/lookups/spec-formats/{id}` | `ApiContractChecker.Lookups.View` | `id:Guid` | `200:Result<SpecFormatDto>` |
| `Update` | `PUT` | `/api/lookups/spec-formats/{id}` | `ApiContractChecker.Lookups.Manage` | `id:Guid,input:UpdateSpecFormatDto` | `200:Result<SpecFormatDto>` |
| `Passivate` | `POST` | `/api/lookups/spec-formats/{id}/passivate` | `ApiContractChecker.Lookups.Manage` | `id:Guid` | `200:Result<SpecFormatDto>` |

### `SpecSnapshotController` — 6 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Get` | `GET` | `/api/snapshots/{id}` | `ApiContractChecker.Sources.View` | `id:Guid` | `200:Result<SpecSnapshotDetailDto>` |
| `ListOperations` | `GET` | `/api/snapshots/{id}/operations` | `ApiContractChecker.Sources.View` | `id:Guid,input:ListSnapshotOperationsInput` | `200:Result<SnapshotOperationInventoryDto>` |
| `FindOperation` | `POST` | `/api/snapshots/{id}/operations/find` | `ApiContractChecker.Sources.View` | `id:Guid,input:OperationSelectionDto` | `200:Result<OperationSummaryDto>` |
| `DescribeSchema` | `POST` | `/api/snapshots/{id}/schemas/describe` | `ApiContractChecker.Sources.View` | `id:Guid,input:DescribeSchemaDto` | `200:Result<SchemaDescriptionDto>` |
| `GetAuthoringResult` | `GET` | `/api/snapshots/authoring-results/{resultRef}` | `ApiContractChecker.Sources.View` | `resultRef:String` | `200:Result<SnapshotAuthoringResultDto>` |
| `GetList` | `GET` | `/api/sources/{id}/documents/{documentId}/snapshots` | `ApiContractChecker.Sources.View` | `id:Guid,documentId:Guid,input:GetSpecSnapshotsInput` | `200:Result<PagedResultDto<SpecSnapshotHeaderDto>>` |

### `SpecSourceController` — 8 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/sources` | `ApiContractChecker.Sources.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<SpecSourceDto>>` |
| `Create` | `POST` | `/api/sources` | `ApiContractChecker.Sources.Manage,ApiContractChecker.Sources.View` | `input:CreateSpecSourceDto` | `200:Result<SpecSourceDto>` |
| `Get` | `GET` | `/api/sources/{id}` | `ApiContractChecker.Sources.View` | `id:Guid` | `200:Result<SpecSourceDto>` |
| `Update` | `PUT` | `/api/sources/{id}` | `ApiContractChecker.Sources.Manage,ApiContractChecker.Sources.View` | `id:Guid,input:UpdateSpecSourceDto` | `200:Result<SpecSourceDto>` |
| `ConfigureMonitoring` | `POST` | `/api/sources/{id}/documents/{documentId}/monitoring` | `ApiContractChecker.Sources.Manage,ApiContractChecker.Sources.View` | `id:Guid,documentId:Guid,input:ConfigureSpecDocumentMonitoringDto` | `200:Result<SpecDocumentMonitoringDto>` |
| `CaptureSnapshot` | `POST` | `/api/sources/{id}/documents/{documentId}/snapshot` | `ApiContractChecker.Sources.Manage,ApiContractChecker.Sources.View` | `id:Guid,documentId:Guid` | `200:Result<SpecSnapshotDto>` |
| `Passivate` | `POST` | `/api/sources/{id}/passivate` | `ApiContractChecker.Sources.Manage,ApiContractChecker.Sources.View` | `id:Guid` | `200:Result<SpecSourceDto>` |
| `Test` | `POST` | `/api/sources/{id}/test` | `ApiContractChecker.Sources.Manage,ApiContractChecker.Sources.View` | `id:Guid` | `200:Result<SpecSourceReachabilityDto>` |

## Database Checker — 17 controller / 72 action

### `AssertionController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `AssertAbsent` | `POST` | `/api/comparison/assertions/absent` | `DatabaseChecker.Assertions,DatabaseChecker.Assertions.Execute` | `input:RowAssertionRequestDto` | `200:Result<RowAssertionResultDto>` |
| `AssertBatch` | `POST` | `/api/comparison/assertions/batch` | `DatabaseChecker.Assertions,DatabaseChecker.Assertions.Execute` | `input:List<RowAssertionRequestDto>` | `200:Result<List<RowAssertionResultDto>>` |
| `AssertCount` | `POST` | `/api/comparison/assertions/count` | `DatabaseChecker.Assertions,DatabaseChecker.Assertions.Execute` | `input:RowAssertionRequestDto` | `200:Result<RowAssertionResultDto>` |
| `ValidateDerivability` | `POST` | `/api/comparison/assertions/derivability` | `DatabaseChecker.Assertions,DatabaseChecker.Assertions.ValidateDerivability` | `input:DerivabilityRequestDto` | `200:Result<DerivabilityResultDto>` |
| `AssertRow` | `POST` | `/api/comparison/assertions/row` | `DatabaseChecker.Assertions,DatabaseChecker.Assertions.Execute` | `input:RowAssertionRequestDto` | `200:Result<RowAssertionResultDto>` |

### `ComparisonConfidenceController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/comparison-confidences` | `DatabaseChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<ComparisonConfidenceDto>>` |
| `Create` | `POST` | `/api/lookups/comparison-confidences` | `DatabaseChecker.Lookups.Manage` | `input:CreateComparisonConfidenceDto` | `200:Result<ComparisonConfidenceDto>` |
| `Delete` | `DELETE` | `/api/lookups/comparison-confidences/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid` | `200:Result` |
| `Get` | `GET` | `/api/lookups/comparison-confidences/{id}` | `DatabaseChecker.Lookups.View` | `id:Guid` | `200:Result<ComparisonConfidenceDto>` |
| `Update` | `PUT` | `/api/lookups/comparison-confidences/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid,input:UpdateComparisonConfidenceDto` | `200:Result<ComparisonConfidenceDto>` |

### `ComparisonDefinitionController` — 4 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/definitions/comparison-definitions` | `DatabaseChecker.Definitions.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<ComparisonDefinitionDto>>` |
| `Create` | `POST` | `/api/definitions/comparison-definitions` | `DatabaseChecker.Definitions.Manage,DatabaseChecker.Definitions.View` | `input:CreateComparisonDefinitionDto` | `200:Result<ComparisonDefinitionDto>` |
| `Get` | `GET` | `/api/definitions/comparison-definitions/{id}` | `DatabaseChecker.Definitions.View` | `id:Guid` | `200:Result<ComparisonDefinitionDto>` |
| `Update` | `PUT` | `/api/definitions/comparison-definitions/{id}` | `DatabaseChecker.Definitions.Manage,DatabaseChecker.Definitions.View` | `id:Guid,input:UpdateComparisonDefinitionDto` | `200:Result<ComparisonDefinitionDto>` |

### `ComparisonRunController` — 6 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/comparison/runs` | `DatabaseChecker.Runs.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<ComparisonRunDto>>` |
| `Get` | `GET` | `/api/comparison/runs/{id}` | `DatabaseChecker.Runs.View` | `id:Guid` | `200:Result<ComparisonRunDto>` |
| `GetDetail` | `GET` | `/api/comparison/runs/{id}/detail` | `DatabaseChecker.Runs.View` | `id:Guid` | `200:Result<ComparisonRunDetailDto>` |
| `GetFindings` | `GET` | `/api/comparison/runs/{id}/findings` | `DatabaseChecker.Runs.View` | `id:Guid,input:FindingQueryInput` | `200:Result<PagedResultDto<FindingDto>>` |
| `GetReport` | `GET` | `/api/comparison/runs/{id}/report` | `DatabaseChecker.Runs.View` | `id:Guid` | `200:Result<ComparisonReportDto>` |
| `Execute` | `POST` | `/api/comparison/runs/execute` | `DatabaseChecker.Runs.Create,DatabaseChecker.Runs.View` | `input:ExecuteComparisonRunDto` | `200:Result<ComparisonRunDetailDto>` |

### `ComparisonRunStatusController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/comparison-run-statuses` | `DatabaseChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<ComparisonRunStatusDto>>` |
| `Create` | `POST` | `/api/lookups/comparison-run-statuses` | `DatabaseChecker.Lookups.Manage` | `input:CreateComparisonRunStatusDto` | `200:Result<ComparisonRunStatusDto>` |
| `Delete` | `DELETE` | `/api/lookups/comparison-run-statuses/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid` | `200:Result` |
| `Get` | `GET` | `/api/lookups/comparison-run-statuses/{id}` | `DatabaseChecker.Lookups.View` | `id:Guid` | `200:Result<ComparisonRunStatusDto>` |
| `Update` | `PUT` | `/api/lookups/comparison-run-statuses/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid,input:UpdateComparisonRunStatusDto` | `200:Result<ComparisonRunStatusDto>` |

### `ComparisonTypeController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/comparison-types` | `DatabaseChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<ComparisonTypeDto>>` |
| `Create` | `POST` | `/api/lookups/comparison-types` | `DatabaseChecker.Lookups.Manage` | `input:CreateComparisonTypeDto` | `200:Result<ComparisonTypeDto>` |
| `Delete` | `DELETE` | `/api/lookups/comparison-types/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid` | `200:Result` |
| `Get` | `GET` | `/api/lookups/comparison-types/{id}` | `DatabaseChecker.Lookups.View` | `id:Guid` | `200:Result<ComparisonTypeDto>` |
| `Update` | `PUT` | `/api/lookups/comparison-types/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid,input:UpdateComparisonTypeDto` | `200:Result<ComparisonTypeDto>` |

### `DatabaseConnectionController` — 6 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/connections/database-connections` | `DatabaseChecker.Connections.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<DatabaseConnectionDto>>` |
| `Create` | `POST` | `/api/connections/database-connections` | `DatabaseChecker.Connections.Manage,DatabaseChecker.Connections.View` | `input:CreateDatabaseConnectionDto` | `200:Result<DatabaseConnectionDto>` |
| `Get` | `GET` | `/api/connections/database-connections/{id}` | `DatabaseChecker.Connections.View` | `id:Guid` | `200:Result<DatabaseConnectionDto>` |
| `Update` | `PUT` | `/api/connections/database-connections/{id}` | `DatabaseChecker.Connections.Manage,DatabaseChecker.Connections.View` | `id:Guid,input:UpdateDatabaseConnectionDto` | `200:Result<DatabaseConnectionDto>` |
| `Passivate` | `POST` | `/api/connections/database-connections/{id}/passivate` | `DatabaseChecker.Connections.Manage,DatabaseChecker.Connections.View` | `id:Guid` | `200:Result<DatabaseConnectionDto>` |
| `TestConnection` | `POST` | `/api/connections/database-connections/{id}/test-connection` | `DatabaseChecker.Connections.Manage,DatabaseChecker.Connections.View` | `id:Guid` | `200:Result<TestConnectionResultDto>` |

### `DatabaseEngineController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/database-engines` | `DatabaseChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<DatabaseEngineDto>>` |
| `Create` | `POST` | `/api/lookups/database-engines` | `DatabaseChecker.Lookups.Manage` | `input:CreateDatabaseEngineDto` | `200:Result<DatabaseEngineDto>` |
| `Delete` | `DELETE` | `/api/lookups/database-engines/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid` | `200:Result` |
| `Get` | `GET` | `/api/lookups/database-engines/{id}` | `DatabaseChecker.Lookups.View` | `id:Guid` | `200:Result<DatabaseEngineDto>` |
| `Update` | `PUT` | `/api/lookups/database-engines/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid,input:UpdateDatabaseEngineDto` | `200:Result<DatabaseEngineDto>` |

### `DiagnosisController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Diagnose` | `POST` | `/api/comparison/diagnosis` | `DatabaseChecker.Diagnosis.Execute` | `input:DiagnoseRequestDto` | `200:Result<DiagnosisReportDto>` |

### `DifferenceKindController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/difference-kinds` | `DatabaseChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<DifferenceKindDto>>` |
| `Create` | `POST` | `/api/lookups/difference-kinds` | `DatabaseChecker.Lookups.Manage` | `input:CreateDifferenceKindDto` | `200:Result<DifferenceKindDto>` |
| `Delete` | `DELETE` | `/api/lookups/difference-kinds/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid` | `200:Result` |
| `Get` | `GET` | `/api/lookups/difference-kinds/{id}` | `DatabaseChecker.Lookups.View` | `id:Guid` | `200:Result<DifferenceKindDto>` |
| `Update` | `PUT` | `/api/lookups/difference-kinds/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid,input:UpdateDifferenceKindDto` | `200:Result<DifferenceKindDto>` |

### `ProjectionController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `ProjectRows` | `POST` | `/api/comparison/projections/rows` | `DatabaseChecker.Projections.Execute` | `input:ProjectionRequestDto` | `200:Result<ProjectionResultDto>` |

### `ReportFormatController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/report-formats` | `DatabaseChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<ReportFormatDto>>` |
| `Create` | `POST` | `/api/lookups/report-formats` | `DatabaseChecker.Lookups.Manage` | `input:CreateReportFormatDto` | `200:Result<ReportFormatDto>` |
| `Delete` | `DELETE` | `/api/lookups/report-formats/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid` | `200:Result` |
| `Get` | `GET` | `/api/lookups/report-formats/{id}` | `DatabaseChecker.Lookups.View` | `id:Guid` | `200:Result<ReportFormatDto>` |
| `Update` | `PUT` | `/api/lookups/report-formats/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid,input:UpdateReportFormatDto` | `200:Result<ReportFormatDto>` |

### `SchemaComparisonController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Compare` | `POST` | `/api/comparison/schema-comparison` | `DatabaseChecker.Connections.Manage` | `input:CompareSchemaRequestDto` | `200:Result<ComparisonFindingsDto>` |

### `SchemaDiscoveryController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetSchemaFingerprint` | `GET` | `/api/comparison/schema-discovery/{connectionId}/fingerprint` | `DatabaseChecker.Connections.View` | `connectionId:Guid,schemaNames:List<String>` | `200:Result<SchemaFingerprintDto>` |
| `GetObjects` | `GET` | `/api/comparison/schema-discovery/{connectionId}/objects` | `DatabaseChecker.Connections.View` | `connectionId:Guid,schema:String` | `200:Result<List<DatabaseSchemaObjectDto>>` |
| `GetSchemas` | `GET` | `/api/comparison/schema-discovery/{connectionId}/schemas` | `DatabaseChecker.Connections.View` | `connectionId:Guid` | `200:Result<List<DatabaseSchemaDto>>` |
| `GetSnapshot` | `GET` | `/api/comparison/schema-discovery/{connectionId}/snapshot` | `DatabaseChecker.Connections.View` | `connectionId:Guid,schemaNames:List<String>` | `200:Result<SchemaSnapshotDto>` |
| `DescribeTable` | `GET` | `/api/comparison/schema-discovery/{connectionId}/tables/{schema}/{table}/describe` | `DatabaseChecker.Connections.View` | `connectionId:Guid,schema:String,table:String` | `200:Result<TableDescriptionDto>` |

### `SchemaObjectTypeController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/schema-object-types` | `DatabaseChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<SchemaObjectTypeDto>>` |
| `Create` | `POST` | `/api/lookups/schema-object-types` | `DatabaseChecker.Lookups.Manage` | `input:CreateSchemaObjectTypeDto` | `200:Result<SchemaObjectTypeDto>` |
| `Delete` | `DELETE` | `/api/lookups/schema-object-types/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid` | `200:Result` |
| `Get` | `GET` | `/api/lookups/schema-object-types/{id}` | `DatabaseChecker.Lookups.View` | `id:Guid` | `200:Result<SchemaObjectTypeDto>` |
| `Update` | `PUT` | `/api/lookups/schema-object-types/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid,input:UpdateSchemaObjectTypeDto` | `200:Result<SchemaObjectTypeDto>` |

### `ScopeKindController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/lookups/scope-kinds` | `DatabaseChecker.Lookups.View` | `input:PagedResultRequestDto` | `200:Result<PagedResultDto<ScopeKindDto>>` |
| `Create` | `POST` | `/api/lookups/scope-kinds` | `DatabaseChecker.Lookups.Manage` | `input:CreateScopeKindDto` | `200:Result<ScopeKindDto>` |
| `Delete` | `DELETE` | `/api/lookups/scope-kinds/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid` | `200:Result` |
| `Get` | `GET` | `/api/lookups/scope-kinds/{id}` | `DatabaseChecker.Lookups.View` | `id:Guid` | `200:Result<ScopeKindDto>` |
| `Update` | `PUT` | `/api/lookups/scope-kinds/{id}` | `DatabaseChecker.Lookups.Manage` | `id:Guid,input:UpdateScopeKindDto` | `200:Result<ScopeKindDto>` |

### `WriteSetCapabilityController` — 3 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Capture` | `POST` | `/capabilities/write-set/capture` | `DatabaseChecker.Capabilities.Capture` | `input:WriteSetCaptureRequestDto` | `200:Result<WriteSetResultDto>` |
| `Probe` | `POST` | `/capabilities/write-set/probe` | `DatabaseChecker.Capabilities.Probe` | `input:CapabilityProbeRequestDto` | `200:Result<CapabilityLevelDto>` |
| `Release` | `POST` | `/capabilities/write-set/release` | `DatabaseChecker.Capabilities.Capture` | `connectionId:Guid,captureRef:Guid` | `200:Result<WriteSetResultDto>` |

## Emailing — 3 controller / 10 action

### `EmailController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Send` | `POST` | `/api/emailing/emails` | `Unspecified` | `input:SendEmailDto` | `200:void,204:void` |

### `EmailProviderController` — 4 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetGoogleAuthorization` | `GET` | `/api/emailing/platform/google/authorization` | `Piton.Emailing.Provider,Piton.Emailing.Provider.Manage` | `-` | `200:EmailAuthorizationDto` |
| `CompleteGoogleAuthorization` | `GET` | `/api/emailing/platform/google/callback` | `Anonymous` | `code:String,state:String` | `200:EmailProviderStatusDto` |
| `GetStatus` | `GET` | `/api/emailing/platform/status` | `Piton.Emailing.Provider,Piton.Emailing.Provider.View` | `-` | `200:EmailProviderStatusDto` |
| `SendTest` | `POST` | `/api/emailing/platform/test` | `Piton.Emailing.Provider,Piton.Emailing.Provider.Manage` | `input:EmailProviderTestDto` | `200:void,204:void` |

### `EmailTemplateController` — 5 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `GetList` | `GET` | `/api/email-templates` | `Piton.Emailing.EmailTemplates` | `input:PagedAndSortedResultRequestDto` | `200:PagedResultDto<EmailTemplateDto>` |
| `Create` | `POST` | `/api/email-templates` | `Piton.Emailing.EmailTemplates,Piton.Emailing.EmailTemplates.Manage` | `input:CreateEmailTemplateDto` | `200:EmailTemplateDto` |
| `Delete` | `DELETE` | `/api/email-templates/{id}` | `Piton.Emailing.EmailTemplates,Piton.Emailing.EmailTemplates.Manage` | `id:Guid` | `200:void,204:void` |
| `Get` | `GET` | `/api/email-templates/{id}` | `Piton.Emailing.EmailTemplates` | `id:Guid` | `200:EmailTemplateDto` |
| `Update` | `PUT` | `/api/email-templates/{id}` | `Piton.Emailing.EmailTemplates,Piton.Emailing.EmailTemplates.Manage` | `id:Guid,input:UpdateEmailTemplateDto` | `200:EmailTemplateDto` |

## Notifications — 1 controller / 4 action

### `NotificationController` — 4 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Publish` | `POST` | `/api/notifications/intents` | `Pintern.Notifications.Live,SystemStandards.SelectedContext:Pintern.Notifications.Live.SelectedContext` | `input:PublishNotificationIntentInput` | `200:Result<NotificationDeliveryOutcomeDto>` |
| `GetOutcome` | `GET` | `/api/notifications/intents/{eventId}/outcome` | `Pintern.Notifications.Live,SystemStandards.SelectedContext:Pintern.Notifications.Live.SelectedContext` | `eventId:Guid` | `200:Result<NotificationDeliveryOutcomeDto>` |
| `Stream` | `GET` | `/api/notifications/live/stream` | `Anonymous` | `ticket:String` | `200:void` |
| `IssueStreamTicket` | `POST` | `/api/notifications/live/ticket` | `Pintern.Notifications.Live,SystemStandards.SelectedContext:Pintern.Notifications.Live.SelectedContext` | `-` | `200:Result<NotificationSseTicketDto>` |

## ABP framework — 3 controller / 3 action

### `AbpApiDefinitionController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Get` | `GET` | `/api/abp/api-definition` | `Unspecified` | `model:ApplicationApiDescriptionModelRequestDto` | `200:ApplicationApiDescriptionModel` |

### `AbpApplicationConfigurationController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Get` | `GET` | `/api/abp/application-configuration` | `Unspecified` | `options:ApplicationConfigurationRequestOptions` | `200:ApplicationConfigurationDto` |

### `AbpApplicationLocalizationController` — 1 action

| Action | HTTP | Route | Erişim | Input | Başarılı response |
|---|---|---|---|---|---|
| `Get` | `GET` | `/api/abp/application-localization` | `Unspecified` | `input:ApplicationLocalizationRequestDto` | `200:ApplicationLocalizationDto` |

## Kanıt ve yenileme kuralı

- Composition sahibi: `ptn-test-module/host/Ptn.TestModule.HttpApi.Host/TestModuleHttpApiHostModule.cs`.
- Envanter kaynağı: hostun Release binary'si ile kurulan gerçek ABP service graph içindeki
  `IApiDescriptionGroupCollectionProvider`.
- Toplama sırasında `Database:AutoMigrate=false`, `Database:SeedOnStartup=false` kullanıldı;
  veritabanı şeması veya seed verisi değiştirilmedi.
- Checker/Emailing/Notifications paket sürümü ya da host `[DependsOn]` grafiği değiştiğinde bu
  sayfa yeniden runtime ApiExplorer/Swagger çıktısından üretilir. Kaynak dosya saymak yeterli kanıt
  değildir.
- Runtime envanteri: **47 controller / 196 action / 192 benzersiz method+route**. Dört
  method+route çakışması yukarıdaki blokaj tablosunda açıkça tutulur.

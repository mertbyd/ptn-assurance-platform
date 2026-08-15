# KBP-105 · PLAN-0003 Blok 6/7 ölçüm defteri

Kaynak PLAN-0003 dosyası bu checkout'ta bulunmadığı için madde adları attachment kapsamı ve canlı kod kanıtıyla ölçülmüştür. Bu dosya eksik planın yerine geçmez; KBP-105 kapanış kanıtıdır.

| Madde | Durum | Kanıt |
|---|---|---|
| TM-32 | ✅ | `ProfilePackManager.cs` — kapalı profil bilgisi |
| TM-33 | ✅ | `PtnBridgeMapper.cs` — compile-time DTO/model eşleme |
| TM-34 | ✅ | `GroundingManager.cs` — deterministik grounding |
| TM-35 | ✅ | `EvidenceChainManager.cs` — veri güdümlü kanıt yolu |
| TM-36 | ✅ | `ToolCatalogManager.cs` — progressive disclosure |
| TM-37 | ✅ | `PtnResponseFormatCodes.cs` — concise/detailed cevap |
| TM-38 | ✅ | `PtnBridgeContextKeys.cs` — kapalı context sözlüğü |
| TM-39 | ✅ | `PtnToolCodes.cs` — 12 tool protokol tavanı |
| TM-40 | ✅ | `ServiceShapeTests.cs` — AppService iş-helper kapısı |
| TM-41 | ✅ | `TestRunResult.cs` — `TakenBranchPath` |
| TM-42 | ✅ | `OperationLinkResultDto.cs` — operasyon-link zinciri |
| TM-43 | ✅ | `ForeignKeyNeighborDto.cs` — yönlü FK komşuları |
| TM-44 | ✅ | `ScenarioPublicationGateManager.cs` — derivability/yayın kapısı |
| TM-45 | ✅ | `SchemaLintGateTests.cs` — lint uyarısı yayın kanıtı |
| TM-46 | ✅ | `host/Ptn.TestModule.HttpApi.Host/Authoring/kurallar.md` + MCP Resource |
| TM-47 | ✅ | `CorrelationRefDto.cs` — trace/step korelasyonu |
| TM-48 | ✅ | `ResponseObservationDto.cs` — senaryo conformance profili |
| TM-49 | ✅ | `FootprintCapabilityManager.cs` — ölçülen write-set yeteneği |
| TM-50 | ✅ | `ManagerReachabilityTests.cs` + host MCP tool yüzeyi |

Kalıcı kapılar: 12 tool, kademe-4 görünmezliği, MCP'nin yalnız hostta olması ve koşumda model istemcisi bulunmaması.

using System;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Kalici senaryo surumunun public okunabilir gorunumunu tasir.
// sistemdeki gorevi: Entity ve audit altyapisini API tuketicisinden ayiran katalog cikti sozlesmesidir.
public sealed class TestScenarioDto : AuditedEntityDto<Guid>
{
    public string ScenarioKey { get; set; } = string.Empty;
    public int VersionNo { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid StateId { get; set; }
    public string SourceDocument { get; set; } = string.Empty;
    public string SourceHash { get; set; } = string.Empty;
    public string CompiledDocument { get; set; } = string.Empty;
    public string CompiledHash { get; set; } = string.Empty;
    public string? RulesFingerprint { get; set; }
    public Guid? SpecSnapshotId { get; set; }
    public string? SpecFingerprint { get; set; }
    public Guid? DbConnectionId { get; set; }
    public string? DbSchemaFingerprint { get; set; }
    public string? ProfileFingerprint { get; set; }
    public int AssertionCount { get; set; }
    public string? DerivabilityCode { get; set; }
    public bool AuthoredByAgent { get; set; }
    public string? AgentModelRef { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalBoundToHash { get; set; }
    public string? Notes { get; set; }
    public Guid? TenantId { get; set; }
}

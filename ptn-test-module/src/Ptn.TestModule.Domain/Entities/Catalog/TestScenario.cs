using System;
using Ptn.TestModule.Models.Catalog;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Ptn.TestModule.Entities.Catalog;

// islevi: Her satiri ayri bir surum olan test senaryosunun DBML alanlarini tasir.
// sistemdeki gorevi: Normalizasyon, gecis, onay ve derleme yazimini Manager'a birakan ince ve tenant-aware persistence kabugudur.
public class TestScenario : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ScenarioKey { get; internal set; } = string.Empty;
    public int VersionNo { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public Guid StateId { get; internal set; }
    public string SourceDocument { get; internal set; } = string.Empty;
    public string SourceHash { get; internal set; } = string.Empty;
    public string CompiledDocument { get; internal set; } = string.Empty;
    public string CompiledHash { get; internal set; } = string.Empty;
    public string? RulesFingerprint { get; internal set; }
    public Guid? SpecSnapshotId { get; internal set; }
    public string? SpecFingerprint { get; internal set; }
    public Guid? DbConnectionId { get; internal set; }
    public string? DbSchemaFingerprint { get; internal set; }
    public string? ProfileFingerprint { get; internal set; }
    public int AssertionCount { get; internal set; }
    public string? DerivabilityCode { get; internal set; }
    public bool AuthoredByAgent { get; internal set; }
    public string? AgentModelRef { get; internal set; }
    public Guid? ApprovedBy { get; internal set; }
    public DateTime? ApprovedAt { get; internal set; }
    public string? ApprovalBoundToHash { get; internal set; }
    public string? Notes { get; internal set; }

    /// <summary>Karantinanin bittigi andir; dolu oldugunda karantina aktiftir, suresiz karantina yoktur.</summary>
    public DateTime? QuarantineUntil { get; internal set; }

    /// <summary>Karantinaya alma gerekcesinin kararli referansidir.</summary>
    public string? QuarantineReason { get; internal set; }

    /// <summary>Yayinlanmis surumun UTC cron ifadesidir; zamanlama yoksa bostur.</summary>
    public string? ScheduleCron { get; internal set; }

    /// <summary>Zamanlamanin acik olup olmadigidir; cron dolu olsa da kapali tutulabilir.</summary>
    public bool ScheduleEnabled { get; internal set; }

    /// <summary>Worker'in vade kararini verdigi zaman damgasidir.</summary>
    public DateTime? NextRunAt { get; internal set; }
    public Guid? TenantId { get; internal set; }

    // EF Core materializasyonu icin ayrilmis kurucudur.
    protected TestScenario()
    {
    }

    // Manager'in dogruladigi yeni surum alanlarini davranis calistirmadan atar.
    public TestScenario(
        Guid id,
        int versionNo,
        Guid stateId,
        Guid? tenantId,
        TestScenarioCreateModel model)
        : base(id)
    {
        ScenarioKey = model.ScenarioKey;
        VersionNo = versionNo;
        Title = model.Title;
        Description = model.Description;
        StateId = stateId;
        SourceDocument = model.SourceDocument;
        SourceHash = model.SourceHash!;
        RulesFingerprint = model.MaterialSeal.RulesFingerprint;
        SpecSnapshotId = model.MaterialSeal.SpecSnapshotId;
        SpecFingerprint = model.MaterialSeal.SpecFingerprint;
        DbConnectionId = model.MaterialSeal.DbConnectionId;
        DbSchemaFingerprint = model.MaterialSeal.DbSchemaFingerprint;
        ProfileFingerprint = model.MaterialSeal.ProfileFingerprint;
        DerivabilityCode = model.DerivabilityCode;
        AuthoredByAgent = model.AuthoredByAgent;
        AgentModelRef = model.AgentModelRef;
        Notes = model.Notes;
        TenantId = tenantId;
    }
}

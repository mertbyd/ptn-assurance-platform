using System;
using System.Collections.Generic;
using Ptn.TestModule.Constants.Authoring;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Volo.Abp.Caching;

namespace Ptn.TestModule.Models.Authoring;

// islevi: Tenant kapsamli gecici yazarlik sorularini, cevaplarini ve mekanik adimlarini tasir.
// sistemdeki gorevi: Kalici tablo acmadan ABP distributed cache'te TTL ile yasayan session kaydidir.
[CacheName(AuthoringSessionConsts.CacheName)]
public sealed class AuthoringSession
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid SpecSnapshotId { get; set; }
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowSummary { get; set; } = string.Empty;
    public string ApiSourceUrl { get; set; } = string.Empty;
    public string DatabaseSourceUrl { get; set; } = string.Empty;
    public List<ClosedQuestion> Questions { get; set; } = [];
    public Dictionary<string, string> Answers { get; set; } = new(StringComparer.Ordinal);
    public List<OperationSuggestion> Operations { get; set; } = [];
    public List<AuthoringStep> Steps { get; set; } = [];
    public string SourceDocument { get; set; } = string.Empty;
    public long TtlMs { get; set; }
}

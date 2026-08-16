using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Scopes;

namespace Ptn.DatabaseChecker.BackgroundJobs.Runs;

// islevi: Kuyruga alinmis comparison run execution isinin yeniden denenebilir payload'ini tasir.
// sistemdeki gorevi: ABP background-job akisinda run/tenant baglamini ve yalniz bu calistirmaya ait scope kurallarini entity alanina yazmadan tasir; secret tasimaz.
public class ComparisonRunExecutionJobArgs : ITenantBackgroundJobArgs
{
    // Calistirilacak Pending run kimligi.
    public Guid ComparisonRunId { get; set; }

    // Job baslarken acilacak tenant baglami.
    public Guid? TenantId { get; set; }

    // Yalniz bu execution icin kullanilacak scope kurallari; comparison definition veya run tablosuna yazilmaz.
    public List<ScopeRuleDto> ScopeRules { get; set; } = new();
}

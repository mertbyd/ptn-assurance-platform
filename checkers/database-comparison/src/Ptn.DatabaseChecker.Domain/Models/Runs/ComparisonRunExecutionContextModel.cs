using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Models.Comparison.Scope;

namespace Ptn.DatabaseChecker.Models.Runs;

// islevi: Bir Pending run'in dis veritabani I/O'su icin gereken, uygulama DB UOW'sinden ayrilmis operasyonel baglamini tasir.
// sistemdeki gorevi: Run/definition/lookup okumalarini kisa UOW'de bitirir; uzun schema/data karsilastirmasi acik uygulama DB transaction'i tutmadan bu modelle calisir.
public class ComparisonRunExecutionContextModel
{
    // Sonucu yazilacak run kimligi.
    public Guid ComparisonRunId { get; set; }

    // Run'in dogdugu definition kimligi; completion mapping mevcut FK snapshot'ini korur.
    public Guid ComparisonDefinitionId { get; set; }

    // Run'in kuyruga alinirken snapshotlanan kaynak baglanti entity'si; secret path tasir, parola tasimaz.
    public DatabaseConnection SourceConnection { get; set; } = default!;

    // Run'in kuyruga alinirken snapshotlanan hedef baglanti entity'si; secret path tasir, parola tasimaz.
    public DatabaseConnection TargetConnection { get; set; } = default!;

    // Run'a snapshotlanan comparison type lookup kimligi.
    public Guid ComparisonTypeId { get; set; }

    // Motor branch'ini belirleyen kararli comparison type kodu.
    public string ComparisonTypeCode { get; set; } = default!;

    /// <summary>
    /// SourceConnection tarafinin Reference veya Audited rolu; siddet siniflandirici bu yonu kullanir.
    /// </summary>
    public string SourceRoleCode { get; set; } = ComparisonSideRoleCodes.Reference;

    // Execution baslarken aktif tariften okunan runtime scope kurallari.
    public List<ComparisonScopeRule> ScopeRules { get; set; } = new();

    // Basarili execution sonunda kullanilacak Completed status lookup kimligi.
    public Guid CompletedStatusId { get; set; }

    // Worker'in run'i Running olarak claim ettigi an; sonuc suresi bu noktadan baslar.
    public DateTime StartedAt { get; set; }
}

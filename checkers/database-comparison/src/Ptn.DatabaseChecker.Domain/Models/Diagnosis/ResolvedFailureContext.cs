using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Models.Diagnosis;

// islevi: Kimlikteki adlari canli katalogdan dogrulanmis tablo, kolon, constraint, anahtar ve ayar olgularina yerellestirir.
// sistemdeki gorevi: Hipotez kurallarinin hata kodu yerine provider-bagimsiz gercekler uzerinden uygulanabilirlik yordami yapmasini saglar.
public sealed class ResolvedFailureContext
{
    public string EngineCode { get; set; } = string.Empty;
    public ObjectReference Location { get; set; } = new();
    public SchemaTableModel? Table { get; set; }
    public SchemaColumnModel? Column { get; set; }
    public SchemaConstraintModel? Constraint { get; set; }
    public SchemaIndexModel? UniqueIndex { get; set; }
    public bool RowWasReportedMissing { get; set; }
    public bool RowTimedOut { get; set; }
    public bool ValueWasReportedDifferent { get; set; }
    public List<string> MissingExpectedColumns { get; set; } = new();
    public List<FailedExpectation> FailedExpectations { get; set; } = new();
    public Dictionary<string, string?> TargetKeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string?> IdentityKeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string?> ParentKeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public TableDataStructureModel? TargetStructure { get; set; }
    public TableDataStructureModel? ParentStructure { get; set; }
    public Dictionary<string, string?> ServerSettingExpectations { get; set; } = new(StringComparer.Ordinal);

    // islevi: Scope hipotezi icin tam anahtardan daha dar ama katalogda unique dogrulanmis kimlik anahtari olup olmadigini bildirir.
    public bool HasBroaderIdentityKey()
        => IdentityKeyValues.Count > 0 && IdentityKeyValues.Count < TargetKeyValues.Count;
}

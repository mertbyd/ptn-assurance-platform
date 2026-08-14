namespace Ptn.TestModule.Models.Bridge;

// islevi: Sema snapshot'indaki tek kolonun adini ve kararli sirasini tasir.
// sistemdeki gorevi: Fingerprint girdisini checker DTO'suyla ayni navigation seklinde tutar.
public sealed class PtnSchemaColumn
{
    public string Name { get; set; } = string.Empty;
    public int Ordinal { get; set; }
}

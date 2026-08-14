namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Sema snapshot'indaki tek kolonun ad ve sirasini tasir.
// sistemdeki gorevi: Kanonik fingerprint girdisini tipli tutar.
public sealed class SchemaColumnDto
{
    public string Name { get; set; } = string.Empty;
    public int Ordinal { get; set; }
}

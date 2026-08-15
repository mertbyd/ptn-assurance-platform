namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Sema snapshot'indaki tek kolonun ad ve sirasini tasir.
// sistemdeki gorevi: Kanonik fingerprint girdisini tipli tutar.
public sealed class SchemaColumnDto
{
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int Ordinal { get; set; }
}

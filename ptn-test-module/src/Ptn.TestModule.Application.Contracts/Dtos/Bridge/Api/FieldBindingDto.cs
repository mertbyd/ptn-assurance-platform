namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Kaynak ve hedef JSON alanlari arasindaki tek baglamayi tasir.
// sistemdeki gorevi: Esleme ifadesini ve guven puanini public kontrata tasir.
public sealed class FieldBindingDto
{
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string SourcePointer { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string TargetPointer { get; set; } = string.Empty;
    /// <summary>
    /// Sozlesmenin type bilgisini belirtir.
    /// </summary>
    public string? Type { get; set; }
    /// <summary>
    /// Karar veya eslesme icin kullanilan sayisal olcuyu belirtir.
    /// </summary>
    public int Score { get; set; }
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string Expression { get; set; } = string.Empty;
}

namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Response assertion yollarinin turetilebilirlik istegini tasir.
// sistemdeki gorevi: Public girdiyi tipli operasyon ve assertion alanlariyla sinirlar.
public sealed class DerivabilityRequestDto
{
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public Guid SnapshotId { get; set; }
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public string? OperationId { get; set; }
    /// <summary>
    /// HTTP operasyonunun yontemini belirtir.
    /// </summary>
    public string Method { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string? StatusCode { get; set; }
    /// <summary>
    /// Sonucun ilgili tanimlayici veya aciklama degerini belirtir.
    /// </summary>
    public string? MediaType { get; set; }
    /// <summary>
    /// Dogrulama veya assertion girdilerini kararli sirada listeler.
    /// </summary>
    public List<string> AssertionPaths { get; set; } = [];
}

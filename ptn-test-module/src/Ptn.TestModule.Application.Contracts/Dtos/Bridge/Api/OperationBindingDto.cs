namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Normalize operasyon esleme sonucunu ve adaylarini tasir.
// sistemdeki gorevi: API checker ayrintilarini public Bridge sozlesmesinden gizler.
public sealed class OperationBindingDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string OutcomeCode { get; set; } = string.Empty;
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<OperationSuggestionDto> Suggestions { get; set; } = [];
}

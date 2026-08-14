using Ptn.TestModule.Dtos.Bridge;

namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Normalize database assertion sonucunu tasir.
// sistemdeki gorevi: Redaksiyonlu sonucu public Bridge cevabi olarak sunar.
public sealed class AssertionResultDto
{
    /// <summary>
    /// Degerin yayinlanan kontrollu sozluk kodunu belirtir.
    /// </summary>
    public string OutcomeCode { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili yetenek, sonuc veya durumun etkin olup olmadigini belirtir.
    /// </summary>
    public bool Passed { get; set; }
    /// <summary>
    /// Checker tarafindan gozlenen satir sayisini belirtir.
    /// </summary>
    public long ObservedRowCount { get; set; }
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public long ObservedAtMs { get; set; }
    /// <summary>
    /// Isleme ait sayisal sinir, sira veya durum degerini belirtir.
    /// </summary>
    public int AttemptCount { get; set; }
    /// <summary>
    /// Dogrulama veya assertion girdilerini kararli sirada listeler.
    /// </summary>
    public List<FailedExpectationDto> FailedExpectations { get; set; } = [];
    /// <summary>
    /// Satir assertion sonucunun kisa ozetini belirtir.
    /// </summary>
    public Dictionary<string, string?>? RowSummary { get; set; }
    /// <summary>
    /// Checker cagrisi ile cevabi eslestiren korelasyon bilgisini tasir.
    /// </summary>
    public CorrelationRefDto? Correlation { get; set; }
}

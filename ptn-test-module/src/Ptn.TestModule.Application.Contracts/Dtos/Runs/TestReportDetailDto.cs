namespace Ptn.TestModule.Dtos.Runs;

// islevi: Bir kosumu terminal hukmu, teshis raporu ve bulgulariyla birlikte tek public gorunumde tasir.
// sistemdeki gorevi: Ajanin tek istekle tam teshis yuzeyini okumasini saglar; liste ucu bu agir alanlari tasimaz.
/// <summary>Bir test kosumunun bulgulu ve teshisli public rapor gorunumudur.</summary>
public sealed class TestReportDetailDto
{
    /// <summary>Raporun ait oldugu kosum gorunumudur.</summary>
    public TestRunDto Run { get; set; } = new();

    /// <summary>Bulgulari ve diagnosis raporunu tasiyan en son terminal sonuctur; kosum terminale girmemisse null kalir.</summary>
    public TestRunResultDto? Result { get; set; }
}

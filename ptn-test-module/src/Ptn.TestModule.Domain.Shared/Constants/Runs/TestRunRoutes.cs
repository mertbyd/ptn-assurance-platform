namespace Ptn.TestModule.Constants.Runs;

// islevi: Test kosumu HTTP endpoint adreslerini ve Swagger grubunu tek sahipte tanimlar.
// sistemdeki gorevi: Controller route metinlerinin transport katmanina dagilmasini engeller.
/// <summary>Test kosumu HTTP rota sabitlerini tasir.</summary>
public static class TestRunRoutes
{
    /// <summary>Test kosumu koleksiyonunun kok rotasidir.</summary>
    public const string Root = "api/test-module/runs";

    /// <summary>Kimlige gore tek kosum rotasidir.</summary>
    public const string ById = "{id:guid}";

    /// <summary>Kosumu olusturup dayanikli icra kuyruguna veren alt rotadir.</summary>
    public const string Trigger = "trigger";

    /// <summary>Paylasilan sirla dogrulanan idempotent webhook tetikleme rotasidir.</summary>
    public const string Webhook = "webhook";

    /// <summary>Pending kosumu claim eden alt rotadir.</summary>
    public const string Start = "{id:guid}/start";

    /// <summary>Kosumun terminal sonucunu yazan alt rotadir.</summary>
    public const string Terminal = "{id:guid}/terminal";

    /// <summary>Kimlige gore bulgulu sonuc rotasidir.</summary>
    public const string ResultById = "results/{id:guid}";

    /// <summary>Kimlige gore bulgulu ve teshisli kosum raporu rotasidir.</summary>
    public const string ReportById = "{id:guid}/report";

    /// <summary>Kuru kosum celiski bildirimini veren rotadir.</summary>
    public const string DryRunContradictionById = "{id:guid}/dry-run-contradiction";

    /// <summary>Terminal sonucun ihracat artefakt baglarini veren rotadir.</summary>
    public const string ResultArtifactsById = "results/{id:guid}/artifacts";

    /// <summary>Kosumu standart rapor formatlarina ihrac eden rotadir.</summary>
    public const string Export = "{id:guid}/export";

    /// <summary>Running kosuma kooperatif iptal talebi yazan rotadir.</summary>
    public const string Cancel = "{id:guid}/cancel";

    /// <summary>Terminal sonuc artefakt govdesini formatla okuyan rotadir.</summary>
    public const string ResultArtifactContent = "results/{id:guid}/artifacts/{format}";

    /// <summary>Kosumun HAR artefakt govdesini okuyan rotadir.</summary>
    public const string HarContent = "{id:guid}/har";

    /// <summary>Test kosumu endpoint'lerinin Swagger grup adidir.</summary>
    public const string SwaggerGroupName = "test-module-runs";
}

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

    /// <summary>Pending kosumu claim eden alt rotadir.</summary>
    public const string Start = "{id:guid}/start";

    /// <summary>Kosumun terminal sonucunu yazan alt rotadir.</summary>
    public const string Terminal = "{id:guid}/terminal";

    /// <summary>Kimlige gore bulgulu sonuc rotasidir.</summary>
    public const string ResultById = "results/{id:guid}";

    /// <summary>Test kosumu endpoint'lerinin Swagger grup adidir.</summary>
    public const string SwaggerGroupName = "test-module-runs";
}

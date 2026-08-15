namespace Ptn.TestModule.Constants.Catalog;

// islevi: Senaryo katalogu HTTP endpoint adreslerini ve Swagger grubunu tek sahipte tanimlar.
// sistemdeki gorevi: CRUD ve yayin use-case rotalarini controller metinlerinden ayirir.
/// <summary>Test senaryosu HTTP rota sabitlerini tasir.</summary>
public static class TestScenarioRoutes
{
    /// <summary>Senaryo katalogu koleksiyonunun kok rotasidir.</summary>
    public const string Root = "api/test-module/scenarios";

    /// <summary>Kimlige gore tek senaryo rotasidir.</summary>
    public const string ById = "{id:guid}";

    /// <summary>Senaryoyu insan onayina sunan alt rotadir.</summary>
    public const string SubmitForApproval = "{id:guid}/submit-for-approval";

    /// <summary>Yayin kapilarini durum degistirmeden degerlendiren alt rotadir.</summary>
    public const string EvaluatePublication = "{id:guid}/evaluate-publication";

    /// <summary>Onayli senaryoyu yayinlayan alt rotadir.</summary>
    public const string Publish = "{id:guid}/publish";

    /// <summary>Yayinlanmis senaryoyu kullanimdan kaldiran alt rotadir.</summary>
    public const string Deprecate = "{id:guid}/deprecate";

    /// <summary>Senaryoyu sinirli sureyle karantinaya alan rotadir.</summary>
    public const string Quarantine = "{id:guid}/quarantine";

    /// <summary>Suresi dolmus karantinayi temizleyen rotadir.</summary>
    public const string ReleaseQuarantine = "{id:guid}/quarantine/release";

    /// <summary>Senaryo katalogu endpoint'lerinin Swagger grup adidir.</summary>
    public const string SwaggerGroupName = "test-module-scenarios";
}

namespace Ptn.TestModule.Constants.Authoring;

// islevi: Yazarlik oturumu HTTP adreslerini ve Swagger grubunu tanimlar.
// sistemdeki gorevi: Dort public action'in route metinlerini Controller disinda sahiplenir.
public static class AuthoringSessionRoutes
{
    public const string Root = "api/test-module/authoring/sessions";
    public const string ById = "{id:guid}";
    public const string Answer = "{id:guid}/answer";
    public const string Step = "{id:guid}/step";
    public const string DatabaseStep = "{id:guid}/database-step";
    public const string SwaggerGroupName = "test-module-authoring";
}

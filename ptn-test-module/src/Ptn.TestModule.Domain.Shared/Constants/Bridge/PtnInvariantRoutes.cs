namespace Ptn.TestModule.Constants.Bridge;

// islevi: Is degismezi degerlendirici ucunun kararli rota ve Swagger grup adini tanimlar.
// sistemdeki gorevi: Rota metninin controller icinde inline string olarak dagilmasini engeller.
public static class PtnInvariantRoutes
{
    public const string Root = "api/test-module/invariants";
    public const string Check = "check";
    public const string SwaggerGroupName = "test-module-bridge";
}

namespace Ptn.TestModule.Constants.Bridge;

// islevi: Ortak diagnosis DTO'sunun kaynak-ozgul FluentValidation kural setlerini adlandirir.
// sistemdeki gorevi: AppService ile validator arasindaki kararli kural seti sozlesmesini sahiplenir.
public static class PtnBridgeValidationRuleSets
{
    public const string Api = nameof(Api);
    public const string Database = nameof(Database);
}

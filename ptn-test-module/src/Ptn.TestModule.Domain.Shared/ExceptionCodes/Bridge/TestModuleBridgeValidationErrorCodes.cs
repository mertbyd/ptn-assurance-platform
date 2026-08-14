namespace Ptn.TestModule.ExceptionCodes.Bridge;

// islevi: Bridge Application.Contracts girdilerinin kararli validation hata kodlarini tanimlar.
// sistemdeki gorevi: Dogrulama sonucunu mesaj metni ve validator sinifindan bagimsizlastirir.
public static class TestModuleBridgeValidationErrorCodes
{
    public const string ConnectionIdRequired = "TestModule.Bridge.Validation:ConnectionIdRequired";
    public const string SnapshotIdRequired = "TestModule.Bridge.Validation:SnapshotIdRequired";
    public const string SchemaNameRequired = "TestModule.Bridge.Validation:SchemaNameRequired";
    public const string TableNameRequired = "TestModule.Bridge.Validation:TableNameRequired";
    public const string ColumnNameRequired = "TestModule.Bridge.Validation:ColumnNameRequired";
    public const string MatcherKindRequired = "TestModule.Bridge.Validation:MatcherKindRequired";
    public const string CardinalityKindRequired = "TestModule.Bridge.Validation:CardinalityKindRequired";
    public const string LocationRequired = "TestModule.Bridge.Validation:LocationRequired";
    public const string MethodRequired = "TestModule.Bridge.Validation:MethodRequired";
    public const string PathRequired = "TestModule.Bridge.Validation:PathRequired";
    public const string AssertionPathRequired = "TestModule.Bridge.Validation:AssertionPathRequired";
    public const string StatusCodeInvalid = "TestModule.Bridge.Validation:StatusCodeInvalid";
    public const string TimeoutInvalid = "TestModule.Bridge.Validation:TimeoutInvalid";
    public const string PollIntervalInvalid = "TestModule.Bridge.Validation:PollIntervalInvalid";
    public const string ProjectionRowLimitInvalid = "TestModule.Bridge.Validation:ProjectionRowLimitInvalid";
    public const string RequestRequired = "TestModule.Bridge.Validation:RequestRequired";
    public const string BatchRequired = "TestModule.Bridge.Validation:BatchRequired";
    public const string ProfileKeyRequired = "TestModule.Bridge.Validation:ProfileKeyRequired";
    public const string OperationReferenceRequired = "TestModule.Bridge.Validation:OperationReferenceRequired";
    public const string AssertionReferenceRequired = "TestModule.Bridge.Validation:AssertionReferenceRequired";
    public const string ResponseFormatInvalid = "TestModule.Bridge.Validation:ResponseFormatInvalid";
    public const string ConceptCodeInvalid = "TestModule.Bridge.Validation:ConceptCodeInvalid";
}

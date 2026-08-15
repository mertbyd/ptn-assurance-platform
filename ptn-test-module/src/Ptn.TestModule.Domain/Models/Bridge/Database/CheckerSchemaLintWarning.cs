namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Database checker sema lint DTO'sunun kaynak alan seklini tasir.
// sistemdeki gorevi: Checker kodunu Bridge sozlugune cevirme kararini Manager'a birakir.
public sealed class CheckerSchemaLintWarning
{
    public string WarningCode { get; set; } = string.Empty;
    public string? ColumnName { get; set; }
}

using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Tablo yazarligini etkileyen sema lint uyarilarini kapali Bridge sozlugunde tanimlar.
// sistemdeki gorevi: Yayin kapisini checker sabitlerinden ve mesaj metninden ayirir.
public static class PtnSchemaLintWarningCodes
{
    public const string MissingPrimaryKey = nameof(MissingPrimaryKey);
    public const string MissingUniqueKey = nameof(MissingUniqueKey);
    public const string GeneratedColumn = nameof(GeneratedColumn);

    public static IReadOnlyCollection<string> All { get; } =
        [MissingPrimaryKey, MissingUniqueKey, GeneratedColumn];
}

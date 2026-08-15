using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Iki checker'in assertion ve uygunluk sonuclarini tek PascalCase sozlukte toplar.
// sistemdeki gorevi: Passed/passed gibi casing farklarinin ajan yuzeyine sizmasini engeller.
public static class PtnOutcomeCodes
{
    public const string Passed = nameof(Passed);
    public const string StatusCodeUndocumented = nameof(StatusCodeUndocumented);
    public const string MediaTypeUndocumented = nameof(MediaTypeUndocumented);
    public const string ResponseSchemaViolation = nameof(ResponseSchemaViolation);
    public const string RequiredHeaderMissing = nameof(RequiredHeaderMissing);
    public const string UndocumentedProperty = nameof(UndocumentedProperty);
    public const string ServerError = nameof(ServerError);
    public const string OperationNotResolved = nameof(OperationNotResolved);
    public const string SnapshotNotFound = nameof(SnapshotNotFound);
    public const string PolicySuppressed = nameof(PolicySuppressed);
    public const string SchemaNotResolved = nameof(SchemaNotResolved);
    public const string RowNotFound = nameof(RowNotFound);
    public const string ValueMismatch = nameof(ValueMismatch);
    public const string CardinalityMismatch = nameof(CardinalityMismatch);
    public const string TimedOut = nameof(TimedOut);
    public const string KeyNotUnique = nameof(KeyNotUnique);
    public const string TableNotFound = nameof(TableNotFound);
    public const string ColumnNotFound = nameof(ColumnNotFound);
    public const string Derivable = nameof(Derivable);
    public const string AssertionNotInContract = nameof(AssertionNotInContract);
    public const string DerivableButOptional = nameof(DerivableButOptional);
    public const string Unavailable = nameof(Unavailable);
    public const string MatcherTypeMismatch = nameof(MatcherTypeMismatch);

    public static IReadOnlyCollection<string> All { get; } =
    [
        Passed, StatusCodeUndocumented, MediaTypeUndocumented, ResponseSchemaViolation,
        RequiredHeaderMissing, UndocumentedProperty, ServerError, OperationNotResolved,
        SnapshotNotFound, PolicySuppressed, SchemaNotResolved, RowNotFound, ValueMismatch,
        CardinalityMismatch, TimedOut, KeyNotUnique, TableNotFound, ColumnNotFound,
        Derivable, AssertionNotInContract, DerivableButOptional, Unavailable,
        MatcherTypeMismatch
    ];
}

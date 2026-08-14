using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Iki checker'in hipotez kimliklerini anlam-temelli tek gramerde tanimlar.
// sistemdeki gorevi: H-CD-01 ve RowNeverCreated gibi kaynak gramerlerini ajan raporundan gizler.
public static class PtnHypothesisCodes
{
    public const string ResponseSchemaChanged = nameof(ResponseSchemaChanged);
    public const string RequiredRequestFieldCreated = nameof(RequiredRequestFieldCreated);
    public const string EnumValueRemoved = nameof(EnumValueRemoved);
    public const string EndpointRemovedOrMoved = nameof(EndpointRemovedOrMoved);
    public const string SuccessStatusChanged = nameof(SuccessStatusChanged);
    public const string MediaTypeRemoved = nameof(MediaTypeRemoved);
    public const string PropertyBecameOptional = nameof(PropertyBecameOptional);
    public const string ResourceNeverCreated = nameof(ResourceNeverCreated);
    public const string ResourceCreatedLate = nameof(ResourceCreatedLate);
    public const string AuthenticationMissing = nameof(AuthenticationMissing);
    public const string TokenExpired = nameof(TokenExpired);
    public const string InsufficientScope = nameof(InsufficientScope);
    public const string PathNotDeployed = nameof(PathNotDeployed);
    public const string MethodNotSupported = nameof(MethodNotSupported);
    public const string SnapshotVersionMismatch = nameof(SnapshotVersionMismatch);
    public const string AssertionValueDiffers = nameof(AssertionValueDiffers);
    public const string AssertionRequiredFieldMissing = nameof(AssertionRequiredFieldMissing);
    public const string AssertionOutsideContract = nameof(AssertionOutsideContract);
    public const string VolatileLiteralAssertion = nameof(VolatileLiteralAssertion);
    public const string RowInAnotherScope = nameof(RowInAnotherScope);
    public const string ExpectedColumnMissing = nameof(ExpectedColumnMissing);
    public const string ForeignKeyParentMissing = nameof(ForeignKeyParentMissing);
    public const string ConstraintNotValidated = nameof(ConstraintNotValidated);
    public const string UniqueDuplicateExists = nameof(UniqueDuplicateExists);
    public const string GeneratedColumnWrite = nameof(GeneratedColumnWrite);
    public const string ServerSettingMismatch = nameof(ServerSettingMismatch);

    public static IReadOnlyCollection<string> All { get; } =
    [
        ResponseSchemaChanged, RequiredRequestFieldCreated, EnumValueRemoved,
        EndpointRemovedOrMoved, SuccessStatusChanged, MediaTypeRemoved, PropertyBecameOptional,
        ResourceNeverCreated, ResourceCreatedLate, AuthenticationMissing, TokenExpired,
        InsufficientScope, PathNotDeployed, MethodNotSupported, SnapshotVersionMismatch,
        AssertionValueDiffers, AssertionRequiredFieldMissing, AssertionOutsideContract,
        VolatileLiteralAssertion, RowInAnotherScope, ExpectedColumnMissing,
        ForeignKeyParentMissing, ConstraintNotValidated, UniqueDuplicateExists,
        GeneratedColumnWrite, ServerSettingMismatch
    ];
}

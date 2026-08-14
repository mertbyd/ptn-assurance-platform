using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Kanit yolu adimlarinin cagirabildigi deterministik port kaynaklarini tanimlar.
// sistemdeki gorevi: Profil dosyasinin serbest servis veya kod calistirmasini engeller.
public static class PtnEvidenceSourceCodes
{
    public const string ApiFailureIdentity = "api.failureIdentity";
    public const string ApiOperationBinding = "api.operationBinding";
    public const string ApiRequestExample = "api.requestExample";
    public const string ApiAssertionDerivability = "api.assertionDerivability";
    public const string DatabaseProjection = "db.projection";
    public const string DatabaseTableDescription = "db.tableDescription";

    public static IReadOnlyCollection<string> All { get; } =
    [
        ApiFailureIdentity, ApiOperationBinding, ApiRequestExample,
        ApiAssertionDerivability, DatabaseProjection, DatabaseTableDescription
    ];
}

namespace Ptn.ApiContractChecker.ExceptionCodes.Snapshots;

// islevi: Spec ayristirma ve anlik goruntu alma kurallarinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Guvenilmeyen spec govdesinin reddedilme sebebini katmanlardan bagimsiz sozlesmeye baglar.
public static class SpecSnapshotExceptionCodes
{
    public const string ParseFailed = "ApiContractChecker:SpecSnapshot:ParseFailed";
    public const string DocumentInvalid = "ApiContractChecker:SpecSnapshot:DocumentInvalid";

    public static class Validation
    {
        public const string OperationMethodCodeInvalid =
            "ApiContractChecker.SpecSnapshot:Validation:OperationMethodCodeInvalid";

        public const string OperationPathPrefixMaxLength =
            "ApiContractChecker.SpecSnapshot:Validation:OperationPathPrefixMaxLength";
    }
}

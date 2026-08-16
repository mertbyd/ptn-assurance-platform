namespace Ptn.ApiContractChecker.Constants.Diagnosis;

// islevi: Checker-bagimsiz sonraki kontrol capability ve operation kodlarini tanimlar.
// sistemdeki gorevi: API checker'in database checker'a bagimlilik almadan tipli kontrol onermesini saglar.
public static class SuggestedCheckCodes
{
    public const string DatabaseCapability = "database";
    public const string AssertRowOperation = "assert.row";
    public const string OperationIdArgument = "operationId";
    public const string ResourceUrlArgument = "resourceUrl";
}

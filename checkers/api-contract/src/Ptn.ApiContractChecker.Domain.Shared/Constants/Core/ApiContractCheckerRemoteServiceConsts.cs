namespace Ptn.ApiContractChecker;

// islevi: Modülün uzak servis kimliğini ve kısa modül adını tek noktada tanımlar.
// sistemdeki gorevi: HTTP istemcileri ile modül kayıtlarının aynı servis sözleşmesini kullanmasını sağlar.
public static class ApiContractCheckerRemoteServiceConsts
{
    public const string RemoteServiceName = Constants.ApiContractCheckerApplicationConstants.RemoteServiceName;

    public const string ModuleName = Constants.ApiContractCheckerApplicationConstants.RemoteServiceModuleName;
}

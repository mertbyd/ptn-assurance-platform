namespace Ptn.ApiContractChecker.Constants;

// islevi: Uygulamanin katmanlar arasi kararli adlarini ve dosya sistemi kimliklerini tanimlar.
// sistemdeki gorevi: Host, remote service ve gelistirme zamani physical-file akislarinin ayni uygulama sozlesmesini kullanmasini saglar.
public static class ApiContractCheckerApplicationConstants
{
    public const string ApplicationName = "ApiContractChecker";
    public const string RemoteServiceName = ApplicationName;
    public const string RemoteServiceModuleName = "apiContractChecker";
    public const string DomainSharedProjectName = "Ptn.ApiContractChecker.Domain.Shared";
    public const string DomainProjectName = "Ptn.ApiContractChecker.Domain";
    public const string ApplicationContractsProjectName = "Ptn.ApiContractChecker.Application.Contracts";
    public const string ApplicationProjectName = "Ptn.ApiContractChecker.Application";
    public const string HttpApiXmlDocumentationFileName = "Ptn.ApiContractChecker.HttpApi.xml";
    public const string ApplicationContractsXmlDocumentationFileName = "Ptn.ApiContractChecker.Application.Contracts.xml";
    public const string SourceDirectoryName = "src";
    public const string ParentDirectorySegment = "..";
}

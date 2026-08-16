namespace Ptn.ApiContractChecker.ExceptionCodes.Sources;

// islevi: SpecSource aggregate ve canli spec cekme kurallarinin kararli hata kodlarini tanimlar.
// sistemdeki gorevi: Kaynak yonetimiyle guvenilmeyen HTTP icerigi hatalarini katmanlardan bagimsiz sozlesmeye baglar.
public static class SpecSourceExceptionCodes
{
    public const string NameAlreadyExists = "ApiContractChecker:SpecSource:NameAlreadyExists";
    public const string DocumentNameAlreadyExists = "ApiContractChecker:SpecSource:DocumentNameAlreadyExists";
    public const string ActiveDocumentNotFound = "ApiContractChecker:SpecSource:ActiveDocumentNotFound";
    public const string FetchHttpStatusRejected = "ApiContractChecker:SpecSource:FetchHttpStatusRejected";
    public const string FetchContentTypeRejected = "ApiContractChecker:SpecSource:FetchContentTypeRejected";
    public const string FetchContentTooLarge = "ApiContractChecker:SpecSource:FetchContentTooLarge";
    public const string FetchContentEmpty = "ApiContractChecker:SpecSource:FetchContentEmpty";
    public const string FetchNetworkFailed = "ApiContractChecker:SpecSource:FetchNetworkFailed";
    public const string FetchTimedOut = "ApiContractChecker:SpecSource:FetchTimedOut";

    public static class Validation
    {
        public const string NameRequired = "ApiContractChecker.SpecSource:Validation:NameRequired";
        public const string NameMaxLength = "ApiContractChecker.SpecSource:Validation:NameMaxLength";
        public const string BaseUrlRequired = "ApiContractChecker.SpecSource:Validation:BaseUrlRequired";
        public const string BaseUrlInvalid = "ApiContractChecker.SpecSource:Validation:BaseUrlInvalid";
        public const string BaseUrlMaxLength = "ApiContractChecker.SpecSource:Validation:BaseUrlMaxLength";
        public const string DocumentsRequired = "ApiContractChecker.SpecSource:Validation:DocumentsRequired";
        public const string DocumentIdMustBeEmpty = "ApiContractChecker.SpecSource:Validation:DocumentIdMustBeEmpty";
        public const string DocumentNameRequired = "ApiContractChecker.SpecSource:Validation:DocumentNameRequired";
        public const string DocumentNameMaxLength = "ApiContractChecker.SpecSource:Validation:DocumentNameMaxLength";
        public const string DocumentNameDuplicate = "ApiContractChecker.SpecSource:Validation:DocumentNameDuplicate";
        public const string DocumentPathRequired = "ApiContractChecker.SpecSource:Validation:DocumentPathRequired";
        public const string DocumentPathInvalid = "ApiContractChecker.SpecSource:Validation:DocumentPathInvalid";
        public const string DocumentPathMaxLength = "ApiContractChecker.SpecSource:Validation:DocumentPathMaxLength";
        public const string HeaderNameRequired = "ApiContractChecker.SpecSource:Validation:HeaderNameRequired";
        public const string HeaderNameInvalid = "ApiContractChecker.SpecSource:Validation:HeaderNameInvalid";
        public const string HeaderNameMaxLength = "ApiContractChecker.SpecSource:Validation:HeaderNameMaxLength";
        public const string HeaderValueRequired = "ApiContractChecker.SpecSource:Validation:HeaderValueRequired";
        public const string HeaderValueInvalid = "ApiContractChecker.SpecSource:Validation:HeaderValueInvalid";
        public const string HeaderValueMaxLength = "ApiContractChecker.SpecSource:Validation:HeaderValueMaxLength";
        public const string MonitoringIntervalRequired = "ApiContractChecker.SpecSource:Validation:MonitoringIntervalRequired";
        public const string MonitoringIntervalOutOfRange = "ApiContractChecker.SpecSource:Validation:MonitoringIntervalOutOfRange";
    }
}

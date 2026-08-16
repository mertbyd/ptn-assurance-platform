namespace Ptn.ApiContractChecker.Constants.Runs;

// islevi: Gecici contract-check kapsam kurallarinin tur ve hedef kodlarini tanimlar.
// sistemdeki gorevi: Istek, job payload'i ve saf comparison filtresinin tabloya yazilmayan ayni kararli dili kullanmasini saglar.
public static class ContractCheckScopeCodes
{
    // islevi: Kapsam kuralinin dahil etme veya haric tutma kararini tasir.
    // sistemdeki gorevi: Enum ya da lookup kaliciligi kurmadan request-scoped kurallara kararli kod verir.
    public static class Kinds
    {
        public const string Include = "include";
        public const string Exclude = "exclude";

        public static IReadOnlyCollection<string> All { get; } = [Include, Exclude];
    }

    // islevi: Kapsam deseninin hangi OpenAPI kimligine uygulanacagini tanimlar.
    // sistemdeki gorevi: Path, tag, operationId ve schema filtrelerini tek gecici kural sozlesmesinde birlestirir.
    public static class Targets
    {
        public const string Path = "path";
        public const string Tag = "tag";
        public const string OperationId = "operation-id";
        public const string Schema = "schema";

        public static IReadOnlyCollection<string> All { get; } = [Path, Tag, OperationId, Schema];
    }

    public const string InternalExtensionName = "x-internal";
}

namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Bir snapshot operasyonunu operationId veya method/path ile secmek icin gereken degerleri tasir.
// sistemdeki gorevi: Request ornegi ve bagimlilik onerisi manager'larina ortak tahminsiz secim girdisi verir.
public sealed class OperationSelectionRequest
{
    public string? OperationId { get; }
    public string Method { get; }
    public string Path { get; }

    public OperationSelectionRequest(string? operationId, string method, string path)
    {
        OperationId = operationId;
        Method = method;
        Path = path;
    }
}

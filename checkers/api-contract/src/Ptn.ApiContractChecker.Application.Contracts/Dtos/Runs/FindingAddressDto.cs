namespace Ptn.ApiContractChecker.Dtos.Runs;

// islevi: Bir contract bulgusunun endpoint, schema veya alan konumunu API cevabinda tasir.
// sistemdeki gorevi: Frontend'in bulguyu sozlesmedeki kesin adrese baglamasini saglar.
public class FindingAddressDto
{
    public string? OperationId { get; set; }
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public string? SchemaName { get; set; }
    public string? PropertyPath { get; set; }
    public string? ParameterName { get; set; }
    public string? ResponseStatus { get; set; }
    public string? MediaType { get; set; }
}

namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Owned JSON bulgu adresini repository projeksiyonunda tasir.
// sistemdeki gorevi: EF projection istisnasini Mapperly'nin DTO adres eslemesine temiz bir kaynak olarak sunar.
public class FindingAddressReadModel
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

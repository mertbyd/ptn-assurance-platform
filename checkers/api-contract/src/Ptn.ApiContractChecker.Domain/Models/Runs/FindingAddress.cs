using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Volo.Abp;

namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Bir contract farkinin endpoint, schema veya alan uzerindeki tam adresini tasir.
// sistemdeki gorevi: Owned JSON bulgusunu frontend'in gosterebilecegi konuma baglar ve tamamen bos adresi reddeder.
public class FindingAddress
{
    public string? OperationId { get; private set; }
    public string? HttpMethod { get; private set; }
    public string? Path { get; private set; }
    public string? SchemaName { get; private set; }
    public string? PropertyPath { get; private set; }
    public string? ParameterName { get; private set; }
    public string? ResponseStatus { get; private set; }
    public string? MediaType { get; private set; }

    // EF Core JSON materializasyonu icin parametresiz ctor.
    protected FindingAddress()
    {
    }

    // En az bir adres bileseni bulunan bulgu konumunu kurar.
    public FindingAddress(
        string? operationId = null,
        string? httpMethod = null,
        string? path = null,
        string? schemaName = null,
        string? propertyPath = null,
        string? parameterName = null,
        string? responseStatus = null,
        string? mediaType = null)
    {
        OperationId = Normalize(operationId);
        HttpMethod = Normalize(httpMethod)?.ToUpperInvariant();
        Path = Normalize(path);
        SchemaName = Normalize(schemaName);
        PropertyPath = Normalize(propertyPath);
        ParameterName = Normalize(parameterName);
        ResponseStatus = Normalize(responseStatus);
        MediaType = Normalize(mediaType)?.ToLowerInvariant();

        if (!HasAddressComponent())
        {
            throw new BusinessException(ContractCheckRunExceptionCodes.FindingAddressRequired);
        }
    }

    // Adresin en az bir anlamli bilesen tasiyip tasimadigini belirler.
    private bool HasAddressComponent()
    {
        return OperationId is not null ||
               HttpMethod is not null ||
               Path is not null ||
               SchemaName is not null ||
               PropertyPath is not null ||
               ParameterName is not null ||
               ResponseStatus is not null ||
               MediaType is not null;
    }

    // Bos adres parcaciklarini null'a indirger, dolu parcaciklari trimler.
    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}

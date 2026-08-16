using System.Text.Json;
using Ptn.ApiContractChecker.Application.Mappers.Sources;
using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.Entities.Sources;
using Ptn.ApiContractChecker.Managers.Sources;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Sources;

// islevi: SpecSource response sozlesmesi ve Mapperly cikisinda secret sizintisi olmadigini kanitlar.
// sistemdeki gorevi: Header degeri veya Vault yolunun yeni bir DTO alani ya da otomatik mapping ile geri gelmesini engeller.
public class SpecSourceSecretContract_Tests
{
    // Secret tasiyan entity'nin response JSON'unda credential ve Vault yolunun bulunmadigini kanitlar.
    [Fact]
    public void Response_Mapping_Should_Not_Expose_Secret_Or_Vault_Path()
    {
        const string secretPath = "tenant/sources/source-id";
        const string secretValue = "Bearer highly-sensitive-value";
        var source = new SpecSource(Guid.NewGuid(), "orders", "https://orders.test", secretPath, Guid.NewGuid());
        new SpecSourceManager(null!, null!)
            .AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");

        var dto = new SpecSourceMapper().MapToDto(source);
        var json = JsonSerializer.Serialize(dto);

        json.ShouldNotContain(secretPath);
        json.ShouldNotContain(secretValue);
        json.ShouldNotContain("VaultSecretPath");
        json.ShouldNotContain("HeaderName");
        json.ShouldNotContain("HeaderValue");
    }

    // Tum response DTO tiplerinin yasakli secret alan adlarindan arinmis oldugunu kanitlar.
    [Fact]
    public void Response_Dtos_Should_Not_Declare_Secret_Properties()
    {
        var responseTypes = new[]
        {
            typeof(SpecSourceDto),
            typeof(SpecDocumentDto),
            typeof(SpecSourceReachabilityDto)
        };

        var propertyNames = responseTypes
            .SelectMany(type => type.GetProperties())
            .Select(property => property.Name)
            .ToList();

        propertyNames.ShouldNotContain(name => name.Contains("Secret", StringComparison.OrdinalIgnoreCase));
        propertyNames.ShouldNotContain(name => name.Contains("Vault", StringComparison.OrdinalIgnoreCase));
        propertyNames.ShouldNotContain(name => name.Contains("Header", StringComparison.OrdinalIgnoreCase));
        propertyNames.ShouldNotContain(name => name.Contains("Credential", StringComparison.OrdinalIgnoreCase));
    }
}

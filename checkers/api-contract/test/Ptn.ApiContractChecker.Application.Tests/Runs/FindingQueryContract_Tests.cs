using Ptn.ApiContractChecker.Application.Mappers.Runs;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;
using Ptn.ApiContractChecker.FluentValidation.Runs;
using Ptn.ApiContractChecker.Models.Runs;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Runs;

// islevi: Finding query validator'i ve Mapperly typed address cikisini public paket sinirinda dogrular.
// sistemdeki gorevi: SinceRunId, bounded SHA-256 kumesi ve sekiz bilesenli adresin contract regresyonunu engeller.
public class FindingQueryContract_Tests
{
    private const string Fingerprint = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    [Fact]
    public void Validator_Should_Reject_Empty_Reference_Invalid_Duplicate_And_Unbounded_Fingerprints()
    {
        var validator = new GetContractCheckFindingsInputValidator();
        validator.Validate(new GetContractCheckFindingsInput { SinceRunId = Guid.Empty }).Errors
            .ShouldContain(error => error.ErrorMessage == ContractCheckRunExceptionCodes.Validation.FindingSinceRunIdInvalid);
        validator.Validate(new GetContractCheckFindingsInput { Fingerprints = [" "] }).Errors
            .ShouldContain(error => error.ErrorMessage == ContractCheckRunExceptionCodes.Validation.FindingFingerprintInvalid);
        validator.Validate(new GetContractCheckFindingsInput { Fingerprints = [Fingerprint, Fingerprint.ToLowerInvariant()] }).Errors
            .ShouldContain(error => error.ErrorMessage == ContractCheckRunExceptionCodes.Validation.FindingFingerprintDuplicate);
        validator.Validate(new GetContractCheckFindingsInput
        {
            Fingerprints = Enumerable.Repeat(Fingerprint, ContractCheckRunConsts.MaxFindingFingerprintFilterCount + 1).ToList()
        }).Errors.ShouldContain(error =>
            error.ErrorMessage == ContractCheckRunExceptionCodes.Validation.FindingFingerprintLimitExceeded);
    }

    [Fact]
    public void Mapperly_Should_Map_All_Address_Components()
    {
        var page = new FindingPageResultModel { TotalCount = 1 };
        page.Items.Add(new FindingReadModel
        {
            KindCode = DifferenceKindCodes.EndpointRemoved,
            SeverityCode = DifferenceSeverityCodes.Breaking,
            DirectionCode = DifferenceDirectionCodes.Endpoint,
            Fingerprint = Fingerprint,
            ChangeStateCode = "New",
            Address = new FindingAddressReadModel
            {
                OperationId = "getOrder",
                HttpMethod = "GET",
                Path = "/orders/{id}",
                SchemaName = "Order",
                PropertyPath = "/id",
                ParameterName = "id",
                ResponseStatus = "200",
                MediaType = "application/json"
            }
        });

        var address = new ContractCheckRunMapper().MapToFindingPageDto(page).Items.Single().Address;
        address.OperationId.ShouldBe("getOrder");
        address.HttpMethod.ShouldBe("GET");
        address.Path.ShouldBe("/orders/{id}");
        address.SchemaName.ShouldBe("Order");
        address.PropertyPath.ShouldBe("/id");
        address.ParameterName.ShouldBe("id");
        address.ResponseStatus.ShouldBe("200");
        address.MediaType.ShouldBe("application/json");
    }
}

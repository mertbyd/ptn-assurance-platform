using Ptn.DatabaseChecker.Application.Mappers.Runs;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Dtos.Runs;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.FluentValidation.Runs;
using Ptn.DatabaseChecker.Models.Runs;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Runs;

// islevi: Public bulgu filtrelerinin FluentValidation ve Mapperly katmanlarinda kayipsiz tasindigini dogrular.
// sistemdeki gorevi: Test Module'un SinceRunId, bounded fingerprint ve typed address contract'ini paket sinirinda pinler.
public class FindingQueryContract_Tests
{
    private const string FingerprintA = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    private const string FingerprintB = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";

    [Fact]
    public void Validator_Should_Enforce_Reference_And_Bounded_Canonical_Fingerprint_Set()
    {
        var validator = new FindingQueryInputValidator();
        validator.Validate(new FindingQueryInput { SinceRunId = Guid.Empty }).Errors
            .ShouldContain(error => error.ErrorMessage == ComparisonRunExceptionCodes.Validation.FindingSinceRunIdInvalid);
        validator.Validate(new FindingQueryInput { Fingerprints = [" "] }).Errors
            .ShouldContain(error => error.ErrorMessage == ComparisonRunExceptionCodes.Validation.FindingFingerprintInvalid);
        validator.Validate(new FindingQueryInput { Fingerprints = ["not-sha256"] }).Errors
            .ShouldContain(error => error.ErrorMessage == ComparisonRunExceptionCodes.Validation.FindingFingerprintInvalid);
        validator.Validate(new FindingQueryInput { Fingerprints = [FingerprintA, FingerprintA.ToLowerInvariant()] }).Errors
            .ShouldContain(error => error.ErrorMessage == ComparisonRunExceptionCodes.Validation.FindingFingerprintDuplicate);
        validator.Validate(new FindingQueryInput
        {
            Fingerprints = Enumerable.Repeat(FingerprintA, ComparisonRunConsts.MaxFindingFingerprintFilterCount + 1).ToList()
        }).Errors.ShouldContain(error =>
            error.ErrorMessage == ComparisonRunExceptionCodes.Validation.FindingFingerprintLimitExceeded);
    }

    [Fact]
    public void Mapperly_Should_Carry_Query_Filters_And_Typed_Address()
    {
        var sinceRunId = Guid.NewGuid();
        var mapper = new ComparisonRunMapper();
        var query = mapper.MapToQueryModel(new FindingQueryInput
        {
            SinceRunId = sinceRunId,
            Fingerprints = [FingerprintA, FingerprintB]
        });
        var dto = mapper.MapToFindingDtos([new FindingReadModel
        {
            Fingerprint = FingerprintA,
            SeverityCode = DifferenceSeverityCodes.Breaking,
            KindCode = DifferenceKindCodes.Modified,
            ObjectTypeCode = SchemaObjectTypeCodes.Column,
            ObjectName = "orders",
            Address = new FindingAddressReadModel
            {
                SourceEngineCode = DatabaseEngineCodes.PostgreSql,
                TargetEngineCode = DatabaseEngineCodes.SqlServer,
                SchemaName = "public",
                ObjectTypeCode = SchemaObjectTypeCodes.Column,
                ObjectName = "orders",
                ChildName = "amount"
            }
        }]).Single();

        query.SinceRunId.ShouldBe(sinceRunId);
        query.Fingerprints.ShouldBe([FingerprintA, FingerprintB]);
        dto.Address.SourceEngineCode.ShouldBe(DatabaseEngineCodes.PostgreSql);
        dto.Address.TargetEngineCode.ShouldBe(DatabaseEngineCodes.SqlServer);
        dto.Address.SchemaName.ShouldBe("public");
        dto.Address.ObjectTypeCode.ShouldBe(SchemaObjectTypeCodes.Column);
        dto.Address.ObjectName.ShouldBe("orders");
        dto.Address.ChildName.ShouldBe("amount");
    }
}

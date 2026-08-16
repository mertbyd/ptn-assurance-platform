using FluentValidation.TestHelper;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.FluentValidation.Runs;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Runs;

// islevi: Contract-check tetikleme DTO'sunun snapshot ve gecici scope sinirlarini dogrular.
// sistemdeki gorevi: Hatali veya sinirsiz job payload'inin Pending run yazilmadan reddedildigini kontrat seviyesinde sabitler.
public class ContractCheckExecutionValidator_Tests
{
    private readonly ExecuteContractCheckDtoValidator _validator = new();

    // Iki snapshot ve kararli scope kodlariyla gecerli tetikleme istegini kabul eder.
    [Fact]
    public async Task Valid_Request_Should_Pass()
    {
        var result = await _validator.TestValidateAsync(new ExecuteContractCheckDto
        {
            BaseSnapshotId = Guid.NewGuid(),
            TargetSnapshotId = Guid.NewGuid(),
            ScopeRules =
            [
                new ContractCheckScopeRuleDto
                {
                    KindCode = ContractCheckScopeCodes.Kinds.Include,
                    TargetCode = ContractCheckScopeCodes.Targets.Path,
                    Pattern = "/api/*"
                }
            ]
        });

        result.IsValid.ShouldBeTrue();
    }

    // Bos snapshot kimliklerinin job kuyruguna girmeden reddedildigini kanitlar.
    [Fact]
    public async Task Empty_Snapshot_Ids_Should_Fail()
    {
        var result = await _validator.TestValidateAsync(new ExecuteContractCheckDto());

        result.ShouldHaveValidationErrorFor(input => input.BaseSnapshotId);
        result.ShouldHaveValidationErrorFor(input => input.TargetSnapshotId);
    }

    // Kararli katalog disindaki scope kodunun reddedildigini kanitlar.
    [Fact]
    public async Task Unknown_Scope_Code_Should_Fail()
    {
        var result = await _validator.TestValidateAsync(new ExecuteContractCheckDto
        {
            BaseSnapshotId = Guid.NewGuid(),
            TargetSnapshotId = Guid.NewGuid(),
            ScopeRules =
            [
                new ContractCheckScopeRuleDto
                {
                    KindCode = "unknown",
                    TargetCode = ContractCheckScopeCodes.Targets.Path,
                    Pattern = "/api/*"
                }
            ]
        });

        result.ShouldHaveValidationErrorFor("ScopeRules[0].KindCode");
    }
}

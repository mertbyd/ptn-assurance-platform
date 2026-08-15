using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Dtos.Runs;
using Ptn.TestModule.Mappers.Runs;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Mappers.Runs;

// islevi: Kosum Mapperly eslemelerinin create ve nested terminal alanlarini korudugunu dogrular.
// sistemdeki gorevi: Application katmaninda elle mapping veya sessiz alan kaybi olusmasini engeller.
/// <summary>TestRunMapper saf esleme testleridir.</summary>
public class TestRunMapperTests
{
    /// <summary>Create DTO alanlarinin domain modeline kayipsiz tasindigini dogrular.</summary>
    [Fact]
    public void Should_map_create_input_to_domain_model()
    {
        var mapper = new TestRunMapper();
        var input = new CreateTestRunDto
        {
            TestKey = "checkout",
            TriggerKindCode = "Manual",
            TriggerRef = "manual-1",
            IsDryRun = true
        };

        var model = mapper.Map(input);

        model.TestKey.ShouldBe(input.TestKey);
        model.TriggerKindCode.ShouldBe(input.TriggerKindCode);
        model.TriggerRef.ShouldBe(input.TriggerRef);
        model.IsDryRun.ShouldBeTrue();
    }

    /// <summary>Terminal DTO icindeki nested bulgunun domain modeline tasindigini dogrular.</summary>
    [Fact]
    public void Should_map_terminal_findings_to_domain_models()
    {
        var mapper = new TestRunMapper();
        var input = new WriteTestRunTerminalDto
        {
            OutcomeCode = "Failed",
            Findings =
            [
                new TestResultFindingInputDto
                {
                    Ordinal = 2,
                    SourceCheckerCode = TestSourceCheckerCodes.DatabaseComparison,
                    ComparisonKindCode = "RowSet",
                    Location = "public.orders",
                    Message = "Unexpected row"
                }
            ]
        };

        var model = mapper.Map(input);

        model.Findings.Count.ShouldBe(1);
        model.Findings.ShouldContain(finding =>
            finding.SourceCheckerCode == TestSourceCheckerCodes.DatabaseComparison &&
            finding.Location == "public.orders");
    }
}

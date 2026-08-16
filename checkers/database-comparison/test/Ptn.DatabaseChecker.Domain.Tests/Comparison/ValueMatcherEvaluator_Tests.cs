using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Managers.Assertions;
using Ptn.DatabaseChecker.Models.Assertions;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: Saf assertion matcher'inin sayisal normalizasyon ve regex emniyeti davranislarini dogrular.
// sistemdeki gorevi: Provider ve polling olmadan CanonicalDataTypeCodes semantiginin kararli regresyon kanitidir.
public class ValueMatcherEvaluator_Tests
{
    private readonly ValueMatcherEvaluator _evaluator = new();

    [Fact]
    public void WithinTolerance_Should_Normalize_Numeric_Scale()
    {
        var expectation = new ColumnExpectation
        {
            MatcherKindCode = MatcherKindCodes.WithinTolerance,
            ExpectedValue = "12.340",
            Tolerance = 0m
        };

        var matched = _evaluator.Evaluate(
            expectation,
            "12.34",
            CanonicalDataTypeCodes.Decimal,
            scale: 3,
            regexTimeoutMs: 200);

        matched.ShouldBeTrue();
    }

    [Fact]
    public void Pathological_Regex_Should_Fail_Without_Exception()
    {
        var expectation = new ColumnExpectation
        {
            MatcherKindCode = MatcherKindCodes.MatchesRegex,
            ExpectedValue = "^(a+)+$"
        };

        var matched = _evaluator.Evaluate(
            expectation,
            new string('a', 10000) + "!",
            CanonicalDataTypeCodes.String,
            scale: null,
            regexTimeoutMs: 1);

        matched.ShouldBeFalse();
    }
}

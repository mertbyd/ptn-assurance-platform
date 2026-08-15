using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Models.Bridge.Invariants;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: RESEARCH-0009 M-1..M-4 desenlerinin aritmetigini dogrular.
// sistemdeki gorevi: Degerlendiricinin alan bilgisi tasimadan dogru gecti/kaldi karari uretmesini garanti eder.
public class BusinessInvariantManagerTests
{
    private static readonly BusinessInvariantManager Manager = new();

    // M-1: korunan buyukluk esit kaldiginda degismez saglanir.
    [Fact]
    public void Should_pass_conservation_when_the_total_is_preserved()
    {
        var result = Evaluate(PtnInvariantPatternCodes.Conservation, left: 120m, right: 120m);

        result.Passed.ShouldBeTrue();
        result.ReasonCode.ShouldBe(PtnInvariantReasonCodes.Balanced);
    }

    // M-1: korunan buyukluk degistiginde degismez bozulur.
    [Fact]
    public void Should_fail_conservation_when_the_total_moves()
    {
        var result = Evaluate(PtnInvariantPatternCodes.Conservation, left: 120m, right: 119m);

        result.Passed.ShouldBeFalse();
        result.ReasonCode.ShouldBe(PtnInvariantReasonCodes.NotBalanced);
    }

    // M-2: stokSonra == stokOnce - 1 beklentisi delta -1 ile saglanir.
    [Fact]
    public void Should_pass_delta_when_the_exact_change_is_observed()
    {
        var result = Evaluate(PtnInvariantPatternCodes.Delta, left: 10m, right: 9m, delta: -1m);

        result.Passed.ShouldBeTrue();
        result.ReasonCode.ShouldBe(PtnInvariantReasonCodes.DeltaMatched);
    }

    // M-2: beklenenden farkli degisim delta desenini dusurur.
    [Fact]
    public void Should_fail_delta_when_the_change_differs()
    {
        var result = Evaluate(PtnInvariantPatternCodes.Delta, left: 10m, right: 8m, delta: -1m);

        result.Passed.ShouldBeFalse();
        result.ReasonCode.ShouldBe(PtnInvariantReasonCodes.DeltaMismatched);
    }

    // M-3: bagimsiz turetilmis iki gorunum ayni degeri verdiginde tutarlilik saglanir.
    [Fact]
    public void Should_pass_consistency_when_both_views_agree()
    {
        var result = Evaluate(PtnInvariantPatternCodes.Consistency, left: 42m, right: 42m);

        result.Passed.ShouldBeTrue();
        result.ReasonCode.ShouldBe(PtnInvariantReasonCodes.Consistent);
    }

    // M-3: gorunumler ayrildiginda tutarlilik bozulur.
    [Fact]
    public void Should_fail_consistency_when_the_views_diverge()
    {
        var result = Evaluate(PtnInvariantPatternCodes.Consistency, left: 42m, right: 41m);

        result.Passed.ShouldBeFalse();
        result.ReasonCode.ShouldBe(PtnInvariantReasonCodes.Inconsistent);
    }

    // M-4: yinelenme sayisi sifir oldugunda tekillik saglanir.
    [Fact]
    public void Should_pass_uniqueness_when_no_duplicate_is_observed()
    {
        var result = Evaluate(PtnInvariantPatternCodes.Uniqueness, left: 0m, right: 0m);

        result.Passed.ShouldBeTrue();
        result.ReasonCode.ShouldBe(PtnInvariantReasonCodes.Unique);
    }

    // M-4: yinelenme gozlendiginde tekillik bozulur.
    [Fact]
    public void Should_fail_uniqueness_when_a_duplicate_is_observed()
    {
        var result = Evaluate(PtnInvariantPatternCodes.Uniqueness, left: 2m, right: 0m);

        result.Passed.ShouldBeFalse();
        result.ReasonCode.ShouldBe(PtnInvariantReasonCodes.Duplicated);
    }

    // Ondalik olculer tam sayiya yuvarlanmadan karsilastirilmalidir.
    [Fact]
    public void Should_compare_decimal_measures_without_rounding()
    {
        var result = Evaluate(PtnInvariantPatternCodes.Delta, left: 10.25m, right: 10.00m, delta: -0.25m);

        result.Passed.ShouldBeTrue();
    }

    // Kapali kume disindaki desen kodu kodlu hata ile reddedilmelidir.
    [Fact]
    public void Should_reject_an_unknown_pattern_code()
    {
        var exception = Should.Throw<BusinessException>(() => Evaluate("Guess", left: 1m, right: 1m));

        exception.Code.ShouldBe(TestModuleBridgeErrorCodes.Validation.InvariantPatternInvalid);
    }

    // Tek girdi seklini testler arasinda paylasir.
    private static BusinessInvariantResult Evaluate(
        string patternCode,
        decimal left,
        decimal right,
        decimal delta = 0m)
    {
        return Manager.Evaluate(new BusinessInvariantRequest
        {
            PatternCode = patternCode,
            Left = left,
            Right = right,
            Delta = delta
        });
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Models.Assertions;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Assertions;

// islevi: Tek kolon beklentisini kanonik veri tipi semantigiyle yan etkisiz degerlendirir.
// sistemdeki gorevi: Matcher secimi, sayi/zaman normalizasyonu ve regex zaman siniri repository ve polling akisindan bagimsiz tek noktada kalir.
public class ValueMatcherEvaluator : ITransientDependency
{
    // islevi: Matcher kodunu yalnizca ilgili kucuk evaluator metoduna yonlendirir.
    public bool Evaluate(
        ColumnExpectation expectation,
        string? observedValue,
        string canonicalTypeCode,
        int? scale,
        int regexTimeoutMs)
        => expectation.MatcherKindCode switch
        {
            MatcherKindCodes.Equals => EvaluateEquals(expectation, observedValue, canonicalTypeCode, scale),
            MatcherKindCodes.NotEquals => EvaluateNotEquals(expectation, observedValue, canonicalTypeCode, scale),
            MatcherKindCodes.IsNull => EvaluateIsNull(observedValue),
            MatcherKindCodes.IsNotNull => EvaluateIsNotNull(observedValue),
            MatcherKindCodes.GreaterThan => EvaluateGreaterThan(expectation, observedValue, canonicalTypeCode, scale),
            MatcherKindCodes.GreaterThanOrEqual => EvaluateGreaterThanOrEqual(expectation, observedValue, canonicalTypeCode, scale),
            MatcherKindCodes.LessThan => EvaluateLessThan(expectation, observedValue, canonicalTypeCode, scale),
            MatcherKindCodes.LessThanOrEqual => EvaluateLessThanOrEqual(expectation, observedValue, canonicalTypeCode, scale),
            MatcherKindCodes.MatchesRegex => EvaluateMatchesRegex(expectation, observedValue, regexTimeoutMs),
            MatcherKindCodes.OneOf => EvaluateOneOf(expectation, observedValue, canonicalTypeCode, scale),
            MatcherKindCodes.WithinTolerance => EvaluateWithinTolerance(expectation, observedValue),
            _ => throw new BusinessException(AssertionExceptionCodes.InvalidMatcherKind)
        };

    // islevi: Tip-semantik karsilastirmasiyla esitligi degerlendirir.
    private static bool EvaluateEquals(ColumnExpectation expectation, string? observed, string typeCode, int? scale)
        => AreEqual(observed, expectation.ExpectedValue, typeCode, scale);

    // islevi: Tip-semantik karsilastirmasiyla esitsizligi degerlendirir.
    private static bool EvaluateNotEquals(ColumnExpectation expectation, string? observed, string typeCode, int? scale)
        => !AreEqual(observed, expectation.ExpectedValue, typeCode, scale);

    // islevi: Gozlenen SQL NULL durumunu degerlendirir.
    private static bool EvaluateIsNull(string? observed)
        => observed is null;

    // islevi: Gozlenen degerin SQL NULL olmadigini degerlendirir.
    private static bool EvaluateIsNotNull(string? observed)
        => observed is not null;

    // islevi: Gozlenen degerin beklenenden buyuk oldugunu tip semantigiyle degerlendirir.
    private static bool EvaluateGreaterThan(ColumnExpectation expectation, string? observed, string typeCode, int? scale)
        => Compare(observed, RequireExpected(expectation.ExpectedValue), typeCode, scale) > 0;

    // islevi: Gozlenen degerin beklenenden buyuk veya esit oldugunu tip semantigiyle degerlendirir.
    private static bool EvaluateGreaterThanOrEqual(ColumnExpectation expectation, string? observed, string typeCode, int? scale)
        => Compare(observed, RequireExpected(expectation.ExpectedValue), typeCode, scale) >= 0;

    // islevi: Gozlenen degerin beklenenden kucuk oldugunu tip semantigiyle degerlendirir.
    private static bool EvaluateLessThan(ColumnExpectation expectation, string? observed, string typeCode, int? scale)
        => Compare(observed, RequireExpected(expectation.ExpectedValue), typeCode, scale) < 0;

    // islevi: Gozlenen degerin beklenenden kucuk veya esit oldugunu tip semantigiyle degerlendirir.
    private static bool EvaluateLessThanOrEqual(ColumnExpectation expectation, string? observed, string typeCode, int? scale)
        => Compare(observed, RequireExpected(expectation.ExpectedValue), typeCode, scale) <= 0;

    // islevi: Regex'i kultur-bagimsiz, non-backtracking ve sure sinirli kurarak metin eslesmesini degerlendirir.
    private static bool EvaluateMatchesRegex(ColumnExpectation expectation, string? observed, int timeoutMs)
    {
        if (observed is null)
        {
            return false;
        }

        var pattern = RequireExpected(expectation.ExpectedValue);
        var regex = new Regex(
            pattern,
            RegexOptions.CultureInvariant | RegexOptions.NonBacktracking,
            TimeSpan.FromMilliseconds(timeoutMs));
        return regex.IsMatch(observed);
    }

    // islevi: Gozlenen degeri izinli beklenen degerlerden herhangi biriyle tip-semantik eslestirir.
    private static bool EvaluateOneOf(ColumnExpectation expectation, string? observed, string typeCode, int? scale)
        => expectation.ExpectedValues.Any(expected => AreEqual(observed, expected, typeCode, scale));

    // islevi: Iki sayisal deger arasindaki mutlak farki izinli toleransla karsilastirir.
    private static bool EvaluateWithinTolerance(ColumnExpectation expectation, string? observed)
    {
        var actual = ParseDecimal(RequireExpected(observed));
        var expected = ParseDecimal(RequireExpected(expectation.ExpectedValue));
        var tolerance = expectation.Tolerance ?? throw new BusinessException(AssertionExceptionCodes.InvalidExpectedValue);
        return Math.Abs(actual - expected) <= tolerance;
    }

    // islevi: Null ayrimini koruyarak iki scalar degeri kanonik tip semantigiyle eslestirir.
    private static bool AreEqual(string? left, string? right, string typeCode, int? scale)
    {
        if (left is null || right is null)
        {
            return left is null && right is null;
        }

        return Compare(left, right, typeCode, scale) == 0;
    }

    // islevi: Tip ailesini uygun tek karsilastirma metoduna yonlendirir.
    private static int Compare(string? left, string right, string typeCode, int? scale)
    {
        var actual = RequireExpected(left);
        if (IsNumeric(typeCode))
        {
            return CompareNumbers(actual, right);
        }

        if (IsTemporal(typeCode))
        {
            return CompareTemporal(actual, right, typeCode, scale);
        }

        return CompareText(actual, right, typeCode);
    }

    // islevi: Sayisal metinleri invariant decimal olcegine normalize edip karsilastirir.
    private static int CompareNumbers(string left, string right)
        => ParseDecimal(left).CompareTo(ParseDecimal(right));

    // islevi: Date/time/timestamp ailelerini hassasiyet kirpma ve UTC normalizasyonuyla karsilastirir.
    private static int CompareTemporal(string left, string right, string typeCode, int? scale)
        => typeCode switch
        {
            CanonicalDataTypeCodes.Date => ParseDate(left).CompareTo(ParseDate(right)),
            CanonicalDataTypeCodes.Time => Truncate(ParseTime(left).Ticks, scale).CompareTo(Truncate(ParseTime(right).Ticks, scale)),
            _ => Truncate(ParseTimestamp(left).UtcTicks, scale).CompareTo(Truncate(ParseTimestamp(right).UtcTicks, scale))
        };

    // islevi: Boolean metinleri mantiksal, diger metinleri ordinal kanonik bicimde karsilastirir.
    private static int CompareText(string left, string right, string typeCode)
        => typeCode == CanonicalDataTypeCodes.Boolean
            ? bool.Parse(left).CompareTo(bool.Parse(right))
            : string.Compare(left, right, StringComparison.Ordinal);

    // islevi: Invariant sayisal metni decimal'e cevirir; gecersiz assertion degerini kararli is hatasina donusturur.
    private static decimal ParseDecimal(string value)
        => decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw new BusinessException(AssertionExceptionCodes.InvalidExpectedValue);

    // islevi: Tarih metnini kultur-bagimsiz takvim gunune cevirir.
    private static DateOnly ParseDate(string value)
        => DateOnly.Parse(value, CultureInfo.InvariantCulture);

    // islevi: Saat metnini kultur-bagimsiz gun-ici sureye cevirir.
    private static TimeSpan ParseTime(string value)
        => TimeSpan.Parse(value, CultureInfo.InvariantCulture);

    // islevi: Timestamp metnini UTC'ye normalize eder.
    private static DateTimeOffset ParseTimestamp(string value)
        => DateTimeOffset.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    // islevi: Tick degerini kolonun bildirilen kesirli saniye hassasiyetine kirpar.
    private static long Truncate(long ticks, int? scale)
    {
        var precision = Math.Clamp(scale ?? 7, 0, 7);
        var factor = (long)Math.Pow(10, 7 - precision);
        return ticks / factor * factor;
    }

    // islevi: Bos olamayacak matcher operandini dogrular.
    private static string RequireExpected(string? value)
        => value ?? throw new BusinessException(AssertionExceptionCodes.InvalidExpectedValue);

    // islevi: Kanonik tip kodunun sayisal ailede olup olmadigini bildirir.
    private static bool IsNumeric(string code)
        => code is CanonicalDataTypeCodes.Integer or CanonicalDataTypeCodes.SmallInteger
            or CanonicalDataTypeCodes.BigInteger or CanonicalDataTypeCodes.Decimal
            or CanonicalDataTypeCodes.Float or CanonicalDataTypeCodes.Double or CanonicalDataTypeCodes.Money;

    // islevi: Kanonik tip kodunun zaman ailesinde olup olmadigini bildirir.
    private static bool IsTemporal(string code)
        => code is CanonicalDataTypeCodes.Date or CanonicalDataTypeCodes.Time
            or CanonicalDataTypeCodes.Timestamp or CanonicalDataTypeCodes.TimestampWithTimeZone;
}

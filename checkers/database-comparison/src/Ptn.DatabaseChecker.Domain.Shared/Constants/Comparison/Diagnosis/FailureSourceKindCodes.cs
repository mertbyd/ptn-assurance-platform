using System;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;

namespace Ptn.DatabaseChecker.Constants.Diagnosis;

// islevi: Teshis sinyalinin assertion veya yapilandirilmis veritabani hatasi kaynagini kararli kodlarla tanimlar.
// sistemdeki gorevi: Giris dogrulamasi, kimlik cikarimi ve RFC 9457 rapor adreslerinin ortak sabit sahibidir.
public static class FailureSourceKindCodes
{
    public const string Assertion = "Assertion";
    public const string DatabaseException = "DatabaseException";
    public const int MaxErrorCodeLength = 32;
    public const int MaxProviderFieldCount = 8;
    public const int MaxProviderFieldNameLength = 64;
    public const int MaxProviderFieldValueLength = 256;

    // islevi: RFC 9457 raporunun kararli tur ve endpoint adreslerini gruplar.
    // sistemdeki gorevi: Domain raporu ile HTTP controller'in ayni public adres sozlesmesini kullanmasini saglar.
    public static class Report
    {
        public const string Type = "urn:checknexus:problem:database-diagnosis";
        public const string Instance = "/api/database-checker/diagnosis";
        public const int Status = 200;
        public const int MaxUtf8Bytes = 4096;
        public const int MaxEvidencePerHypothesis = 3;
        public const int MaxNextChecks = 3;
    }

    // islevi: Public sinyal kaynak kodunun kapali kume icinde olup olmadigini bildirir.
    public static bool IsDefined(string? code)
        => string.Equals(code, Assertion, StringComparison.Ordinal) ||
           string.Equals(code, DatabaseException, StringComparison.Ordinal);

    // islevi: Assertion outcome kodunun KBP-704 kararli sonuc kumesinde olup olmadigini bildirir.
    public static bool IsAssertionOutcomeDefined(string? code)
        => code is AssertionOutcomeCodes.Passed
            or AssertionOutcomeCodes.RowNotFound
            or AssertionOutcomeCodes.ValueMismatch
            or AssertionOutcomeCodes.CardinalityMismatch
            or AssertionOutcomeCodes.TimedOut
            or AssertionOutcomeCodes.KeyNotUnique
            or AssertionOutcomeCodes.TableNotFound
            or AssertionOutcomeCodes.ColumnNotFound;
}

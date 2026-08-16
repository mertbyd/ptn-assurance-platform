using System;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.Managers.Catalog;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Catalog;

// islevi: Senaryo belge metninin (SourceDocument) ptn-source-canonical-v1 kuralina gore hash uretimini dogrular.
// sistemdeki gorevi: Ajan tarafindan uretilen veya farkli isetim sistemlerinden yuklenen ayni icerikli belgelerin ayni hash altinda toplanmasini (ve o hash uzerinde mühür olusmasini) garanti eder.
public class SourceHashCanonicalizationTests
{
    private const string BaselineDocument = "Feature: Kanoniklesme\n  Scenario: Test 1\n    Given X\n    Then Y";
    // Baseline belgesinin onceden hesaplanmis ptn-source-canonical-v1 uzerinden SHA-256 degeri
    private const string BaselineHash = "9bddc567f9a1ee1f0c77ff537ce32a43301776752d3a8a9400041d89625332c3";

    [Fact]
    public void ComputeSourceHash_should_produce_expected_lowercase_hex()
    {
        var hash = TestScenarioManager.ComputeSourceHash(BaselineDocument);
        hash.ShouldBe(BaselineHash);
    }

    [Theory]
    [InlineData("Feature: Kanoniklesme\r\n  Scenario: Test 1\r\n    Given X\r\n    Then Y")] // CRLF (Windows)
    [InlineData("Feature: Kanoniklesme \n  Scenario: Test 1  \n    Given X \n    Then Y   ")] // Sonda bosluklar
    [InlineData("Feature: Kanoniklesme\n  Scenario: Test 1\n    Given X\n    Then Y\n")] // Dosya sonu yeni satir
    [InlineData("Feature: Kanoniklesme\n  Scenario: Test 1\n    Given X\n    Then Y\n\n  \n")] // Dosya sonu bosluklu yeni satirlar
    [InlineData("\uFEFFFeature: Kanoniklesme\n  Scenario: Test 1\n    Given X\n    Then Y")] // BOM'lu baslangic
    public void ComputeSourceHash_should_normalize_line_endings_and_trailing_whitespaces(string document)
    {
        var hash = TestScenarioManager.ComputeSourceHash(document);
        hash.ShouldBe(BaselineHash);
    }
}

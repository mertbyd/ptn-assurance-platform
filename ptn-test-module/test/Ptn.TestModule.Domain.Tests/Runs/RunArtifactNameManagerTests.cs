using System;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: Ihracat artefakt adinin determinizmini, format kapisini ve boyut butcelerini dogrular.
// sistemdeki gorevi: Agir ciktinin satirdan cikip kararli bir blob adresine baglanmasini korur (PLAN-0003 TM-13).
public class RunArtifactNameManagerTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid RunId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    // Ayni kosum ve deneme icin uretilen ad iki cagride bayt-es olmalidir.
    [Fact]
    public void Should_create_a_deterministic_blob_name_for_the_same_run_and_attempt()
    {
        var first = RunArtifactNameManager.CreateBlobName(TenantId, RunId, 1, RunArtifactFormatCodes.Ctrf);
        var second = RunArtifactNameManager.CreateBlobName(TenantId, RunId, 1, RunArtifactFormatCodes.Ctrf);

        first.ShouldBe(second);
        first.ShouldBe("11111111111111111111111111111111/22222222222222222222222222222222/1/ctrf.json");
    }

    // Uc format ayni kosum icin ayri dosya adlarina ayrilmalidir.
    [Fact]
    public void Should_separate_the_three_export_formats_by_file_name()
    {
        var ctrf = RunArtifactNameManager.CreateBlobName(TenantId, RunId, 2, RunArtifactFormatCodes.Ctrf);
        var junit = RunArtifactNameManager.CreateBlobName(TenantId, RunId, 2, RunArtifactFormatCodes.JUnit);
        var sarif = RunArtifactNameManager.CreateBlobName(TenantId, RunId, 2, RunArtifactFormatCodes.Sarif);

        ctrf.ShouldEndWith(RunArtifactConsts.FileNames.Ctrf);
        junit.ShouldEndWith(RunArtifactConsts.FileNames.JUnit);
        sarif.ShouldEndWith(RunArtifactConsts.FileNames.Sarif);
        new[] { ctrf, junit, sarif }.ShouldBeUnique();
    }

    // Tenant tasimayan kosum host bolumune dusmelidir.
    [Fact]
    public void Should_place_a_host_run_under_the_host_segment()
    {
        var blobName = RunArtifactNameManager.CreateBlobName(null, RunId, 1, RunArtifactFormatCodes.Sarif);

        blobName.ShouldStartWith($"{HarArtifactConsts.HostTenantSegment}/");
    }

    // Ayni kosumun farkli denemeleri birbirinin artefaktini ezmemelidir.
    [Fact]
    public void Should_keep_attempts_in_separate_blob_names()
    {
        var first = RunArtifactNameManager.CreateBlobName(TenantId, RunId, 1, RunArtifactFormatCodes.Ctrf);
        var second = RunArtifactNameManager.CreateBlobName(TenantId, RunId, 2, RunArtifactFormatCodes.Ctrf);

        first.ShouldNotBe(second);
    }

    // Kapali kume disindaki format kodu kararli kodla reddedilmelidir.
    [Fact]
    public void Should_reject_an_unsupported_export_format()
    {
        var exception = Should.Throw<BusinessException>(
            () => RunArtifactNameManager.CreateBlobName(TenantId, RunId, 1, "Xml"));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.ArtifactFormatNotSupported);
    }

    // Bir tabanli olmayan deneme numarasi ad uretmeden reddedilmelidir.
    [Fact]
    public void Should_reject_a_non_positive_attempt()
    {
        var exception = Should.Throw<BusinessException>(
            () => RunArtifactNameManager.CreateBlobName(TenantId, RunId, 0, RunArtifactFormatCodes.Ctrf));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.ArtifactAttemptInvalid);
    }

    // Depo butcesini asan ihracat kalici yazima girmeden reddedilmelidir.
    [Fact]
    public void Should_reject_content_above_the_storage_budget()
    {
        var oversized = new string('a', RunArtifactConsts.MaxArtifactBytes + 1);

        var exception = Should.Throw<BusinessException>(
            () => new RunArtifactNameManager().EnsureArtifactIsValid("tenant/run/1/ctrf.json", oversized));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.ArtifactTooLarge);
    }

    // Satirda tutulamayacak uzunluktaki blob adi reddedilmelidir.
    [Fact]
    public void Should_reject_a_blob_name_above_the_column_budget()
    {
        var oversized = new string('a', RunArtifactConsts.MaxBlobNameLength + 1);

        var exception = Should.Throw<BusinessException>(
            () => new RunArtifactNameManager().EnsureArtifactIsValid(oversized));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.ArtifactBlobNameTooLong);
    }

    // Null gelen format mevcut bagi silmemeli, dolu gelen format satiri guncellemelidir.
    [Fact]
    public void Should_preserve_existing_links_when_a_format_is_absent()
    {
        var result = CreateResult();
        TestRunResultManager.AttachArtifactLinks(result, new RunArtifactLinks { CtrfBlobName = "a/ctrf.json" });

        TestRunResultManager.AttachArtifactLinks(result, new RunArtifactLinks { SarifBlobName = "a/sarif.json" });

        result.CtrfBlobName.ShouldBe("a/ctrf.json");
        result.SarifBlobName.ShouldBe("a/sarif.json");
        result.JUnitBlobName.ShouldBeNull();
    }

    // Satirda duran uc bag okuma yuzeyine kayipsiz tasinmalidir.
    [Fact]
    public void Should_read_the_attached_links_back_as_a_domain_model()
    {
        var result = CreateResult();
        TestRunResultManager.AttachArtifactLinks(
            result,
            new RunArtifactLinks
            {
                CtrfBlobName = "a/ctrf.json",
                JUnitBlobName = "a/junit.xml",
                SarifBlobName = "a/sarif.json"
            });

        var links = TestRunResultManager.ReadArtifactLinks(result);

        links.CtrfBlobName.ShouldBe("a/ctrf.json");
        links.JUnitBlobName.ShouldBe("a/junit.xml");
        links.SarifBlobName.ShouldBe("a/sarif.json");
    }

    // Bag testleri icin artefakt kolonlari bos bir terminal sonuc kabugu kurar.
    private static TestRunResult CreateResult()
    {
        return new TestRunResult(
            Guid.NewGuid(),
            RunId,
            attempt: 1,
            outcomeStatusId: Guid.NewGuid(),
            failureCategoryId: null,
            durationMs: 10,
            tenantId: TenantId,
            new TestRunTerminalModel { OutcomeCode = TestOutcomeStatusCodes.Passed },
            []);
    }
}

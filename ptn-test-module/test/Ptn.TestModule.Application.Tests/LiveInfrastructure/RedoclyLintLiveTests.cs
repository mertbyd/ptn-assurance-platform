using System;
using System.IO;
using System.Threading.Tasks;
using Ptn.TestModule.ExceptionCodes.Compilation;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Managers.Compilation;
using Ptn.TestModule.Managers.Shared;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Services.Compilation;
using Ptn.TestModule.Services.Shared;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Application.Tests.LiveInfrastructure;

// islevi: Arazzo lint kapisini gercek pinli Redocly konteyneriyle kanitlar.
// sistemdeki gorevi: Stub unit kanitinin otesinde Docker process siniri ve XPath reddini canli olarak sabitler.
[Collection(LiveInfrastructureCollection.Name)]
public class RedoclyLintLiveTests
{
    // Gecerli elle yazilmis Arazzo 1.0.1 belgesi gercek CLI'da temiz lint edilmelidir.
    [Fact]
    [Trait("Category", "LiveInfrastructure")]
    public async Task Should_lint_a_hand_written_arazzo_document_with_the_pinned_container()
    {
        LiveInfrastructureCollection.EnsurePinnedRedoclyImageIsAvailable();

        var result = await CreateLinter().LintAsync(ReadFixture("valid-lookup-scenario.arazzo.yaml"), default);

        result.IsValid.ShouldBeTrue(result.Diagnostics);
    }

    // XPath criterion'u lint process'ine ulasmadan derleme kapisinda kararli kodla reddedilmelidir.
    [Fact]
    [Trait("Category", "LiveInfrastructure")]
    public async Task Should_reject_xpath_criteria_before_starting_the_lint_process()
    {
        LiveInfrastructureCollection.EnsurePinnedRedoclyImageIsAvailable();
        var manager = new ArazzoCompilerManager(new ProfilePackManager(), CreateLinter());

        var exception = await Should.ThrowAsync<BusinessException>(() => manager.CompileAsync(
            ReadFixture("xpath-criteria.arazzo.yaml"),
            new ProfilePack(),
            Guid.NewGuid()));

        exception.Code.ShouldBe(TestModuleCompilationErrorCodes.XPathCriteriaUnsupported);
    }

    // Gercek linter portunu Manager ve ortak process siniri uygulamalariyla kurar.
    private static RedoclyArazzoDocumentLinter CreateLinter()
    {
        return new RedoclyArazzoDocumentLinter(
            new ArazzoLintManager(),
            new ProcessBoundaryService(new ProcessPlanManager()));
    }

    // Test kosucusunun bin klasorunden modul kokunu bulup kaynak fixture'i okur.
    private static string ReadFixture(string name)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ptn.TestModule.slnx")))
        {
            directory = directory.Parent;
        }

        var moduleRoot = directory ?? throw new DirectoryNotFoundException("Ptn.TestModule.slnx");
        return File.ReadAllText(Path.Combine(
            moduleRoot.FullName,
            "test",
            "Ptn.TestModule.Application.Tests",
            "LiveInfrastructure",
            "Fixtures",
            name));
    }
}

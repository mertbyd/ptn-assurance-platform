using System;
using NSubstitute;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Services.Runs;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Runs;

// islevi: Elle yazilmis Arazzo belgesinden surum, XPath ve adim kimligi olgularinin cikarilmasini dogrular.
// sistemdeki gorevi: Belge cozumlemesinin surec sinirinda kalmasini ve kararin Manager'da olmasini korur.
public class WorkflowRunnerServiceTests
{
    // Elle yazilmis 1.0.1 belgesi tum adim kimlikleriyle okunmalidir.
    [Fact]
    public void Should_read_facts_from_hand_written_arazzo_document()
    {
        var facts = CreateService().ReadDocumentFacts(ArazzoDocument);

        facts.ArazzoVersion.ShouldBe(WorkflowRunnerConsts.ArazzoTargetVersion);
        facts.HasXPathCriterion.ShouldBeFalse();
        facts.StepKeys.ShouldBe(["create-order", "verify-subject-row"]);
    }

    // Belge agacinin herhangi bir seviyesindeki XPath criterion'u gorulmelidir.
    [Fact]
    public void Should_detect_xpath_criterion_anywhere_in_document()
    {
        var document = ArazzoDocument.Replace("type: jsonpath", "type: xpath", StringComparison.Ordinal);

        var facts = CreateService().ReadDocumentFacts(document);

        facts.HasXPathCriterion.ShouldBeTrue();
    }

    // Cozulemeyen belge kararli koda cevrilmelidir.
    [Fact]
    public void Should_reject_unreadable_document()
    {
        var exception = Should.Throw<BusinessException>(() =>
            CreateService().ReadDocumentFacts("\tarazzo: [1.0.1"));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.ArazzoVersionUnsupported);
    }

    // Surec sinirini varsayilan ayarlarla calisan planner'a baglar.
    private static WorkflowRunnerService CreateService()
    {
        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        return new WorkflowRunnerService(new WorkflowRunPlanner(settingProvider));
    }

    private const string ArazzoDocument = """
        arazzo: 1.0.1
        info:
          title: Order creation end to end
          version: 1.0.1
        sourceDescriptions:
          - name: orders
            url: ./orders.openapi.yaml
            type: openapi
        workflows:
          - workflowId: create-and-verify-order
            steps:
              - stepId: create-order
                operationId: createOrder
                successCriteria:
                  - condition: $statusCode == 201
                  - context: $response.body
                    condition: $.data.id
                    type: jsonpath
              - stepId: verify-subject-row
                operationPath: '{$sourceDescriptions.databaseChecker.url}#/paths/~1api~1comparison~1assertions~1row/post'
                successCriteria:
                  - condition: $response.body#/data/passed == true
        """;
}

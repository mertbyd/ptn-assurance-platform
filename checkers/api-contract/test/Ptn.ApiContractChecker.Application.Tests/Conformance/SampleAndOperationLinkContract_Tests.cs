using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Application.Mappers.Conformance;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Conformance;
using Ptn.ApiContractChecker.Controllers.Conformance;
using Ptn.ApiContractChecker.Dtos.Conformance;
using Ptn.ApiContractChecker.FluentValidation.Conformance;
using Ptn.ApiContractChecker.Models.Conformance;
using Ptn.ApiContractChecker.Permissions;
using Ptn.ApiContractChecker.Services.Conformance;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Conformance;

// islevi: Yeni conformance DTO, validator, Mapperly, interface, route ve permission sozlesmelerini kanitlar.
// sistemdeki gorevi: Public yuzeyin Domain modelinden veya HTTP yetkilendirmesinden sessizce ayrismasini engeller.
public class SampleAndOperationLinkContract_Tests
{
    [Fact]
    public void Public_Input_Validators_Should_Reject_Open_Ended_Or_Unbounded_Requests()
    {
        var sampleResult = new SampleSetRequestDtoValidator().Validate(new SampleSetRequestDto
        {
            SnapshotId = Guid.NewGuid(),
            OperationId = "createOrder",
            SampleKindCode = "Unknown",
            MaxSamplesPerField = SampleGenerationConsts.MaxSamplesPerField + 1
        });
        var linkResult = new OperationLinkRequestDtoValidator().Validate(new OperationLinkRequestDto
        {
            SnapshotId = Guid.NewGuid(),
            SourceOperationId = string.Empty,
            MaxCandidates = SampleGenerationConsts.DefaultMaxCandidates + 1
        });

        sampleResult.IsValid.ShouldBeFalse();
        linkResult.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Mapperly_Should_Map_Requests_And_Keep_Human_Approval_True()
    {
        var mapper = new ConformanceMapper();
        var request = mapper.MapToRequest(new SampleSetRequestDto
        {
            SnapshotId = Guid.NewGuid(),
            OperationId = "createOrder",
            SampleKindCode = "Boundary",
            MaxSamplesPerField = 3
        });
        var result = mapper.MapToDto(new OperationLinkResult("passed",
        [
            new OperationLinkCandidate("getOrder", "DeclaredLink", [], 1m)
        ]));

        request.OperationId.ShouldBe("createOrder");
        request.MaxSamplesPerField.ShouldBe(3);
        result.Candidates.Single().RequiresHumanApproval.ShouldBeTrue();
    }

    [Fact]
    public void Interface_And_Controller_Should_Expose_Exact_Authorized_Routes()
    {
        typeof(IResponseConformanceAppService).GetMethod(
            nameof(IResponseConformanceAppService.BuildSampleSetAsync)).ShouldNotBeNull();
        typeof(IResponseConformanceAppService).GetMethod(
            nameof(IResponseConformanceAppService.SuggestOperationLinksAsync)).ShouldNotBeNull();

        AssertAction(
            nameof(ResponseConformanceController.BuildSampleSet),
            ApiContractCheckerRoutes.ConformanceSampleSets,
            ApiContractCheckerPermissions.Conformance.GenerateSamples);
        AssertAction(
            nameof(ResponseConformanceController.SuggestOperationLinks),
            ApiContractCheckerRoutes.ConformanceOperationLinks,
            ApiContractCheckerPermissions.Conformance.SuggestLinks);
    }

    // Controller action'inin route ve action-level permission sozlesmesini birlikte dogrular.
    private static void AssertAction(string methodName, string route, string permission)
    {
        var method = typeof(ResponseConformanceController).GetMethod(methodName)!;
        method.GetCustomAttribute<HttpPostAttribute>()!.Template.ShouldBe(route);
        method.GetCustomAttribute<AuthorizeAttribute>()!.Policy.ShouldBe(permission);
    }
}

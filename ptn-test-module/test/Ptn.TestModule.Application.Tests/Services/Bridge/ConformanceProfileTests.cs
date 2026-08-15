using System;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Dtos.Bridge.Api;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.FluentValidation.Bridge.Api;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Senaryo response gozleminin uc uygunluk profilini ve Runtime varsayilanini dogrular.
// sistemdeki gorevi: API checker cagrilarinin tek profile yeniden pinlenmesini engeller.
public class ConformanceProfileTests
{
    // Kapali sozlukteki her senaryo profili checker istegine degismeden tasinmalidir.
    [Theory]
    [InlineData(PtnConformanceProfileCodes.Strict)]
    [InlineData(PtnConformanceProfileCodes.Runtime)]
    [InlineData(PtnConformanceProfileCodes.Lenient)]
    public void Should_resolve_each_scenario_profile(string profileCode)
    {
        var request = new ApiOracleManager().CreateResponseRequest(CreateObservation(profileCode));

        request.ProfileCode.ShouldBe(profileCode);
    }

    // Profil secmeyen mevcut senaryolar geriye uyumlu Runtime davranisini korumalidir.
    [Fact]
    public void Should_default_missing_profile_to_runtime()
    {
        var request = new ApiOracleManager().CreateResponseRequest(CreateObservation(null));

        request.ProfileCode.ShouldBe(PtnConformanceProfileCodes.Runtime);
    }

    // Kapali sozluk disindaki profil public input kapisinda checker'a ulasmadan reddedilmelidir.
    [Fact]
    public void Should_reject_unknown_profile_in_the_input_validator()
    {
        var input = new ResponseObservationDto
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/orders/1",
            StatusCode = 200,
            ProfileCode = "Exploratory"
        };

        var result = new ResponseObservationDtoValidator().Validate(input);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error =>
            error.ErrorMessage == TestModuleBridgeErrorCodes.Validation.ProfileCodeInvalid);
    }

    // Manager testleri icin en kucuk gecerli response gozlemini kurar.
    private static ResponseObservation CreateObservation(string? profileCode)
    {
        return new ResponseObservation
        {
            SnapshotId = Guid.NewGuid(),
            Method = "GET",
            Path = "/orders/1",
            StatusCode = 200,
            ProfileCode = profileCode
        };
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.DatabaseChecker.Constants.Capabilities;
using Ptn.DatabaseChecker.Dtos.Capabilities;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Services.Bridge;
using Shouldly;
using Xunit;
using CheckerWriteSetCapabilityAppService = Ptn.DatabaseChecker.Services.Capabilities.IWriteSetCapabilityAppService;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Yazma kumesi capability ve capture sonucunun checker yuzeyinden devralindigini dogrular.
// sistemdeki gorevi: Test Module'de ikinci baglanti/slot sahibinin geri gelmesini engeller.
public class WriteSetCapabilityTests
{
    // Logical decoding yoklugunu exception yerine checker'in Inferred capability sonucu olarak tasir.
    [Fact]
    public async Task Should_use_checker_capability_without_throwing_when_wal_is_not_logical()
    {
        var checker = Substitute.For<CheckerWriteSetCapabilityAppService>();
        checker.ProbeAsync(Arg.Any<CapabilityProbeRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new CapabilityLevelDto
            {
                StrengthCode = FootprintStrengthCodes.Inferred,
                HasExclusiveSandbox = true,
                Reasons = [CapabilityReasonCodes.WalLevelNotLogical]
            });
        var service = new WriteSetCapabilityAppService(checker, new FootprintCapabilityManager());

        var result = await service.ProbeCapabilityAsync(
            Guid.NewGuid(), true, CancellationToken.None);

        result.FootprintStrengthCode.ShouldBe(PtnFootprintStrengthCodes.Inferred);
        result.HasProjectionSurface.ShouldBeTrue();
        result.Reasons.ShouldContain(CapabilityReasonCodes.WalLevelNotLogical);
        await checker.Received(1).ProbeAsync(
            Arg.Is<CapabilityProbeRequestDto>(request => request.RequiresExclusiveSandbox),
            CancellationToken.None);
    }

    // Checker capture sonucunda false gelse bile advisory sinirini zorunlu true yapar.
    [Fact]
    public async Task Should_keep_checker_capture_advisory()
    {
        var checker = Substitute.For<CheckerWriteSetCapabilityAppService>();
        WriteSetCaptureRequestDto? capturedRequest = null;
        checker.CaptureAsync(Arg.Any<WriteSetCaptureRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedRequest = callInfo.ArgAt<WriteSetCaptureRequestDto>(0);
                return new WriteSetResultDto
                {
                    StrengthCode = FootprintStrengthCodes.Exact,
                    Tables = ["public.tickets"],
                    IsAdvisoryOnly = false
                };
            });
        var service = new WriteSetCapabilityAppService(checker, new FootprintCapabilityManager());
        var captureId = Guid.NewGuid();

        var result = await service.CaptureWriteSetAsync(
            Guid.NewGuid(), captureId, CancellationToken.None);

        result.StrengthCode.ShouldBe(PtnFootprintStrengthCodes.Exact);
        result.IsAdvisoryOnly.ShouldBeTrue();
        result.Tables.ShouldContain("public.tickets");
        capturedRequest.ShouldNotBeNull();
        capturedRequest.Correlation.ShouldNotBeNull();
        capturedRequest.Correlation.TraceId.ShouldBe(captureId.ToString("N"));
        capturedRequest.Correlation.StepKey.ShouldBe(PtnCorrelationConsts.WriteSetCaptureStepKey);
    }
}

using System;
using FluentValidation.TestHelper;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.FluentValidation.Bridge.Agent;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Ground isteginin operasyon referansi veya adim niyetiyle baslayabilmesini dogrular.
// sistemdeki gorevi: Dogal-dilden baslangici acarken eksik secim girdisinin Manager'a ulasmasini engeller.
public class GroundRequestDtoValidatorTests
{
    // Operasyon referansi yoksa aday sorgusu icin StepIntent zorunlu olur.
    [Fact]
    public void Should_require_step_intent_when_operation_reference_is_missing()
    {
        var result = new GroundRequestDtoValidator().TestValidate(ValidInput());

        result.ShouldHaveValidationErrorFor(input => input.StepIntent);
    }

    // Kapali operasyon referansi verildiginde geriye uyumlu kesin yol StepIntent istemez.
    [Fact]
    public void Should_accept_a_closed_operation_reference_without_step_intent()
    {
        var input = ValidInput();
        input.OperationReferenceId = Guid.NewGuid();

        var result = new GroundRequestDtoValidator().TestValidate(input);

        result.ShouldNotHaveValidationErrorFor(item => item.StepIntent);
    }

    // Diger zorunlu alanlari gecerli en kucuk ground girdisinde toplar.
    private static GroundRequestDto ValidInput() => new()
    {
        ProfileKey = "unit-profile",
        SpecSnapshotId = Guid.NewGuid(),
        ConnectionId = Guid.NewGuid(),
        ResponseFormat = PtnResponseFormatCodes.Detailed
    };
}

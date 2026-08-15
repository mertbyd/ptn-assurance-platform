using FluentValidation;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.ExceptionCodes.Bridge;

namespace Ptn.TestModule.FluentValidation.Bridge.Agent;

// islevi: MCP Task esleme girdisinin kimlik ve polling surelerini dogrular.
// sistemdeki gorevi: Gecersiz TTL veya durumun protokol cevabina sizmasini engeller.
public sealed class McpTaskStatusRequestDtoValidator : AbstractValidator<McpTaskStatusRequestDto>
{
    public McpTaskStatusRequestDtoValidator()
    {
        RuleFor(input => input.TaskId).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.TaskIdRequired);
        RuleFor(input => input.InternalStatus).NotEmpty().WithMessage(TestModuleBridgeErrorCodes.Validation.TaskStatusRequired);
        RuleFor(input => input.TtlMs).GreaterThan(0).WithMessage(TestModuleBridgeErrorCodes.Validation.TimeoutInvalid);
        RuleFor(input => input.PollIntervalMs).GreaterThan(0).LessThanOrEqualTo(input => input.TtlMs)
            .WithMessage(TestModuleBridgeErrorCodes.Validation.PollIntervalInvalid);
    }
}

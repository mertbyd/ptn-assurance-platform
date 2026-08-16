using FluentValidation;
using Ptn.DatabaseChecker.Constants.Capabilities;
using Ptn.DatabaseChecker.Dtos.Capabilities;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.FluentValidation.Correlation;

namespace Ptn.DatabaseChecker.FluentValidation.Capabilities;

// islevi: Write-set capture request'in kimlik, aday tablo tavani, adres grameri ve correlation seklini dogrular.
// sistemdeki gorevi: Provider'a yalniz sinirli schema.table adaylari gitmesini garanti eder.
public sealed class WriteSetCaptureRequestDtoValidator : AbstractValidator<WriteSetCaptureRequestDto>
{
    // islevi: Capture request-shape kurallarini kararli validation kodlariyla kaydeder.
    public WriteSetCaptureRequestDtoValidator()
    {
        RuleFor(input => input.ConnectionId)
            .NotEmpty().WithMessage(DataComparisonExceptionCodes.WriteSetValidation.ConnectionIdRequired);
        RuleFor(input => input.CaptureRef)
            .NotEmpty().WithMessage(DataComparisonExceptionCodes.WriteSetValidation.CaptureRefRequired);
        RuleFor(input => input.CandidateTables)
            .NotEmpty().WithMessage(DataComparisonExceptionCodes.WriteSetValidation.CandidateTablesRequired)
            .Must(tables => tables.Count <= WriteSetConsts.MaxCandidateTables)
            .WithMessage(DataComparisonExceptionCodes.WriteSetValidation.CandidateTablesTooMany);
        RuleForEach(input => input.CandidateTables)
            .Matches(WriteSetConsts.CandidateTablePattern)
            .WithMessage(DataComparisonExceptionCodes.WriteSetValidation.CandidateTableInvalid);
        RuleFor(input => input.Correlation!)
            .SetValidator(new CorrelationRefDtoValidator())
            .When(input => input.Correlation is not null);
    }
}

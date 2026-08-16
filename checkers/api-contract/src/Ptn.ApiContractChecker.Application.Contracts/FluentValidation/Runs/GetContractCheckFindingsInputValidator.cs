using FluentValidation;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Constants.Runs.Lookups;
using Ptn.ApiContractChecker.Dtos.Runs;
using Ptn.ApiContractChecker.ExceptionCodes.Runs;

namespace Ptn.ApiContractChecker.FluentValidation.Runs;

// islevi: Bulgu sayfalama filtrelerini kapali kod, uzunluk ve negatif sayfa degerlerine karsi dogrular.
// sistemdeki gorevi: Repository'ye yalniz sekil olarak gecerli bakim ani sorgularinin ulasmasini saglar.
public class GetContractCheckFindingsInputValidator : AbstractValidator<GetContractCheckFindingsInput>
{
    public GetContractCheckFindingsInputValidator()
    {
        RuleFor(input => input.SkipCount).GreaterThanOrEqualTo(0);
        RuleFor(input => input.MaxResultCount).GreaterThan(0);
        RuleFor(input => input.SeverityCode)
            .Must(value => value is null || DifferenceSeverityCodes.All.Contains(value));
        RuleFor(input => input.KindCode)
            .Must(value => value is null || DifferenceKindCodes.All.Contains(value));
        RuleFor(input => input.ChangeStateCode)
            .Must(value => value is null || FindingChangeStateCodes.All.Contains(value));
        RuleFor(input => input.Path).MaximumLength(ContractCheckRunConsts.MaxFindingFilterLength);
        RuleFor(input => input.SchemaName).MaximumLength(ContractCheckRunConsts.MaxFindingFilterLength);
        RuleFor(input => input.SinceRunId)
            .NotEqual(Guid.Empty)
            .When(input => input.SinceRunId.HasValue)
            .WithMessage(ContractCheckRunExceptionCodes.Validation.FindingSinceRunIdInvalid);
        RuleFor(input => input.Fingerprints)
            .Must(values => values.Count <= ContractCheckRunConsts.MaxFindingFingerprintFilterCount)
            .WithMessage(ContractCheckRunExceptionCodes.Validation.FindingFingerprintLimitExceeded)
            .Must(HaveUniqueFingerprints)
            .WithMessage(ContractCheckRunExceptionCodes.Validation.FindingFingerprintDuplicate);
        RuleForEach(input => input.Fingerprints)
            .NotEmpty()
            .Matches(ContractCheckRunConsts.FindingFingerprintPattern)
            .WithMessage(ContractCheckRunExceptionCodes.Validation.FindingFingerprintInvalid);
    }

    // islevi: Hex harf buyuklugu farkli ayni SHA-256 degerini de duplicate kabul eder.
    private static bool HaveUniqueFingerprints(IReadOnlyCollection<string> fingerprints)
        => fingerprints.Distinct(StringComparer.OrdinalIgnoreCase).Count() == fingerprints.Count;
}

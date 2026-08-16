using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Dtos.Runs;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Runs;

// islevi: Bulgu sorgusunun filtre kodlarini ve adres uzunluklarini API sinirinda dogrular.
// sistemdeki gorevi: Repository'ye kapali katalog disi siddet/yon kodu veya sinirsiz adres metni ulasmasini engeller.
/// <summary>
/// Bulgu sorgusunun kod ve adres filtrelerini dogrular.
/// </summary>
public class FindingQueryInputValidator : AbstractValidator<FindingQueryInput>
{
    /// <summary>
    /// Opsiyonel filtrelere kapali katalog ve uzunluk kurallarini ekler.
    /// </summary>
    public FindingQueryInputValidator()
    {
        RuleFor(input => input.SeverityCode)
            .Must(code => code is null || DifferenceSeverityCodes.IsDefined(code))
            .WithMessage(ComparisonRunExceptionCodes.Validation.FindingSeverityInvalid);
        RuleFor(input => input.KindCode)
            .Must(code => code is null || DifferenceKindCodes.IsDefined(code))
            .WithMessage(ComparisonRunExceptionCodes.Validation.FindingKindInvalid);
        RuleFor(input => input.ObjectTypeCode)
            .Must(code => code is null || SchemaObjectTypeCodes.IsDefined(code))
            .WithMessage(ComparisonRunExceptionCodes.Validation.FindingObjectTypeInvalid);
        RuleFor(input => input.SchemaName)
            .MaximumLength(SchemaObjectConsts.MaxSchemaNameLength)
            .WithMessage(ComparisonRunExceptionCodes.Validation.FindingSchemaNameTooLong);
        RuleFor(input => input.TableName)
            .MaximumLength(SchemaObjectConsts.MaxObjectNameLength)
            .WithMessage(ComparisonRunExceptionCodes.Validation.FindingTableNameTooLong);
        RuleFor(input => input.SinceRunId)
            .NotEqual(Guid.Empty)
            .When(input => input.SinceRunId.HasValue)
            .WithMessage(ComparisonRunExceptionCodes.Validation.FindingSinceRunIdInvalid);
        RuleFor(input => input.Fingerprints)
            .Must(fingerprints => fingerprints.Count <= ComparisonRunConsts.MaxFindingFingerprintFilterCount)
            .WithMessage(ComparisonRunExceptionCodes.Validation.FindingFingerprintLimitExceeded)
            .Must(HaveUniqueFingerprints)
            .WithMessage(ComparisonRunExceptionCodes.Validation.FindingFingerprintDuplicate);
        RuleForEach(input => input.Fingerprints)
            .NotEmpty()
            .Matches(ComparisonRunConsts.FindingFingerprintPattern)
            .WithMessage(ComparisonRunExceptionCodes.Validation.FindingFingerprintInvalid);
    }

    // islevi: Ayni SHA-256 degerinin harf buyuklugu farkiyla iki kez verilmesini de duplicate sayar.
    private static bool HaveUniqueFingerprints(IReadOnlyCollection<string> fingerprints)
        => fingerprints.Distinct(StringComparer.OrdinalIgnoreCase).Count() == fingerprints.Count;
}

using System.Linq;
using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Dtos.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.FluentValidation.Scopes;

namespace Ptn.DatabaseChecker.FluentValidation.Comparison;

// islevi: Sema karsilastirma isteginin girdi-format kurallarini tanimlar.
// sistemdeki gorevi: Kaynak/hedef baglanti kimliklerinin bos olmamasini, modun izinli kod kumesinde olmasini saglar ve her kapsam kuralini mevcut ScopeRuleDtoValidator'a baglar; baglanti varlik kontrolu AppService/manager katmaninda yapilir.
public class CompareSchemaRequestDtoValidator : AbstractValidator<CompareSchemaRequestDto>
{
    // ComparisonType lookup'inin kararli kod kumesi; anlik karsilastirma bu setin disinda bir mod tasiyamaz.
    private static readonly string[] AllowedComparisonTypeCodes =
    {
        ComparisonTypeCodes.SchemaOnly,
        ComparisonTypeCodes.DataOnly,
        ComparisonTypeCodes.Both
    };

    public CompareSchemaRequestDtoValidator()
    {
        // Kaynak baglanti kimligi bos Guid olamaz; varlik kontrolu AppService/manager katmaninda yapilir.
        RuleFor(x => x.SourceConnectionId)
            .NotEmpty().WithMessage(ComparisonRunExceptionCodes.Validation.SourceConnectionIdRequired);

        // Hedef baglanti kimligi bos Guid olamaz; varlik kontrolu AppService/manager katmaninda yapilir.
        RuleFor(x => x.TargetConnectionId)
            .NotEmpty().WithMessage(ComparisonRunExceptionCodes.Validation.TargetConnectionIdRequired);

        // Karsilastirma modu bos olamaz ve yalnizca izinli ComparisonTypeCodes degerlerinden biri olabilir.
        RuleFor(x => x.ComparisonTypeCode)
            .NotEmpty().WithMessage(ComparisonRunExceptionCodes.Validation.ComparisonTypeCodeRequired)
            .Must(code => AllowedComparisonTypeCodes.Contains(code)).WithMessage(ComparisonRunExceptionCodes.Validation.ComparisonTypeCodeInvalid);

        // Her kapsam kurali gomulu ScopeRuleDtoValidator format kurallarindan gecer; null liste iterasyonsuz gecer.
        RuleForEach(x => x.ScopeRules).SetValidator(new ScopeRuleDtoValidator());
    }
}

using System.Linq;
using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Dtos.Scopes;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Scopes;

// islevi: Gomulu tek bir kapsam kuralinin (ScopeRuleDto) girdi-format kurallarini calistirir.
// sistemdeki gorevi: ComparisonDefinition create/update validator'lari her kurali RuleForEach ile buna baglar; ScopeKindCode kararli kod kumesine (ScopeKindCodes) karsi dogrulanir - FK/DB kontrolu yok.
public class ScopeRuleDtoValidator : AbstractValidator<ScopeRuleDto>
{
    // ScopeKind lookup'inin kararli kod kumesi; kural bu setin disinda bir kod tasiyamaz.
    private static readonly string[] AllowedScopeKindCodes =
    {
        ScopeKindCodes.Include,
        ScopeKindCodes.Exclude,
        ScopeKindCodes.Ignore,
        ScopeKindCodes.DataCompare
    };

    public ScopeRuleDtoValidator()
    {
        // Kural turu kodu bos olamaz ve yalnizca izinli ScopeKindCodes degerlerinden biri olabilir.
        RuleFor(x => x.ScopeKindCode)
            .NotEmpty().WithMessage(ComparisonDefinitionExceptionCodes.Validation.ScopeRuleKindRequired)
            .Must(code => AllowedScopeKindCodes.Contains(code)).WithMessage(ComparisonDefinitionExceptionCodes.Validation.ScopeRuleKindInvalid);

        // Kural bir semaya baglanmalidir; bos olamaz ve uzunluk sinirini asamaz.
        RuleFor(x => x.SchemaName)
            .NotEmpty().WithMessage(ComparisonDefinitionExceptionCodes.Validation.ScopeRuleSchemaRequired)
            .MaximumLength(SchemaObjectConsts.MaxSchemaNameLength).WithMessage(ComparisonDefinitionExceptionCodes.Validation.ScopeRuleSchemaMaxLength);

        // Veri sayimi tablo secimi uzerinden yapilir; DataCompare kurali nesne adi olmadan belirsiz kalamaz.
        RuleFor(x => x.ObjectName)
            .NotEmpty().When(x => x.ScopeKindCode == ScopeKindCodes.DataCompare)
            .WithMessage(ComparisonDefinitionExceptionCodes.Validation.ScopeRuleObjectRequired);

        // Nesne adi opsiyoneldir (null = tum sema); verilirse uzunluk sinirini asamaz.
        RuleFor(x => x.ObjectName)
            .MaximumLength(SchemaObjectConsts.MaxObjectNameLength).WithMessage(ComparisonDefinitionExceptionCodes.Validation.ScopeRuleObjectMaxLength);

        // Alt nesne (kolon/index/constraint) adi opsiyoneldir (null = nesnenin tamami); verilirse uzunluk sinirini asamaz.
        RuleFor(x => x.ChildName)
            .MaximumLength(SchemaObjectConsts.MaxObjectNameLength).WithMessage(ComparisonDefinitionExceptionCodes.Validation.ScopeRuleObjectMaxLength);
    }
}

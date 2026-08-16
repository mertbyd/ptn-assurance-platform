using System;
using System.Linq.Expressions;
using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Definitions;

// islevi: ComparisonDefinition create/update DTO'larinin ortak girdi-format kurallarini toplar.
// sistemdeki gorevi: Tarif adi, baglanti/mod FK boslugu ve aciklama uzunlugu kurallarinin tekrarini engeller.
public abstract class ComparisonDefinitionDtoValidatorBase<TDto> : AbstractValidator<TDto>
{
    // islevi: 0.1.x tuketicilerinin kullandigi tanim validator imzasini ikili uyumlu tutar.
    protected void AddRules(
        Expression<Func<TDto, string>> nameSelector,
        Expression<Func<TDto, Guid>> sourceConnectionIdSelector,
        Expression<Func<TDto, Guid>> targetConnectionIdSelector,
        Expression<Func<TDto, Guid>> comparisonTypeIdSelector,
        Expression<Func<TDto, string?>> descriptionSelector)
    {
        AddCoreRules(nameSelector, sourceConnectionIdSelector, targetConnectionIdSelector, comparisonTypeIdSelector, descriptionSelector);
    }

    protected void AddRules(
        Expression<Func<TDto, string>> nameSelector,
        Expression<Func<TDto, Guid>> sourceConnectionIdSelector,
        Expression<Func<TDto, Guid>> targetConnectionIdSelector,
        Expression<Func<TDto, Guid>> comparisonTypeIdSelector,
        Expression<Func<TDto, string>> sourceRoleCodeSelector,
        Expression<Func<TDto, string?>> descriptionSelector)
    {
        AddCoreRules(nameSelector, sourceConnectionIdSelector, targetConnectionIdSelector, comparisonTypeIdSelector, descriptionSelector);

        // Kaynak tarafi rolu kapali Reference/Audited katalogundan gelmelidir.
        RuleFor(sourceRoleCodeSelector)
            .NotEmpty().WithMessage(ComparisonDefinitionExceptionCodes.Validation.SourceRoleCodeRequired)
            .Must(ComparisonSideRoleCodes.IsDefined)
            .WithMessage(ComparisonDefinitionExceptionCodes.Validation.SourceRoleCodeInvalid);
    }

    // islevi: Eski ve source-role genisletilmis overload'un ortak tanim kurallarini tek yerde uygular.
    private void AddCoreRules(
        Expression<Func<TDto, string>> nameSelector,
        Expression<Func<TDto, Guid>> sourceConnectionIdSelector,
        Expression<Func<TDto, Guid>> targetConnectionIdSelector,
        Expression<Func<TDto, Guid>> comparisonTypeIdSelector,
        Expression<Func<TDto, string?>> descriptionSelector)
    {
        // Tarif adi bos olamaz ve semadaki uzunluk sinirini asamaz.
        RuleFor(nameSelector)
            .NotEmpty().WithMessage(ComparisonDefinitionExceptionCodes.Validation.NameRequired)
            .MaximumLength(ComparisonDefinitionConsts.MaxNameLength).WithMessage(ComparisonDefinitionExceptionCodes.Validation.NameMaxLength);

        // SourceConnectionId bos Guid olamaz; baglanti varlik kontrolu manager katmaninda yapilir.
        RuleFor(sourceConnectionIdSelector)
            .NotEmpty().WithMessage(ComparisonDefinitionExceptionCodes.Validation.SourceConnectionIdRequired);

        // TargetConnectionId bos Guid olamaz; baglanti varlik kontrolu manager katmaninda yapilir.
        RuleFor(targetConnectionIdSelector)
            .NotEmpty().WithMessage(ComparisonDefinitionExceptionCodes.Validation.TargetConnectionIdRequired);

        // ComparisonTypeId bos Guid olamaz; lookup varlik kontrolu manager katmaninda yapilir.
        RuleFor(comparisonTypeIdSelector)
            .NotEmpty().WithMessage(ComparisonDefinitionExceptionCodes.Validation.ComparisonTypeIdRequired);

        // Tarif aciklamasi opsiyoneldir; verilirse semadaki uzunluk sinirini asamaz.
        RuleFor(descriptionSelector)
            .MaximumLength(ComparisonDefinitionConsts.MaxDescriptionLength).WithMessage(ComparisonDefinitionExceptionCodes.Validation.DescriptionMaxLength);
    }
}

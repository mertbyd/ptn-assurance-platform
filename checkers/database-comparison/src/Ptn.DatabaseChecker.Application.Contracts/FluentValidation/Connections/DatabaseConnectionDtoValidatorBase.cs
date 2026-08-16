using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Connections;

// islevi: DatabaseConnection create/update DTO'larinin ortak girdi-format kurallarini toplar.
// sistemdeki gorevi: Baglanti alanlari icin ayni bosluk, uzunluk ve port araligi kurallarinin iki validator'da tekrar yazilmasini engeller.
public abstract class DatabaseConnectionDtoValidatorBase<TDto> : AbstractValidator<TDto>
{
    // islevi: 0.1.x tuketicilerinin kullandigi ortak baglanti kurali imzasini ikili uyumlu tutar.
    protected void AddRules(
        System.Linq.Expressions.Expression<System.Func<TDto, System.Guid>> engineIdSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> nameSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> hostSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, int>> portSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> databaseNameSelector)
    {
        AddCoreRules(engineIdSelector, nameSelector, hostSelector, portSelector, databaseNameSelector);
    }

    protected void AddRules(
        System.Linq.Expressions.Expression<System.Func<TDto, System.Guid>> engineIdSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> nameSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> hostSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, int>> portSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> databaseNameSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> tlsModeCodeSelector)
    {
        AddCoreRules(engineIdSelector, nameSelector, hostSelector, portSelector, databaseNameSelector);

        // TLS modu serbest metin degil Domain.Shared'daki kapali kararli kod kumesinden gelmelidir.
        RuleFor(tlsModeCodeSelector)
            .NotEmpty().WithMessage(DatabaseConnectionExceptionCodes.InvalidTlsMode)
            .Must(TlsModeCodes.IsDefined).WithMessage(DatabaseConnectionExceptionCodes.InvalidTlsMode);
    }

    // islevi: Eski ve TLS genisletilmis overload'un paylastigi baglanti sekil kurallarini tek yerde uygular.
    private void AddCoreRules(
        System.Linq.Expressions.Expression<System.Func<TDto, System.Guid>> engineIdSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> nameSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> hostSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, int>> portSelector,
        System.Linq.Expressions.Expression<System.Func<TDto, string>> databaseNameSelector)
    {
        // EngineId bos Guid olamaz; lookup varlik kontrolu manager katmaninda yapilir.
        RuleFor(engineIdSelector)
            .NotEmpty().WithMessage(DatabaseConnectionExceptionCodes.Validation.EngineIdRequired);

        // Baglanti adi bos olamaz ve semadaki uzunluk sinirini asamaz.
        RuleFor(nameSelector)
            .NotEmpty().WithMessage(DatabaseConnectionExceptionCodes.Validation.NameRequired)
            .MaximumLength(DatabaseConnectionConsts.MaxNameLength).WithMessage(DatabaseConnectionExceptionCodes.Validation.NameMaxLength);

        // Host bos olamaz ve semadaki uzunluk sinirini asamaz.
        RuleFor(hostSelector)
            .NotEmpty().WithMessage(DatabaseConnectionExceptionCodes.Validation.HostRequired)
            .MaximumLength(DatabaseConnectionConsts.MaxHostLength).WithMessage(DatabaseConnectionExceptionCodes.Validation.HostMaxLength);

        // Port TCP araliginda olmalidir.
        RuleFor(portSelector)
            .InclusiveBetween(DatabaseConnectionConsts.MinPort, DatabaseConnectionConsts.MaxPort)
            .WithMessage(DatabaseConnectionExceptionCodes.Validation.PortOutOfRange);

        // DatabaseName bos olamaz ve semadaki uzunluk sinirini asamaz.
        RuleFor(databaseNameSelector)
            .NotEmpty().WithMessage(DatabaseConnectionExceptionCodes.Validation.DatabaseNameRequired)
            .MaximumLength(DatabaseConnectionConsts.MaxDatabaseNameLength).WithMessage(DatabaseConnectionExceptionCodes.Validation.DatabaseNameMaxLength);

    }
}

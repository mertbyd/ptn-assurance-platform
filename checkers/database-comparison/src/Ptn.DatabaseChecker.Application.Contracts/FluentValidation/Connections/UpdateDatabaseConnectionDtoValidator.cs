using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Dtos.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Connections;

// islevi: DatabaseConnection update isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Adres kurallari her zaman; kimlik bilgisi opsiyonel - Password verilirse Username zorunlu, verilmezse secret'a dokunulmaz.
public class UpdateDatabaseConnectionDtoValidator : DatabaseConnectionDtoValidatorBase<UpdateDatabaseConnectionDto>
{
    public UpdateDatabaseConnectionDtoValidator()
    {
        AddRules(x => x.EngineId, x => x.Name, x => x.Host, x => x.Port, x => x.DatabaseName, x => x.TlsModeCode);

        // Parola verildiyse kullanici adi da zorunlu (secret ikisiyle birlikte yeniden yazilir).
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(DatabaseConnectionExceptionCodes.Validation.UsernameRequired)
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        // Kullanici adi da tek basina gonderilemez: secret ikili yazildigi icin parolasiz username hicbir yere islenmez.
        // Kural olmazsa istek 200 doner ama degisiklik sessizce kaybolur; cagiran ne degistigini bilemez.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(DatabaseConnectionExceptionCodes.Validation.PasswordRequiredForUsernameChange)
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        // Verilen kimlik bilgileri uzunluk sinirini asamaz (null ise kural gecerli sayilir).
        RuleFor(x => x.Username)
            .MaximumLength(DatabaseConnectionConsts.MaxUsernameLength).WithMessage(DatabaseConnectionExceptionCodes.Validation.UsernameMaxLength);

        RuleFor(x => x.Password)
            .MaximumLength(DatabaseConnectionConsts.MaxPasswordLength).WithMessage(DatabaseConnectionExceptionCodes.Validation.PasswordMaxLength);
    }
}

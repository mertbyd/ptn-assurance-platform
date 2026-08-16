using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Dtos.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;

namespace Ptn.DatabaseChecker.FluentValidation.Connections;

// islevi: DatabaseConnection create isteginin girdi-format kurallarini calistirir.
// sistemdeki gorevi: Ortak adres kurallarina ek olarak kimlik bilgisini zorunlu kilar; kimlik Vault'a yazilir, DB'de tutulmaz.
public class CreateDatabaseConnectionDtoValidator : DatabaseConnectionDtoValidatorBase<CreateDatabaseConnectionDto>
{
    public CreateDatabaseConnectionDtoValidator()
    {
        AddRules(x => x.EngineId, x => x.Name, x => x.Host, x => x.Port, x => x.DatabaseName, x => x.TlsModeCode);

        // Kullanici adi create'te zorunlu.
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(DatabaseConnectionExceptionCodes.Validation.UsernameRequired)
            .MaximumLength(DatabaseConnectionConsts.MaxUsernameLength).WithMessage(DatabaseConnectionExceptionCodes.Validation.UsernameMaxLength);

        // Sifre create'te zorunlu.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(DatabaseConnectionExceptionCodes.Validation.PasswordRequired)
            .MaximumLength(DatabaseConnectionConsts.MaxPasswordLength).WithMessage(DatabaseConnectionExceptionCodes.Validation.PasswordMaxLength);
    }
}

using System.Linq;
using FluentValidation;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Dtos.Diagnosis;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.FluentValidation.Correlation;

namespace Ptn.DatabaseChecker.FluentValidation.Diagnosis;

// islevi: Diagnose request'in baglanti, exactly-one sinyal, assertion adresi ve provider alan seklini dogrular.
// sistemdeki gorevi: Canli katalog/engine eslesmesi kurallarini manager'a birakip public input shape'ini endpoint oncesinde fail-fast korur.
public sealed class DiagnoseRequestDtoValidator : AbstractValidator<DiagnoseRequestDto>
{
    // islevi: Root exactly-one kurali ile assertion ve DB-exception alan kosullarini kaydeder.
    public DiagnoseRequestDtoValidator()
    {
        RuleFor(item => item.ConnectionId)
            .NotEmpty().WithMessage(DiagnosisExceptionCodes.Validation.ConnectionRequired);
        RuleFor(item => item)
            .Must(HasExactlyOneSignal)
            .WithMessage(DiagnosisExceptionCodes.Validation.ExactlyOneSignalRequired);
        RuleFor(item => item.Correlation!)
            .SetValidator(new CorrelationRefDtoValidator())
            .When(item => item.Correlation is not null);
        AddAssertionRules();
        AddDatabaseExceptionRules();
    }

    // islevi: Assertion sinyalinin sema, tablo, anahtar ve kararli outcome seklini kosullu dogrular.
    private void AddAssertionRules()
    {
        When(item => item.Assertion is not null, () =>
        {
            RuleFor(item => item.Assertion!.SchemaName)
                .NotEmpty().WithMessage(DiagnosisExceptionCodes.Validation.SchemaRequired)
                .MaximumLength(SchemaObjectConsts.MaxSchemaNameLength)
                .WithMessage(DiagnosisExceptionCodes.Validation.SchemaTooLong);
            RuleFor(item => item.Assertion!.TableName)
                .NotEmpty().WithMessage(DiagnosisExceptionCodes.Validation.TableRequired)
                .MaximumLength(SchemaObjectConsts.MaxObjectNameLength)
                .WithMessage(DiagnosisExceptionCodes.Validation.TableTooLong);
            RuleFor(item => item.Assertion!.KeyValues)
                .NotEmpty().WithMessage(DiagnosisExceptionCodes.Validation.KeyRequired)
                .Must(values => values.Keys.All(key => !string.IsNullOrWhiteSpace(key)));
            RuleFor(item => item.Assertion!.OutcomeCode)
                .NotEmpty().WithMessage(DiagnosisExceptionCodes.Validation.OutcomeRequired)
                .Must(FailureSourceKindCodes.IsAssertionOutcomeDefined)
                .WithMessage(DiagnosisExceptionCodes.Validation.OutcomeInvalid);
        });
    }

    // islevi: DB-exception sinyalinin destekli engine, kod ve yapilandirilmis alan adlarini kosullu dogrular.
    private void AddDatabaseExceptionRules()
    {
        When(item => item.DbException is not null, () =>
        {
            RuleFor(item => item.DbException!.EngineCode)
                .NotEmpty().WithMessage(DiagnosisExceptionCodes.Validation.EngineRequired)
                .Must(IsSupportedEngine).WithMessage(DiagnosisExceptionCodes.Validation.EngineInvalid);
            RuleFor(item => item.DbException!.SqlState)
                .NotEmpty().WithMessage(DiagnosisExceptionCodes.Validation.ErrorCodeRequired)
                .MaximumLength(FailureSourceKindCodes.MaxErrorCodeLength)
                .WithMessage(DiagnosisExceptionCodes.Validation.ErrorCodeInvalid);
            RuleFor(item => item.DbException!.ProviderFields)
                .Must(HasBoundedProviderFields)
                .WithMessage(DiagnosisExceptionCodes.Validation.ProviderFieldsInvalid);
        });
    }

    // islevi: Assertion ve DB-exception nesnelerinden yalniz birinin dolu oldugunu bildirir.
    private static bool HasExactlyOneSignal(DiagnoseRequestDto input)
        => (input.Assertion is null) != (input.DbException is null);

    // islevi: Public engine kodunun mevcut PostgreSQL veya SQL Server kapali kumesinde oldugunu bildirir.
    private static bool IsSupportedEngine(string engineCode)
        => engineCode is DatabaseEngineCodes.PostgreSql or DatabaseEngineCodes.SqlServer;

    // islevi: Provider alan sozlugunun yalniz kucuk ve sinirli ad/degerlerden olustugunu bildirir.
    private static bool HasBoundedProviderFields(
        System.Collections.Generic.Dictionary<string, string?> fields)
        => fields.Count <= FailureSourceKindCodes.MaxProviderFieldCount &&
           fields.All(pair => !string.IsNullOrWhiteSpace(pair.Key) &&
                              pair.Key.Length <= FailureSourceKindCodes.MaxProviderFieldNameLength &&
                              (pair.Value?.Length ?? 0) <= FailureSourceKindCodes.MaxProviderFieldValueLength);
}

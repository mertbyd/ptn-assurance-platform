using FluentValidation;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Constants.Snapshots.Lookups;
using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.ExceptionCodes.Snapshots;

namespace Ptn.ApiContractChecker.FluentValidation.Snapshots;

// islevi: Envanter sorgusunun sayfa penceresini, kapali metot kodunu ve path onek uzunlugunu dogrular.
// sistemdeki gorevi: Manager'a yalniz sekli gecerli ve sinirli envanter sorgusunun ulasmasini saglar.
public class ListSnapshotOperationsInputValidator : AbstractValidator<ListSnapshotOperationsInput>
{
    public ListSnapshotOperationsInputValidator()
    {
        RuleFor(input => input.SkipCount).GreaterThanOrEqualTo(0);
        RuleFor(input => input.MaxResultCount).GreaterThan(0);
        RuleFor(input => input.MethodCode)
            .Must(value => value is null || SpecOperationMethodCodes.All.Contains(value))
            .WithMessage(SpecSnapshotExceptionCodes.Validation.OperationMethodCodeInvalid);
        RuleFor(input => input.PathPrefix)
            .MaximumLength(SnapshotOperationInventoryConsts.MaxPathPrefixLength)
            .WithMessage(SpecSnapshotExceptionCodes.Validation.OperationPathPrefixMaxLength);
    }
}

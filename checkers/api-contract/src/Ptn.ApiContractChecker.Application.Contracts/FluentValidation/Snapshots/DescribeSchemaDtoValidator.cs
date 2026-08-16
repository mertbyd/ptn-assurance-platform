using FluentValidation;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Constants.Snapshots.Lookups;
using Ptn.ApiContractChecker.Dtos.Snapshots;

namespace Ptn.ApiContractChecker.FluentValidation.Snapshots;

// islevi: schema.describe girdisinin kimlik, ref uzunlugu ve verbosity kodunu dogrular.
public class DescribeSchemaDtoValidator : AbstractValidator<DescribeSchemaDto>
{
    public DescribeSchemaDtoValidator()
    {
        RuleFor(input => input.SnapshotId).NotEmpty();
        RuleFor(input => input.SchemaRef).NotEmpty()
            .MaximumLength(SnapshotAuthoringConstants.MaxSchemaReferenceLength);
        RuleFor(input => input.VerbosityCode).Must(SnapshotVerbosityCodes.All.Contains);
    }
}

using Ptn.ApiContractChecker.Constants.Snapshots.Lookups;

namespace Ptn.ApiContractChecker.Dtos.Snapshots;

// islevi: Tek snapshot component semasi ve verbosity secimini tasir.
// sistemdeki gorevi: schema.describe public input'unu tam spec govdesinden ayirir.
public class DescribeSchemaDto
{
    public Guid SnapshotId { get; set; }
    public string SchemaRef { get; set; } = string.Empty;
    public string VerbosityCode { get; set; } = SnapshotVerbosityCodes.Minimal;
}

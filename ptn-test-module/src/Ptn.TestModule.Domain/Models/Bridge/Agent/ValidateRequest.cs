using System;
using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge.Database;
using Ptn.TestModule.Models.Catalog;
namespace Ptn.TestModule.Models.Bridge.Agent;
// islevi: Yayinlanabilirlik yoklamasinin kaynak belge, malzeme muhru ve kapali referanslarini tasir.
// sistemdeki gorevi: Derleme kanitini tasirken assertion pointer veya operasyon adresi uydurulmasini engeller.
public sealed class ValidateRequest
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid ConnectionId { get; set; }
    public Guid SpecSnapshotId { get; set; }
    public Guid OperationReferenceId { get; set; }
    public List<Guid> AssertionReferenceIds { get; set; } = [];
    public string? SourceDocument { get; set; }
    public TestScenarioMaterialSeal? MaterialSeal { get; set; }
    public List<DatabaseDerivabilityAddress> DatabaseAssertions { get; set; } = [];
    public string ResponseFormat { get; set; } = string.Empty;
}

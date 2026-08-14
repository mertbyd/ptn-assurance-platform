using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Yayin anindaki sema, turetilebilirlik ve sourceDescriptions kanitini tasir.
// sistemdeki gorevi: Bes yayin kapisinin calismasi icin gereken anlik makine sonucunu kabul eder.
public sealed class PublishTestScenarioDto
{
    public bool IsSchemaValid { get; set; }
    public bool AreAssertionsDerivable { get; set; }
    public List<Guid> SourceDescriptionSpecSnapshotIds { get; set; } = [];
}

using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Catalog;

// islevi: Yayin aninda makine kapilarindan gelen sema, turetilebilirlik ve sourceDescriptions kanitini tasir.
// sistemdeki gorevi: Gate Manager'in public DTO veya checker tipine baglanmadan bes kapinin tamamini degerlendirmesini saglar.
public sealed class TestScenarioPublishModel
{
    public bool IsSchemaValid { get; set; }
    public bool AreAssertionsDerivable { get; set; }
    public List<Guid> SourceDescriptionSpecSnapshotIds { get; set; } = [];
}

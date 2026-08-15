using System.Collections.Generic;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Riok.Mapperly.Abstractions;
using CheckerSchemaColumnDto = Ptn.DatabaseChecker.Dtos.SchemaDiscovery.SchemaColumnDto;
using CheckerSchemaLintWarningDto = Ptn.DatabaseChecker.Dtos.SchemaDiscovery.SchemaLintWarningDto;
using CheckerSchemaSnapshotDto = Ptn.DatabaseChecker.Dtos.SchemaDiscovery.SchemaSnapshotDto;
using CheckerSchemaTableDto = Ptn.DatabaseChecker.Dtos.SchemaDiscovery.SchemaTableDto;
using CheckerForeignKeyNeighborDto = Ptn.DatabaseChecker.Dtos.SchemaDiscovery.ForeignKeyNeighborDto;
using CheckerTableColumnDto = Ptn.DatabaseChecker.Dtos.SchemaDiscovery.TableDescriptionColumnDto;
using CheckerTableDescriptionDto = Ptn.DatabaseChecker.Dtos.SchemaDiscovery.TableDescriptionDto;
using CheckerTableKeyDto = Ptn.DatabaseChecker.Dtos.SchemaDiscovery.TableKeyDefinitionDto;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Database checker sema DTO'larini provider-bagimsiz Bridge modellerine compile-time esler.
// sistemdeki gorevi: Application servisini elle alan kopyalamadan iki tipli esleme cagrisi yapar hale getirir.
[Mapper]
public partial class SchemaKnowledgeMapper
{
    public partial TableQuery Map(TableQueryDto input);
    public partial TableQueryDto Map(TableQuery input);
    public partial TableDescriptionDto Map(TableDescription input);
    public partial SchemaSnapshotDto Map(SchemaSnapshot input);
    public partial CheckerTableDescription Map(CheckerTableDescriptionDto input);
    public partial CheckerForeignKeyNeighbor MapForeignKeyNeighbor(CheckerForeignKeyNeighborDto input);
    public partial ForeignKeyNeighborDto MapForeignKeyNeighbor(ForeignKeyNeighbor input);
    public partial CheckerSchemaLintWarning MapSchemaLintWarning(CheckerSchemaLintWarningDto input);
    public partial SchemaLintWarningDto MapSchemaLintWarning(SchemaLintWarning input);
    public partial SchemaLintWarning MapSchemaLintWarning(SchemaLintWarningDto input);
    public partial List<SchemaLintWarning> MapLintWarnings(List<SchemaLintWarningDto> input);
    public partial TableColumn MapColumn(CheckerTableColumnDto input);
    public partial TableKey MapKey(CheckerTableKeyDto input);
    public partial SchemaSnapshot Map(CheckerSchemaSnapshotDto input);
    public partial SchemaTable MapSchemaTable(CheckerSchemaTableDto input);
    public partial SchemaColumn MapSchemaColumn(CheckerSchemaColumnDto input);
}

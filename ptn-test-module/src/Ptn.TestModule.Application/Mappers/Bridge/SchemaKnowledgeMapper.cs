using Ptn.DatabaseChecker.Dtos.SchemaDiscovery;
using Ptn.TestModule.Models.Bridge;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Bridge;

// islevi: Database checker sema DTO'larini provider-bagimsiz Bridge modellerine compile-time esler.
// sistemdeki gorevi: Application servisini elle alan kopyalamadan iki tipli esleme cagrisi yapar hale getirir.
[Mapper]
public partial class SchemaKnowledgeMapper
{
    public partial PtnTableDescription Map(TableDescriptionDto input);
    private partial PtnTableColumn MapColumn(TableDescriptionColumnDto input);
    private partial PtnTableKey MapKey(TableKeyDefinitionDto input);
    public partial PtnSchemaSnapshot Map(SchemaSnapshotDto input);
    private partial PtnSchemaTable MapSchemaTable(SchemaTableDto input);
    private partial PtnSchemaColumn MapSchemaColumn(SchemaColumnDto input);
}

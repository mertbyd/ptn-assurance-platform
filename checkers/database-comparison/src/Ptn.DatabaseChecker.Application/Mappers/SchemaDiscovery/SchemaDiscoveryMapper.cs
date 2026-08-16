using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.SchemaDiscovery;
using Ptn.DatabaseChecker.Models.Comparison;
using Riok.Mapperly.Abstractions;

namespace Ptn.DatabaseChecker.Application.Mappers.SchemaDiscovery;

// islevi: Sema kesfi (schemas/objects/snapshot) domain modellerinin API cevap DTO donusumlerini Mapperly ile uretir.
// sistemdeki gorevi: Kesif okuma sonuclarini katmanlar arasi elle kopyalamadan tek noktada DTO'ya tasir.
[Mapper]
public partial class SchemaDiscoveryMapper
{
    // islevi: Kesif akisinda okunan sema listesini API cevap DTO listesine tasir.
    public partial List<DatabaseSchemaDto> MapToSchemaDtos(List<DatabaseSchemaModel> schemas);

    // islevi: Kesif akisinda okunan sema nesne listesini API cevap DTO listesine tasir.
    public partial List<DatabaseSchemaObjectDto> MapToObjectDtos(List<DatabaseSchemaObjectModel> objects);

    // islevi: Okuyucunun urettigi tam sema fotografini (tablo + kolonlar) API cevap DTO'suna tasir.
    public partial SchemaSnapshotDto MapToSnapshotDto(SchemaSnapshotModel snapshot);

    // islevi: Assertion odakli tek tablo tanimlama modelini API cevap DTO'suna tasir.
    public partial TableDescriptionDto MapToTableDescriptionDto(TableDescriptionModel description);

    // islevi: Kanonik sema muhrunu ve dal muhurlerini API cevap DTO'suna tasir.
    public partial SchemaFingerprintDto MapToFingerprintDto(SchemaFingerprintModel fingerprint);

    private partial SchemaFingerprintEntryDto MapFingerprintEntry(SchemaFingerprintEntryModel entry);

    private partial SchemaTableDto MapTable(SchemaTableModel table);

    private partial SchemaColumnDto MapColumn(SchemaColumnModel column);

    private partial SchemaIndexDto MapIndex(SchemaIndexModel index);

    private partial SchemaConstraintDto MapConstraint(SchemaConstraintModel constraint);

    private partial SchemaTriggerDto MapTrigger(SchemaTriggerModel trigger);

    private partial SchemaObjectDefinitionDto MapObjectDefinition(SchemaObjectDefinitionModel definition);

    private partial SchemaLintWarningDto MapLintWarning(SchemaLintWarningModel warning);
}

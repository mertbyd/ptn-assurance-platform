using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Comparison.Scope;
using Volo.Abp.Domain.Services;

// Definition alan adlari ve change-summary etiketleri Domain.Shared'daki SchemaComparisonTextConstants altinda toplanir; cagri yerleri okunur kalsin diye alias'lanir.
using DefinitionFields = Ptn.DatabaseChecker.Constants.Comparison.SchemaComparisonTextConstants.DefinitionFields;
using ChangeLabels = Ptn.DatabaseChecker.Constants.Comparison.SchemaComparisonTextConstants.ChangeLabels;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: Iki schema snapshot'ini scope kurallariyla karsilastirip owned-JSON findings modeli uretir.
// sistemdeki gorevi: T6 reader -> normalize -> compare -> finding zincirinin domain orkestrasyonudur; repository katalog detaylarini bilmez, AppService/Controller katmanina cikmaz.
public class SchemaComparisonManager : DomainService
{
    // Scope include/exclude/ignore kurallarini tek merkezden yorumlayan servis.
    private readonly ComparisonScopeRuleEvaluator _scopeRuleEvaluator;

    // Source/target koleksiyonlarindaki yon ve modified farklarini generic ureten servis.
    private readonly SchemaCollectionComparer _collectionComparer;

    // Kolon/index/constraint alanlarindaki expression ve identifier normalizasyonunu yapan servis.
    private readonly SchemaDefinitionNormalizer _normalizer;

    // Kolon tip ciftinin motor ve fidelity bilgisine gore bulgu guvenini cozen saf servis.
    private readonly ColumnTypeConfidenceResolver _columnTypeConfidenceResolver;

    // islevi: Schema compare orkestrasyonuna scope, koleksiyon, normalizasyon ve kolon guven servislerini baglar.
    public SchemaComparisonManager(
        ComparisonScopeRuleEvaluator scopeRuleEvaluator,
        SchemaCollectionComparer collectionComparer,
        SchemaDefinitionNormalizer normalizer)
        : this(scopeRuleEvaluator, collectionComparer, normalizer, new ColumnTypeConfidenceResolver())
    {
    }

    public SchemaComparisonManager(
        ComparisonScopeRuleEvaluator scopeRuleEvaluator,
        SchemaCollectionComparer collectionComparer,
        SchemaDefinitionNormalizer normalizer,
        ColumnTypeConfidenceResolver columnTypeConfidenceResolver)
    {
        _scopeRuleEvaluator = scopeRuleEvaluator;
        _collectionComparer = collectionComparer;
        _normalizer = normalizer;
        _columnTypeConfidenceResolver = columnTypeConfidenceResolver;
    }

    // islevi: Iki snapshot'tan sema bulgularini uretir; veri/migration bulgulari T7/T8 islerinde ayrica doldurulur.
    public ComparisonFindings Compare(
        SchemaSnapshotModel sourceSnapshot,
        SchemaSnapshotModel targetSnapshot,
        List<ComparisonScopeRule> scopeRules)
    {
        var nonColumnConfidenceCode = GetNonColumnConfidenceCode(sourceSnapshot, targetSnapshot);
        var sourceTables = _scopeRuleEvaluator.FilterComparableTables(sourceSnapshot.Tables, scopeRules);
        var targetTables = _scopeRuleEvaluator.FilterComparableTables(targetSnapshot.Tables, scopeRules);
        var sourceObjects = _scopeRuleEvaluator.FilterComparableObjects(sourceSnapshot.Objects, scopeRules);
        var targetObjects = _scopeRuleEvaluator.FilterComparableObjects(targetSnapshot.Objects, scopeRules);
        var findings = new ComparisonFindings();
        findings.SchemaDifferences.AddRange(CompareDatabaseMetadata(sourceSnapshot, targetSnapshot));
        findings.SchemaDifferences.AddRange(CompareTables(sourceTables, targetTables, nonColumnConfidenceCode));
        findings.SchemaDifferences.AddRange(CompareSharedTableChildren(
            sourceTables,
            targetTables,
            scopeRules,
            sourceSnapshot.EngineCode,
            targetSnapshot.EngineCode,
            sourceSnapshot.DatabaseCollationName,
            targetSnapshot.DatabaseCollationName,
            nonColumnConfidenceCode));
        findings.SchemaDifferences.AddRange(CompareSchemaObjects(sourceObjects, targetObjects, nonColumnConfidenceCode));
        findings.SchemaDifferences = SortDifferences(findings.SchemaDifferences);
        return findings;
    }

    // islevi: Veritabani collation ve desteklenen provider metadata farkini tek Database bulgusu olarak uretir.
    private List<SchemaDifferenceModel> CompareDatabaseMetadata(
        SchemaSnapshotModel sourceSnapshot,
        SchemaSnapshotModel targetSnapshot)
    {
        var isSameEngine = IsSameEngine(sourceSnapshot.EngineCode, targetSnapshot.EngineCode);
        var sourceDefinition = BuildDatabaseDefinition(sourceSnapshot, isSameEngine);
        var targetDefinition = BuildDatabaseDefinition(targetSnapshot, isSameEngine);
        if (string.Equals(sourceDefinition, targetDefinition, StringComparison.Ordinal))
        {
            return new List<SchemaDifferenceModel>();
        }

        return new List<SchemaDifferenceModel>
        {
            SchemaDifferenceFactory.Modified(
                BuildDatabaseAddress(),
                sourceDefinition,
                targetDefinition,
                BuildDatabaseChangeSummary(sourceSnapshot, targetSnapshot, isSameEngine),
                GetNonColumnConfidenceCode(sourceSnapshot, targetSnapshot))
        };
    }

    // islevi: Tablo var/yok farklarini uretir.
    private List<SchemaDifferenceModel> CompareTables(
        List<SchemaTableModel> sourceTables,
        List<SchemaTableModel> targetTables,
        string confidenceCode)
    {
        return _collectionComparer.Compare(
            sourceTables,
            targetTables,
            BuildTableKey,
            table => BuildTableAddress(table),
            BuildTableDefinition,
            (_, _) => null,
            (_, _) => confidenceCode);
    }

    // islevi: Iki tarafta da bulunan tablolarin kolon/index/constraint/trigger alt farklarini uretir; kapsam kurallari child seviyesinde de uygulanir.
    private List<SchemaDifferenceModel> CompareSharedTableChildren(
        List<SchemaTableModel> sourceTables,
        List<SchemaTableModel> targetTables,
        List<ComparisonScopeRule> scopeRules,
        string sourceEngineCode,
        string targetEngineCode,
        string? sourceDatabaseCollation,
        string? targetDatabaseCollation,
        string nonColumnConfidenceCode)
    {
        var differences = new List<SchemaDifferenceModel>();
        foreach (var (sourceTable, targetTable) in _collectionComparer.EnumerateMatched(sourceTables, targetTables, BuildTableKey))
        {
            differences.AddRange(CompareColumns(
                sourceTable,
                targetTable,
                scopeRules,
                sourceEngineCode,
                targetEngineCode,
                sourceDatabaseCollation,
                targetDatabaseCollation));
            differences.AddRange(CompareColumnOrder(
                sourceTable,
                targetTable,
                scopeRules,
                sourceEngineCode,
                targetEngineCode,
                sourceDatabaseCollation,
                targetDatabaseCollation));
            differences.AddRange(CompareIndexes(sourceTable, targetTable, scopeRules, nonColumnConfidenceCode));
            differences.AddRange(CompareConstraints(
                sourceTable,
                targetTable,
                scopeRules,
                nonColumnConfidenceCode,
                IsSameEngine(sourceEngineCode, targetEngineCode)));
            differences.AddRange(CompareTriggers(sourceTable, targetTable, scopeRules, nonColumnConfidenceCode));
        }
        return differences;
    }

    // islevi: View/function/procedure/sequence/type/extension gibi tablo disi nesne farklarini uretir.
    private List<SchemaDifferenceModel> CompareSchemaObjects(
        List<SchemaObjectDefinitionModel> sourceObjects,
        List<SchemaObjectDefinitionModel> targetObjects,
        string confidenceCode)
    {
        return _collectionComparer.Compare(
            sourceObjects,
            targetObjects,
            BuildSchemaObjectKey,
            BuildSchemaObjectAddress,
            schemaObject => schemaObject.Definition,
            (_, _) => ChangeLabels.Definition,
            (_, _) => confidenceCode);
    }

    // islevi: Ortak tablodaki kolon var/yok/tanim farklarini uretir; child kapsam kurallari kolonlari daraltir.
    private List<SchemaDifferenceModel> CompareColumns(
        SchemaTableModel sourceTable,
        SchemaTableModel targetTable,
        List<ComparisonScopeRule> scopeRules,
        string sourceEngineCode,
        string targetEngineCode,
        string? sourceDatabaseCollation,
        string? targetDatabaseCollation)
    {
        var isSameEngine = IsSameEngine(sourceEngineCode, targetEngineCode);
        var sourceColumns = FilterChildren(sourceTable, sourceTable.Columns, column => column.Name, scopeRules)
            .Select(column => (Column: column, DatabaseCollation: sourceDatabaseCollation));
        var targetColumns = FilterChildren(targetTable, targetTable.Columns, column => column.Name, scopeRules)
            .Select(column => (Column: column, DatabaseCollation: targetDatabaseCollation));
        return _collectionComparer.Compare(
            sourceColumns,
            targetColumns,
            item => _normalizer.NormalizeIdentifier(item.Column.Name),
            item => BuildChildAddress(sourceTable, item.Column.Name, SchemaObjectTypeCodes.Column),
            item => BuildColumnDefinition(item.Column, isSameEngine, item.DatabaseCollation),
            (sourceColumn, targetColumn) => BuildColumnChangeSummary(
                sourceColumn.Column,
                targetColumn.Column,
                isSameEngine,
                sourceColumn.DatabaseCollation,
                targetColumn.DatabaseCollation),
            (sourceColumn, targetColumn) => _columnTypeConfidenceResolver.Resolve(
                sourceEngineCode,
                targetEngineCode,
                sourceColumn.Column,
                targetColumn.Column));
    }

    // islevi: Iki tarafta da bulunan kolonlarin GORELI sirasini karsilastirir; yalniz gercek yer degisimini Modified(Ordinal) olarak raporlar.
    // sistemdeki gorevi: Ham Ordinal (attnum/column_id) DB'ler arasi bosluk tasidigindan dogrudan kiyaslanamaz; olcut ortak kolonlarin bagil siralamasidir, boylece kolon ekleme/silme false-positive uretmez. Tanimi da degisen kolonlar zaten CompareColumns'ta raporlandigi icin burada atlanir (cift bulgu olmaz).
    private List<SchemaDifferenceModel> CompareColumnOrder(
        SchemaTableModel sourceTable,
        SchemaTableModel targetTable,
        List<ComparisonScopeRule> scopeRules,
        string sourceEngineCode,
        string targetEngineCode,
        string? sourceDatabaseCollation,
        string? targetDatabaseCollation)
    {
        var isSameEngine = IsSameEngine(sourceEngineCode, targetEngineCode);
        var sourceColumns = FilterChildren(sourceTable, sourceTable.Columns, column => column.Name, scopeRules);
        var targetColumns = FilterChildren(targetTable, targetTable.Columns, column => column.Name, scopeRules);
        var sourceByName = BuildColumnByName(sourceColumns);
        var targetByName = BuildColumnByName(targetColumns);

        // Her iki tarafta da olan kolonlarin, kendi tablosundaki gorunum sirasindaki bagil konumu.
        var sharedSourceOrder = sourceColumns
            .Where(column => targetByName.ContainsKey(_normalizer.NormalizeIdentifier(column.Name)))
            .ToList();
        var targetPositionByName = targetColumns
            .Where(column => sourceByName.ContainsKey(_normalizer.NormalizeIdentifier(column.Name)))
            .Select((column, position) => (Name: _normalizer.NormalizeIdentifier(column.Name), Position: position))
            .ToDictionary(item => item.Name, item => item.Position, StringComparer.OrdinalIgnoreCase);

        var differences = new List<SchemaDifferenceModel>();
        for (var sourcePosition = 0; sourcePosition < sharedSourceOrder.Count; sourcePosition++)
        {
            var sourceColumn = sharedSourceOrder[sourcePosition];
            var normalizedName = _normalizer.NormalizeIdentifier(sourceColumn.Name);
            if (targetPositionByName[normalizedName] == sourcePosition)
            {
                continue;
            }

            var targetColumn = targetByName[normalizedName];
            var confidenceCode = _columnTypeConfidenceResolver.Resolve(
                sourceEngineCode,
                targetEngineCode,
                sourceColumn,
                targetColumn);
            if (string.Equals(
                    confidenceCode,
                    ComparisonConfidenceCodes.Incomparable,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var sourceDefinition = BuildColumnDefinition(sourceColumn, isSameEngine, sourceDatabaseCollation);
            if (!string.Equals(
                    sourceDefinition,
                    BuildColumnDefinition(targetColumn, isSameEngine, targetDatabaseCollation),
                    StringComparison.Ordinal))
            {
                continue;
            }

            differences.Add(SchemaDifferenceFactory.Modified(
                BuildChildAddress(sourceTable, sourceColumn.Name, SchemaObjectTypeCodes.Column),
                sourceDefinition,
                BuildColumnDefinition(targetColumn, isSameEngine, targetDatabaseCollation),
                ChangeLabels.Ordinal,
                confidenceCode));
        }

        return differences;
    }

    // islevi: Kolon listesini normalize adiyla case-insensitive indexler; duplicate katalog satirinda ilk kayit korunur.
    private Dictionary<string, SchemaColumnModel> BuildColumnByName(List<SchemaColumnModel> columns)
        => columns
            .GroupBy(column => _normalizer.NormalizeIdentifier(column.Name), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

    // islevi: Ortak tablodaki index var/yok/tanim farklarini uretir; child kapsam kurallari index'leri daraltir.
    private List<SchemaDifferenceModel> CompareIndexes(
        SchemaTableModel sourceTable,
        SchemaTableModel targetTable,
        List<ComparisonScopeRule> scopeRules,
        string confidenceCode)
    {
        return _collectionComparer.CompareWithUniqueFallback(
            FilterChildren(sourceTable, sourceTable.Indexes, index => index.Name, scopeRules),
            FilterChildren(targetTable, targetTable.Indexes, index => index.Name, scopeRules),
            index => _normalizer.NormalizeIdentifier(index.Name),
            BuildIndexDefinition,
            index => BuildChildAddress(sourceTable, index.Name, SchemaObjectTypeCodes.Index),
            BuildIndexDefinition,
            BuildIndexChangeSummary,
            (_, _) => confidenceCode);
    }

    // islevi: Ortak tablodaki PK/unique/FK/check constraint var/yok/tanim farklarini uretir; child kapsam kurallari constraint'leri daraltir.
    private List<SchemaDifferenceModel> CompareConstraints(
        SchemaTableModel sourceTable,
        SchemaTableModel targetTable,
        List<ComparisonScopeRule> scopeRules,
        string confidenceCode,
        bool isSameEngine)
    {
        return _collectionComparer.Compare(
            FilterChildren(sourceTable, sourceTable.Constraints, constraint => constraint.Name, scopeRules),
            FilterChildren(targetTable, targetTable.Constraints, constraint => constraint.Name, scopeRules),
            constraint => BuildConstraintKey(constraint),
            constraint => BuildChildAddress(sourceTable, constraint.Name, MapConstraintObjectType(constraint.TypeCode)),
            constraint => BuildConstraintDefinition(constraint, isSameEngine),
            (source, target) => BuildConstraintChangeSummary(source, target, isSameEngine),
            (_, _) => confidenceCode);
    }

    // islevi: Ortak tablodaki trigger var/yok/tanim farklarini uretir; child kapsam kurallari trigger'lari daraltir.
    private List<SchemaDifferenceModel> CompareTriggers(
        SchemaTableModel sourceTable,
        SchemaTableModel targetTable,
        List<ComparisonScopeRule> scopeRules,
        string confidenceCode)
    {
        return _collectionComparer.Compare(
            FilterChildren(sourceTable, sourceTable.Triggers, trigger => trigger.Name, scopeRules),
            FilterChildren(targetTable, targetTable.Triggers, trigger => trigger.Name, scopeRules),
            trigger => _normalizer.NormalizeIdentifier(trigger.Name),
            trigger => BuildChildAddress(sourceTable, trigger.Name, SchemaObjectTypeCodes.Trigger),
            BuildTriggerDefinition,
            BuildTriggerChangeSummary,
            (_, _) => confidenceCode);
    }

    // islevi: Tablo alt nesne listesini tablonun sema/adiyla scope evaluator'a delege ederek child kapsam kurallarina gore daraltir.
    private List<TChild> FilterChildren<TChild>(
        SchemaTableModel table,
        List<TChild> children,
        Func<TChild, string> childNameSelector,
        List<ComparisonScopeRule> scopeRules)
        => _scopeRuleEvaluator.FilterComparableChildren(children, table.Schema, table.Name, childNameSelector, scopeRules);

    // islevi: Kolonun karsilastirilabilir kanonik tanim metnini uretir.
    private string BuildColumnDefinition(
        SchemaColumnModel column,
        bool isSameEngine,
        string? databaseCollation)
    {
        var fields = BuildColumnBaseDefinitionFields(column, isSameEngine);
        fields.AddRange(BuildColumnDepthDefinitionFields(column, isSameEngine, databaseCollation));
        return BuildDefinition(fields.ToArray());
    }

    // islevi: Kolonun mevcut tip/null/boyut/identity/default alanlarini definition field listesine cevirir.
    private List<(string FieldName, string FieldValue)> BuildColumnBaseDefinitionFields(
        SchemaColumnModel column,
        bool isSameEngine)
    {
        return new List<(string FieldName, string FieldValue)>
        {
            DefinitionField(DefinitionFields.Type, GetComparableDataType(column, isSameEngine)),
            DefinitionField(DefinitionFields.CanonicalType, column.CanonicalDataType),
            DefinitionField(DefinitionFields.Nullable, column.IsNullable),
            DefinitionField(DefinitionFields.MaxLength, column.MaxLength),
            DefinitionField(DefinitionFields.NumericPrecision, column.NumericPrecision),
            DefinitionField(DefinitionFields.NumericScale, column.NumericScale),
            DefinitionField(DefinitionFields.Identity, column.IsIdentity),
            DefinitionField(
                DefinitionFields.Default,
                _normalizer.NormalizeExpression(column.DefaultValueSql))
        };
    }

    // islevi: Kolonun collation/generated/identity-sequence/comment derinlik alanlarini definition field listesine cevirir.
    private List<(string FieldName, string FieldValue)> BuildColumnDepthDefinitionFields(
        SchemaColumnModel column,
        bool isSameEngine,
        string? databaseCollation)
    {
        var fields = new List<(string FieldName, string FieldValue)>
        {
            DefinitionField(DefinitionFields.Collation, _normalizer.NormalizeColumnCollation(column.CollationName, databaseCollation)),
            DefinitionField(DefinitionFields.Generated, column.IsGenerated),
            DefinitionField(DefinitionFields.GenerationExpression, _normalizer.NormalizeExpression(column.GenerationExpression)),
            DefinitionField(DefinitionFields.IdentitySeed, column.IdentitySeed),
            DefinitionField(DefinitionFields.IdentityIncrement, column.IdentityIncrement),
            DefinitionField(DefinitionFields.Comment, column.Comment)
        };
        if (isSameEngine)
        {
            fields.Add(DefinitionField(DefinitionFields.Persisted, column.IsPersisted));
        }

        return fields;
    }

    // islevi: Index'in adindan bagimsiz yapisal tanimini uretir.
    private string BuildIndexDefinition(SchemaIndexModel index)
    {
        return BuildDefinition(
            DefinitionField(DefinitionFields.Unique, index.IsUnique),
            DefinitionField(DefinitionFields.PrimaryKey, index.IsPrimaryKey),
            DefinitionField(
                DefinitionFields.Columns,
                _normalizer.NormalizeNameList(index.Columns, sort: false)),
            DefinitionField(
                DefinitionFields.IncludedColumns,
                _normalizer.NormalizeNameList(index.IncludedColumns, sort: true)),
            DefinitionField(
                DefinitionFields.Filter,
                _normalizer.NormalizeExpression(index.FilterDefinition)),
            DefinitionField(
                DefinitionFields.Definition,
                BuildIndexProviderDetailDefinition(index)));
    }

    // islevi: Kolon listesine ayrisamayan expression index'lerde provider taniminin nesne adindan bagimsiz semantik kismini korur.
    // sistemdeki gorevi: Normal kolon indexlerinde redundant pg_get_indexdef metnini kiyaslamaz; expression index degisikligini kacirmadan rename/casing yalanci farkini eler.
    private string BuildIndexProviderDetailDefinition(SchemaIndexModel index)
    {
        if (index.Columns.Count > 0)
        {
            return string.Empty;
        }

        var definition = _normalizer.NormalizeDefinition(index.Definition);
        var indexKeywordPosition = definition.IndexOf(
            SchemaComparisonTextConstants.IndexDefinitionParsing.IndexKeyword,
            StringComparison.OrdinalIgnoreCase);
        if (indexKeywordPosition < 0)
        {
            return definition;
        }

        var nameStartPosition =
            indexKeywordPosition + SchemaComparisonTextConstants.IndexDefinitionParsing.IndexKeyword.Length;
        var onKeywordPosition = definition.IndexOf(
            SchemaComparisonTextConstants.IndexDefinitionParsing.OnKeyword,
            nameStartPosition,
            StringComparison.OrdinalIgnoreCase);
        if (onKeywordPosition < 0)
        {
            return definition;
        }

        return string.Concat(
            definition.AsSpan(0, nameStartPosition),
            SchemaComparisonTextConstants.IndexDefinitionParsing.NamePlaceholder,
            definition.AsSpan(onKeywordPosition));
    }

    // islevi: Constraint'in tur/kolon/hedef/aksiyon/tanim alanlarini tek karsilastirma metnine cevirir.
    private string BuildConstraintDefinition(SchemaConstraintModel constraint, bool isSameEngine)
    {
        var fields = BuildConstraintRelationshipDefinitionFields(constraint);
        AddConstraintStateDefinitionFields(fields, constraint, isSameEngine);
        return BuildDefinition(fields.ToArray());
    }

    // islevi: Constraint tur/kolon/hedef/aksiyon/tanim alanlarini definition field listesine cevirir.
    private List<(string FieldName, string FieldValue)> BuildConstraintRelationshipDefinitionFields(
        SchemaConstraintModel constraint)
    {
        return new List<(string FieldName, string FieldValue)>
        {
            DefinitionField(DefinitionFields.Type, constraint.TypeCode),
            DefinitionField(
                DefinitionFields.Columns,
                _normalizer.NormalizeNameList(constraint.Columns, sort: false)),
            DefinitionField(
                DefinitionFields.ReferencedTable,
                _normalizer.NormalizeIdentifier(constraint.ReferencedTable)),
            DefinitionField(
                DefinitionFields.ReferencedColumns,
                _normalizer.NormalizeNameList(constraint.ReferencedColumns, sort: false)),
            DefinitionField(DefinitionFields.DeleteAction, constraint.DeleteActionCode),
            DefinitionField(DefinitionFields.UpdateAction, constraint.UpdateActionCode),
            DefinitionField(
                DefinitionFields.Definition,
                _normalizer.NormalizeExpression(constraint.Definition))
        };
    }

    // islevi: Constraint trust/etkinlik ve desteklenen erteleme alanlarini definition field listesine ekler.
    private static void AddConstraintStateDefinitionFields(
        List<(string FieldName, string FieldValue)> fields,
        SchemaConstraintModel constraint,
        bool isSameEngine)
    {
        fields.Add(DefinitionField(DefinitionFields.Validated, constraint.IsValidated));
        fields.Add(DefinitionField(DefinitionFields.Enabled, constraint.IsEnabled));
        if (isSameEngine)
        {
            fields.Add(DefinitionField(DefinitionFields.Deferrable, constraint.IsDeferrable));
            fields.Add(DefinitionField(DefinitionFields.InitiallyDeferred, constraint.IsInitiallyDeferred));
        }
    }

    // islevi: Trigger definition ve etkinlik durumunu tek kanonik tanim metnine cevirir.
    private string BuildTriggerDefinition(SchemaTriggerModel trigger)
        => BuildDefinition(
            DefinitionField(DefinitionFields.Definition, _normalizer.NormalizeDefinition(trigger.Definition)),
            DefinitionField(DefinitionFields.Enabled, trigger.IsEnabled));

    // islevi: Veritabani collation metadata'sini motor destek sinirina gore tek kanonik tanim metnine cevirir.
    private static string BuildDatabaseDefinition(SchemaSnapshotModel snapshot, bool isSameEngine)
    {
        var fields = new List<(string FieldName, string FieldValue)>
        {
            DefinitionField(DefinitionFields.DatabaseCollation, snapshot.DatabaseCollationName)
        };
        if (isSameEngine)
        {
            fields.Add(DefinitionField(DefinitionFields.CollationProvider, snapshot.CollationProviderCode));
        }

        return BuildDefinition(fields.ToArray());
    }

    // islevi: Kolon modified bulgusunda hangi alanlarin degistigini kisa ozetler.
    private string? BuildColumnChangeSummary(
        SchemaColumnModel sourceColumn,
        SchemaColumnModel targetColumn,
        bool isSameEngine,
        string? sourceDatabaseCollation,
        string? targetDatabaseCollation)
    {
        var changes = new List<string>();
        AddColumnBaseChanges(changes, sourceColumn, targetColumn, isSameEngine);
        AddColumnDepthChanges(
            changes,
            sourceColumn,
            targetColumn,
            isSameEngine,
            sourceDatabaseCollation,
            targetDatabaseCollation);
        return BuildChangeSummary(changes);
    }

    // islevi: Kolonun tip/null/boyut/identity/default degisikliklerini summary listesine ekler.
    private void AddColumnBaseChanges(
        List<string> changes,
        SchemaColumnModel sourceColumn,
        SchemaColumnModel targetColumn,
        bool isSameEngine)
    {
        AddColumnTypeShapeChanges(changes, sourceColumn, targetColumn, isSameEngine);
        AddColumnIdentityDefaultChanges(changes, sourceColumn, targetColumn);
    }

    // islevi: Kolon tip/null/uzunluk/precision/scale degisikliklerini summary listesine ekler.
    private static void AddColumnTypeShapeChanges(
        List<string> changes,
        SchemaColumnModel sourceColumn,
        SchemaColumnModel targetColumn,
        bool isSameEngine)
    {
        AddChange(
            changes,
            ChangeLabels.DataType,
            GetComparableDataType(sourceColumn, isSameEngine),
            GetComparableDataType(targetColumn, isSameEngine));
        AddChange(changes, ChangeLabels.Nullable, sourceColumn.IsNullable, targetColumn.IsNullable);
        AddChange(changes, ChangeLabels.MaxLength, sourceColumn.MaxLength, targetColumn.MaxLength);
        AddChange(
            changes,
            ChangeLabels.NumericPrecision,
            sourceColumn.NumericPrecision,
            targetColumn.NumericPrecision);
        AddChange(
            changes,
            ChangeLabels.NumericScale,
            sourceColumn.NumericScale,
            targetColumn.NumericScale);
    }

    // islevi: Kolon identity bayragi ve normalize default ifade degisikliklerini summary listesine ekler.
    private void AddColumnIdentityDefaultChanges(
        List<string> changes,
        SchemaColumnModel sourceColumn,
        SchemaColumnModel targetColumn)
    {
        AddChange(changes, ChangeLabels.Identity, sourceColumn.IsIdentity, targetColumn.IsIdentity);
        AddChange(
            changes,
            ChangeLabels.Default,
            _normalizer.NormalizeExpression(sourceColumn.DefaultValueSql),
            _normalizer.NormalizeExpression(targetColumn.DefaultValueSql));
    }

    // islevi: Kolonun collation/generated/identity-sequence/comment derinlik degisikliklerini summary listesine ekler.
    private void AddColumnDepthChanges(
        List<string> changes,
        SchemaColumnModel sourceColumn,
        SchemaColumnModel targetColumn,
        bool isSameEngine,
        string? sourceDatabaseCollation,
        string? targetDatabaseCollation)
    {
        AddChange(changes, ChangeLabels.Collation,
            _normalizer.NormalizeColumnCollation(sourceColumn.CollationName, sourceDatabaseCollation),
            _normalizer.NormalizeColumnCollation(targetColumn.CollationName, targetDatabaseCollation));
        AddChange(changes, ChangeLabels.Generated, sourceColumn.IsGenerated, targetColumn.IsGenerated);
        AddChange(changes, ChangeLabels.GenerationExpression,
            _normalizer.NormalizeExpression(sourceColumn.GenerationExpression),
            _normalizer.NormalizeExpression(targetColumn.GenerationExpression));
        AddChange(changes, ChangeLabels.IdentitySeed, sourceColumn.IdentitySeed, targetColumn.IdentitySeed);
        AddChange(changes, ChangeLabels.IdentityIncrement, sourceColumn.IdentityIncrement, targetColumn.IdentityIncrement);
        AddChange(changes, ChangeLabels.Comment, sourceColumn.Comment, targetColumn.Comment);
        if (isSameEngine)
        {
            AddChange(changes, ChangeLabels.Persisted, sourceColumn.IsPersisted, targetColumn.IsPersisted);
        }
    }

    // islevi: Index modified bulgusunda hangi ana alanlarin degistigini kisa ozetler.
    private string? BuildIndexChangeSummary(SchemaIndexModel sourceIndex, SchemaIndexModel targetIndex)
    {
        var changes = new List<string>();
        AddChange(changes, ChangeLabels.Unique, sourceIndex.IsUnique, targetIndex.IsUnique);
        AddChange(changes, ChangeLabels.PrimaryKey, sourceIndex.IsPrimaryKey, targetIndex.IsPrimaryKey);
        AddChange(
            changes,
            ChangeLabels.Columns,
            _normalizer.NormalizeNameList(sourceIndex.Columns, sort: false),
            _normalizer.NormalizeNameList(targetIndex.Columns, sort: false));
        AddChange(
            changes,
            ChangeLabels.IncludedColumns,
            _normalizer.NormalizeNameList(sourceIndex.IncludedColumns, sort: true),
            _normalizer.NormalizeNameList(targetIndex.IncludedColumns, sort: true));
        AddChange(
            changes,
            ChangeLabels.Filter,
            _normalizer.NormalizeExpression(sourceIndex.FilterDefinition),
            _normalizer.NormalizeExpression(targetIndex.FilterDefinition));
        AddChange(
            changes,
            ChangeLabels.Definition,
            BuildIndexProviderDetailDefinition(sourceIndex),
            BuildIndexProviderDetailDefinition(targetIndex));
        return BuildChangeSummary(changes);
    }

    // islevi: Constraint modified bulgusunda hangi ana alanlarin degistigini kisa ozetler.
    private string? BuildConstraintChangeSummary(
        SchemaConstraintModel sourceConstraint,
        SchemaConstraintModel targetConstraint,
        bool isSameEngine)
    {
        var changes = new List<string>();
        AddConstraintRelationshipChanges(changes, sourceConstraint, targetConstraint);
        AddConstraintStateChanges(changes, sourceConstraint, targetConstraint, isSameEngine);
        return BuildChangeSummary(changes);
    }

    // islevi: Constraint tur/kolon/hedef/aksiyon/tanim degisikliklerini summary listesine ekler.
    private void AddConstraintRelationshipChanges(
        List<string> changes,
        SchemaConstraintModel sourceConstraint,
        SchemaConstraintModel targetConstraint)
    {
        AddConstraintIdentityChanges(changes, sourceConstraint, targetConstraint);
        AddConstraintReferenceChanges(changes, sourceConstraint, targetConstraint);
        AddConstraintActionDefinitionChanges(changes, sourceConstraint, targetConstraint);
    }

    // islevi: Constraint tur ve kaynak kolon degisikliklerini summary listesine ekler.
    private void AddConstraintIdentityChanges(
        List<string> changes,
        SchemaConstraintModel sourceConstraint,
        SchemaConstraintModel targetConstraint)
    {
        AddChange(changes, ChangeLabels.Type, sourceConstraint.TypeCode, targetConstraint.TypeCode);
        AddChange(
            changes,
            ChangeLabels.Columns,
            _normalizer.NormalizeNameList(sourceConstraint.Columns, sort: false),
            _normalizer.NormalizeNameList(targetConstraint.Columns, sort: false));
    }

    // islevi: Constraint hedef tablo ve hedef kolon degisikliklerini summary listesine ekler.
    private void AddConstraintReferenceChanges(
        List<string> changes,
        SchemaConstraintModel sourceConstraint,
        SchemaConstraintModel targetConstraint)
    {
        AddChange(
            changes,
            ChangeLabels.ReferencedTable,
            _normalizer.NormalizeIdentifier(sourceConstraint.ReferencedTable),
            _normalizer.NormalizeIdentifier(targetConstraint.ReferencedTable));
        AddChange(
            changes,
            ChangeLabels.ReferencedColumns,
            _normalizer.NormalizeNameList(sourceConstraint.ReferencedColumns, sort: false),
            _normalizer.NormalizeNameList(targetConstraint.ReferencedColumns, sort: false));
    }

    // islevi: Constraint referential-action ve normalize definition degisikliklerini summary listesine ekler.
    private void AddConstraintActionDefinitionChanges(
        List<string> changes,
        SchemaConstraintModel sourceConstraint,
        SchemaConstraintModel targetConstraint)
    {
        AddChange(
            changes,
            ChangeLabels.DeleteAction,
            sourceConstraint.DeleteActionCode,
            targetConstraint.DeleteActionCode);
        AddChange(
            changes,
            ChangeLabels.UpdateAction,
            sourceConstraint.UpdateActionCode,
            targetConstraint.UpdateActionCode);
        AddChange(
            changes,
            ChangeLabels.Definition,
            _normalizer.NormalizeExpression(sourceConstraint.Definition),
            _normalizer.NormalizeExpression(targetConstraint.Definition));
    }

    // islevi: Constraint guven/etkinlik ve motor destekliyorsa erteleme degisikliklerini summary listesine ekler.
    private static void AddConstraintStateChanges(
        List<string> changes,
        SchemaConstraintModel sourceConstraint,
        SchemaConstraintModel targetConstraint,
        bool isSameEngine)
    {
        AddChange(changes, ChangeLabels.Validated, sourceConstraint.IsValidated, targetConstraint.IsValidated);
        AddChange(changes, ChangeLabels.Enabled, sourceConstraint.IsEnabled, targetConstraint.IsEnabled);
        if (isSameEngine)
        {
            AddChange(changes, ChangeLabels.Deferrable, sourceConstraint.IsDeferrable, targetConstraint.IsDeferrable);
            AddChange(changes, ChangeLabels.InitiallyDeferred, sourceConstraint.IsInitiallyDeferred, targetConstraint.IsInitiallyDeferred);
        }
    }

    // islevi: Trigger definition ve etkinlik degisikliklerini summary listesine ekler.
    private string? BuildTriggerChangeSummary(SchemaTriggerModel sourceTrigger, SchemaTriggerModel targetTrigger)
    {
        var changes = new List<string>();
        AddChange(changes, ChangeLabels.Definition,
            _normalizer.NormalizeDefinition(sourceTrigger.Definition),
            _normalizer.NormalizeDefinition(targetTrigger.Definition));
        AddChange(changes, ChangeLabels.Enabled, sourceTrigger.IsEnabled, targetTrigger.IsEnabled);
        return BuildChangeSummary(changes);
    }

    // islevi: Veritabani collation ve desteklenen provider degisikliklerini summary listesine ekler.
    private static string? BuildDatabaseChangeSummary(
        SchemaSnapshotModel sourceSnapshot,
        SchemaSnapshotModel targetSnapshot,
        bool isSameEngine)
    {
        var changes = new List<string>();
        AddChange(changes, ChangeLabels.DatabaseCollation, sourceSnapshot.DatabaseCollationName, targetSnapshot.DatabaseCollationName);
        if (isSameEngine)
        {
            AddChange(changes, ChangeLabels.CollationProvider, sourceSnapshot.CollationProviderCode, targetSnapshot.CollationProviderCode);
        }

        return BuildChangeSummary(changes);
    }

    // islevi: Kolon disi nesnelerde ayni engine icin Exact, capraz engine icin Canonical guven kodunu secer.
    private static string GetNonColumnConfidenceCode(
        SchemaSnapshotModel sourceSnapshot,
        SchemaSnapshotModel targetSnapshot)
    {
        return string.Equals(sourceSnapshot.EngineCode, targetSnapshot.EngineCode, StringComparison.Ordinal)
            ? ComparisonConfidenceCodes.Exact
            : ComparisonConfidenceCodes.Canonical;
    }

    // islevi: Tabloyu schema+name anahtariyla eslestirir.
    private static string BuildTableKey(SchemaTableModel table)
        => BuildObjectKey(table.Schema, table.Name);

    // islevi: Tablo disi nesneyi schema+tur+name anahtariyla eslestirir.
    private static string BuildSchemaObjectKey(SchemaObjectDefinitionModel schemaObject)
        => BuildObjectKey(schemaObject.Schema, schemaObject.ObjectTypeCode, schemaObject.Name);

    // islevi: Constraint'i tur+ad anahtariyla eslestirir.
    private string BuildConstraintKey(SchemaConstraintModel constraint)
        => BuildObjectKey(constraint.TypeCode, _normalizer.NormalizeIdentifier(constraint.Name));

    // islevi: Birden cok kimlik parcasini case-insensitive dictionary icin kararli anahtara cevirir.
    private static string BuildObjectKey(params string?[] parts)
        => string.Join(
            ComparisonCanonicalTextConstants.KeySeparator,
            parts.Select(part => part?.Trim() ?? string.Empty));

    // islevi: Tablo fark adresini olusturur.
    private static SchemaComparisonAddressModel BuildTableAddress(SchemaTableModel table)
        => new()
        {
            SchemaName = table.Schema,
            ObjectName = table.Name,
            ObjectTypeCode = SchemaObjectTypeCodes.Table
        };

    // islevi: Veritabani-seviyesi metadata farki icin finding adresini olusturur.
    private static SchemaComparisonAddressModel BuildDatabaseAddress()
        => new()
        {
            ObjectName = SchemaObjectTypeCodes.Database,
            ObjectTypeCode = SchemaObjectTypeCodes.Database
        };

    // islevi: Tablo disi nesne fark adresini olusturur.
    private static SchemaComparisonAddressModel BuildSchemaObjectAddress(SchemaObjectDefinitionModel schemaObject)
        => new()
        {
            SchemaName = schemaObject.Schema,
            ObjectName = schemaObject.Name,
            ObjectTypeCode = schemaObject.ObjectTypeCode
        };

    // islevi: Tablo alt nesnesi fark adresini olusturur.
    private static SchemaComparisonAddressModel BuildChildAddress(
        SchemaTableModel table,
        string childName,
        string objectTypeCode)
        => new()
        {
            SchemaName = table.Schema,
            ObjectName = table.Name,
            ChildName = childName,
            ObjectTypeCode = objectTypeCode
        };

    // islevi: Tablo var/yok farkinda rapor kaniti olarak okunabilir tablo kimligi uretir.
    private static string BuildTableDefinition(SchemaTableModel table)
        => $"{table.Schema}.{table.Name}";

    // islevi: Ayni motorda ham tipi, capraz motorda yalniz tasinan kanonik aileyi karsilastirma olcutu yapar.
    private static string? GetComparableDataType(SchemaColumnModel column, bool isSameEngine)
        => isSameEngine
            ? column.RawDataType
            : column.CanonicalDataType;

    // islevi: Iki snapshot motor kodunun ayni kesinlik sinirinda olup olmadigini belirler.
    private static bool IsSameEngine(string sourceEngineCode, string targetEngineCode)
        => string.Equals(sourceEngineCode, targetEngineCode, StringComparison.Ordinal);

    // islevi: Constraint type kodunu raporda kullanilan schema object type koduna cevirir.
    private static string MapConstraintObjectType(string constraintTypeCode)
        => constraintTypeCode switch
        {
            SchemaConstraintTypeCodes.PrimaryKey => SchemaObjectTypeCodes.PrimaryKey,
            SchemaConstraintTypeCodes.ForeignKey => SchemaObjectTypeCodes.ForeignKey,
            SchemaConstraintTypeCodes.Unique => SchemaObjectTypeCodes.Unique,
            SchemaConstraintTypeCodes.Check => SchemaObjectTypeCodes.Check,
            _ => SchemaObjectTypeCodes.Check
        };

    // islevi: Degisen alan adini ancak normalized degerler farkliysa summary listesine ekler.
    private static void AddChange<TValue>(List<string> changes, string label, TValue sourceValue, TValue targetValue)
    {
        if (!EqualityComparer<TValue>.Default.Equals(sourceValue, targetValue))
        {
            changes.Add(label);
        }
    }

    // islevi: Degisim alan listesini finding summary string'ine cevirir.
    private static string? BuildChangeSummary(List<string> changes)
        => changes.Count == 0
            ? null
            : string.Join(ComparisonCanonicalTextConstants.ChangeSummarySeparator, changes);

    // islevi: Field/value ciftlerini tek kararli schema-definition string formatina cevirir.
    private static string BuildDefinition(params (string FieldName, string FieldValue)[] fields)
        => string.Join(
            ComparisonCanonicalTextConstants.DefinitionFieldSeparator,
            fields.Select(field =>
                field.FieldName +
                ComparisonCanonicalTextConstants.DefinitionKeyValueSeparator +
                field.FieldValue));

    // islevi: Null degerleri bos string'e indirerek definition formatini provider farklarindan korur.
    private static (string FieldName, string FieldValue) DefinitionField(string fieldName, object? value)
        => (fieldName, value?.ToString() ?? string.Empty);

    // islevi: Bulgularin response ve testlerde kararli sirayla gorunmesini saglar.
    private static List<SchemaDifferenceModel> SortDifferences(List<SchemaDifferenceModel> differences)
        => differences
            .OrderBy(difference => difference.SchemaName)
            .ThenBy(difference => difference.ObjectTypeCode)
            .ThenBy(difference => difference.ObjectName)
            .ThenBy(difference => difference.ChildName)
            .ThenBy(difference => difference.KindCode)
            .ToList();

}

using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Comparison.Scope;
using Shouldly;
using Xunit;
using ChangeLabels = Ptn.DatabaseChecker.Constants.Comparison.SchemaComparisonTextConstants.ChangeLabels;

namespace Ptn.DatabaseChecker.Comparison;

// islevi: SchemaComparisonManager'in normalize + compare + finding uretim davranisini dogrular.
// sistemdeki gorevi: T6 motorunun yon bilgisi, scope uygulamasi ve yalanci whitespace/sira farki elemesini saf domain testleriyle korur.
public class SchemaComparisonManager_Tests
{
    [Fact]
    public void Should_Produce_Directional_Column_And_Table_Findings()
    {
        var manager = CreateManager();
        var source = Snapshot(
            Table(
                "public",
                "customers",
                Column("id", "integer", isNullable: false),
                Column("name", "varchar(50)", isNullable: true)));
        var target = Snapshot(
            Table(
                "public",
                "customers",
                Column("id", "bigint", isNullable: false),
                Column("email", "varchar(100)", isNullable: true)),
            Table("public", "orders", Column("id", "integer", isNullable: false)));

        var findings = manager.Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Table &&
            difference.ObjectName == "orders" &&
            difference.KindCode == DifferenceKindCodes.OnlyInTarget);
        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Column &&
            difference.ChildName == "name" &&
            difference.KindCode == DifferenceKindCodes.OnlyInSource);
        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Column &&
            difference.ChildName == "email" &&
            difference.KindCode == DifferenceKindCodes.OnlyInTarget);
        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Column &&
            difference.ChildName == "id" &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary == "DataType");
    }

    [Fact]
    public void Should_Report_Reordered_Columns_As_Ordinal_Change()
    {
        var manager = CreateManager();
        var source = Snapshot(
            Table(
                "public",
                "customers",
                Column("id", "integer", isNullable: false),
                Column("name", "varchar(50)", isNullable: true),
                Column("email", "varchar(100)", isNullable: true)));
        var target = Snapshot(
            Table(
                "public",
                "customers",
                Column("id", "integer", isNullable: false),
                Column("email", "varchar(100)", isNullable: true),
                Column("name", "varchar(50)", isNullable: true)));

        var findings = manager.Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ChildName == "name" &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary == "Ordinal");
        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ChildName == "email" &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary == "Ordinal");
        findings.SchemaDifferences.ShouldNotContain(difference => difference.ChildName == "id");
    }

    [Fact]
    public void Should_Not_Report_Order_Change_When_Column_Added_In_Middle()
    {
        var manager = CreateManager();
        var source = Snapshot(
            Table(
                "public",
                "customers",
                Column("id", "integer", isNullable: false),
                Column("name", "varchar(50)", isNullable: true)));
        var target = Snapshot(
            Table(
                "public",
                "customers",
                Column("id", "integer", isNullable: false),
                Column("created_at", "timestamp", isNullable: true),
                Column("name", "varchar(50)", isNullable: true)));

        var findings = manager.Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ChildName == "created_at" &&
            difference.KindCode == DifferenceKindCodes.OnlyInTarget);
        findings.SchemaDifferences.ShouldNotContain(difference => difference.ChangeSummary == "Ordinal");
    }

    [Fact]
    public void Should_Ignore_View_Whitespace_Noise()
    {
        var manager = CreateManager();
        var source = Snapshot(
            objects: ObjectDefinition(
                "public",
                "v_customers",
                SchemaObjectTypeCodes.View,
                "CREATE VIEW public.v_customers AS SELECT id, name FROM public.customers"));
        var target = Snapshot(
            objects: ObjectDefinition(
                "public",
                "v_customers",
                SchemaObjectTypeCodes.View,
                "CREATE   VIEW public.v_customers AS\r\nSELECT id ,  name FROM public.customers;"));

        var findings = manager.Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Ignore_Included_Index_Column_Order()
    {
        var manager = CreateManager();
        var source = Snapshot(Table(
            "public",
            "customers",
            indexes: new[] { Index("IX_Customers_Email", columns: ["email"], includedColumns: ["name", "phone"]) }));
        var target = Snapshot(Table(
            "public",
            "customers",
            indexes: new[] { Index("IX_Customers_Email", columns: ["email"], includedColumns: ["phone", "name"]) }));

        var findings = manager.Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Ignore_Index_Name_Only_Rename()
    {
        var source = Snapshot(Table(
            "public",
            "routes",
            indexes:
            [
                Index(
                    "ix_routes_route_creation_time",
                    columns: ["creation_time"],
                    includedColumns: [],
                    definition: "CREATE INDEX ix_routes_route_creation_time ON gtfs_management.routes USING btree (creation_time)")
            ]));
        var target = Snapshot(Table(
            "public",
            "routes",
            indexes:
            [
                Index(
                    "IX_routes_creation_time",
                    columns: ["creation_time"],
                    includedColumns: [],
                    definition: "CREATE INDEX \"IX_routes_creation_time\" ON gtfs_management.routes USING btree (creation_time)")
            ]));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Ignore_Index_Name_Casing_And_Raw_Definition_Casing()
    {
        var source = Snapshot(Table(
            "public",
            "routes",
            indexes:
            [
                Index(
                    "ix_routes_creation_time",
                    columns: ["creation_time"],
                    includedColumns: [],
                    definition: "CREATE INDEX ix_routes_creation_time ON public.routes USING btree (creation_time)")
            ]));
        var target = Snapshot(Table(
            "public",
            "routes",
            indexes:
            [
                Index(
                    "IX_ROUTES_CREATION_TIME",
                    columns: ["creation_time"],
                    includedColumns: [],
                    definition: "CREATE INDEX \"IX_ROUTES_CREATION_TIME\" ON public.routes USING btree (creation_time)")
            ]));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Not_Structurally_Match_Ambiguous_Duplicate_Indexes()
    {
        var source = Snapshot(Table(
            "public",
            "routes",
            indexes:
            [
                Index("ix_routes_creation_time_a", columns: ["creation_time"], includedColumns: []),
                Index("ix_routes_creation_time_b", columns: ["creation_time"], includedColumns: [])
            ]));
        var target = Snapshot(Table(
            "public",
            "routes",
            indexes:
            [
                Index("ix_routes_creation_time_target", columns: ["creation_time"], includedColumns: [])
            ]));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.Count(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Index &&
            difference.KindCode == DifferenceKindCodes.OnlyInSource).ShouldBe(2);
        findings.SchemaDifferences.Count(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Index &&
            difference.KindCode == DifferenceKindCodes.OnlyInTarget).ShouldBe(1);
    }

    [Fact]
    public void Expression_Index_Rename_Is_Ignored_But_Expression_Change_Is_Detected()
    {
        var source = Snapshot(Table(
            "public",
            "routes",
            indexes:
            [
                Index(
                    "ix_routes_lower_name",
                    columns: [],
                    includedColumns: [],
                    definition: "CREATE INDEX ix_routes_lower_name ON public.routes USING btree (lower(name))")
            ]));
        var renamedTarget = Snapshot(Table(
            "public",
            "routes",
            indexes:
            [
                Index(
                    "IX_ROUTES_NAME",
                    columns: [],
                    includedColumns: [],
                    definition: "CREATE INDEX \"IX_ROUTES_NAME\" ON public.routes USING btree (lower(name))")
            ]));
        var changedTarget = Snapshot(Table(
            "public",
            "routes",
            indexes:
            [
                Index(
                    "ix_routes_lower_name",
                    columns: [],
                    includedColumns: [],
                    definition: "CREATE INDEX ix_routes_lower_name ON public.routes USING btree (upper(name))")
            ]));

        CreateManager()
            .Compare(source, renamedTarget, new List<ComparisonScopeRule>())
            .SchemaDifferences
            .ShouldBeEmpty();
        CreateManager()
            .Compare(source, changedTarget, new List<ComparisonScopeRule>())
            .SchemaDifferences
            .ShouldContain(difference =>
                difference.ObjectTypeCode == SchemaObjectTypeCodes.Index &&
                difference.KindCode == DifferenceKindCodes.Modified &&
                difference.ChangeSummary!.Contains(ChangeLabels.Definition));
    }

    [Fact]
    public void Should_Apply_Exclude_Scope_To_Tables_And_Objects()
    {
        var manager = CreateManager();
        var source = Snapshot(
            Table("public", "audit_log", Column("id", "integer", isNullable: false)),
            objects: ObjectDefinition("public", "audit_view", SchemaObjectTypeCodes.View, "select 1"));
        var target = Snapshot();
        var rules = new List<ComparisonScopeRule>
        {
            Rule(ScopeKindCodes.Exclude, "public")
        };

        var findings = manager.Compare(source, target, rules);

        findings.SchemaDifferences.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Report_Constraint_And_Trigger_Definition_Changes()
    {
        var manager = CreateManager();
        var source = Snapshot(Table(
            "public",
            "orders",
            constraints: new[] { Constraint("FK_orders_customer", SchemaConstraintTypeCodes.ForeignKey, ["customer_id"], "public.customers", ["id"], deleteAction: SchemaReferentialActionCodes.NoAction) },
            triggers: new[] { Trigger("trg_orders_audit", "CREATE TRIGGER trg_orders_audit BEFORE INSERT ON orders EXECUTE FUNCTION audit_old()") }));
        var target = Snapshot(Table(
            "public",
            "orders",
            constraints: new[] { Constraint("FK_orders_customer", SchemaConstraintTypeCodes.ForeignKey, ["customer_id"], "public.customers", ["id"], deleteAction: SchemaReferentialActionCodes.Cascade) },
            triggers: new[] { Trigger("trg_orders_audit", "CREATE TRIGGER trg_orders_audit BEFORE INSERT ON orders EXECUTE FUNCTION audit_new()") }));

        var findings = manager.Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.ForeignKey &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary == "DeleteAction");
        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Trigger &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary == "Definition");
    }

    [Fact]
    public void Should_Exclude_Named_Column_From_Table_Findings()
    {
        var manager = CreateManager();
        var source = Snapshot(Table(
            "public",
            "customers",
            Column("id", "integer", isNullable: false),
            Column("email", "varchar(50)", isNullable: true),
            Column("audit_note", "varchar(50)", isNullable: true)));
        var target = Snapshot(Table(
            "public",
            "customers",
            Column("id", "bigint", isNullable: false),
            Column("email", "varchar(100)", isNullable: true),
            Column("audit_note", "varchar(100)", isNullable: true)));
        var rules = new List<ComparisonScopeRule>
        {
            Rule(ScopeKindCodes.Exclude, "public", "customers", "audit_note")
        };

        var findings = manager.Compare(source, target, rules);

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ChildName == "id" && difference.KindCode == DifferenceKindCodes.Modified);
        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ChildName == "email" && difference.KindCode == DifferenceKindCodes.Modified);
        findings.SchemaDifferences.ShouldNotContain(difference => difference.ChildName == "audit_note");
    }

    [Fact]
    public void Should_Whitelist_Only_Included_Column_Within_Table()
    {
        var manager = CreateManager();
        var source = Snapshot(Table(
            "public",
            "customers",
            Column("id", "integer", isNullable: false),
            Column("email", "varchar(50)", isNullable: true),
            Column("phone", "varchar(50)", isNullable: true)));
        var target = Snapshot(Table(
            "public",
            "customers",
            Column("id", "bigint", isNullable: false),
            Column("email", "varchar(100)", isNullable: true),
            Column("phone", "varchar(100)", isNullable: true)));
        var rules = new List<ComparisonScopeRule>
        {
            Rule(ScopeKindCodes.Include, "public", "customers", "email")
        };

        var findings = manager.Compare(source, target, rules);

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ChildName == "email" && difference.KindCode == DifferenceKindCodes.Modified);
        findings.SchemaDifferences.ShouldNotContain(difference => difference.ChildName == "id");
        findings.SchemaDifferences.ShouldNotContain(difference => difference.ChildName == "phone");
    }

    // islevi: Ayni motor kolon tip farkinin ham tip uzerinden Exact guvenle raporlandigini dogrular.
    [Fact]
    public void Same_Engine_Type_Difference_Should_Remain_Exact()
    {
        var findings = CompareOneColumn(
            Column("id", "integer", isNullable: false),
            Column("id", "bigint", isNullable: false));

        findings.SchemaDifferences.ShouldHaveSingleItem().ConfidenceCode
            .ShouldBe(ComparisonConfidenceCodes.Exact);
    }

    // islevi: PostgreSQL varchar ile SQL Server nvarchar'in ayni String ailesinde yalanci fark uretmedigini dogrular.
    [Fact]
    public void Cross_Engine_Varchar_And_NVarChar_Should_Not_Differ()
    {
        var source = SnapshotForEngine(
            DatabaseEngineCodes.PostgreSql,
            Table(
                "public",
                "customers",
                Column(
                    "name",
                    "varchar(100)",
                    isNullable: false,
                    maxLength: 100,
                    canonicalDataType: CanonicalDataTypeCodes.String,
                    fidelityCode: TypeMappingFidelityCodes.Exact)));
        var target = SnapshotForEngine(
            DatabaseEngineCodes.SqlServer,
            Table(
                "public",
                "customers",
                Column(
                    "name",
                    "nvarchar(100)",
                    isNullable: false,
                    maxLength: 100,
                    canonicalDataType: CanonicalDataTypeCodes.String,
                    fidelityCode: TypeMappingFidelityCodes.Exact)));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldBeEmpty();
    }

    // islevi: Ayni adli FK trust farkinin Modified ve Validated ozeti urettigini dogrular.
    [Fact]
    public void Should_Report_Unvalidated_Constraint_As_Modified()
    {
        var sourceConstraint = Constraint("FK_orders_customer", SchemaConstraintTypeCodes.ForeignKey, ["customer_id"]);
        sourceConstraint.IsValidated = false;
        var targetConstraint = Constraint("FK_orders_customer", SchemaConstraintTypeCodes.ForeignKey, ["customer_id"]);
        var source = Snapshot(Table("public", "orders", constraints: [sourceConstraint]));
        var target = Snapshot(Table("public", "orders", constraints: [targetConstraint]));

        var difference = CreateManager().Compare(source, target, []).SchemaDifferences.ShouldHaveSingleItem();

        difference.KindCode.ShouldBe(DifferenceKindCodes.Modified);
        difference.ChangeSummary.ShouldBe(ChangeLabels.Validated);
        difference.ConfidenceCode.ShouldBe(ComparisonConfidenceCodes.Exact);
    }

    // islevi: Devre disi constraint ve trigger'in Enabled degisikligi olarak raporlandigini dogrular.
    [Fact]
    public void Should_Report_Disabled_Constraint_And_Trigger_As_Modified()
    {
        var sourceConstraint = Constraint("CK_orders_total", SchemaConstraintTypeCodes.Check, [], "total >= 0");
        sourceConstraint.IsEnabled = false;
        var targetConstraint = Constraint("CK_orders_total", SchemaConstraintTypeCodes.Check, [], "total >= 0");
        var source = Snapshot(Table("public", "orders", constraints: [sourceConstraint], triggers: [Trigger("audit", "body", false)]));
        var target = Snapshot(Table("public", "orders", constraints: [targetConstraint], triggers: [Trigger("audit", "body")]));

        var differences = CreateManager().Compare(source, target, []).SchemaDifferences;

        differences.Count.ShouldBe(2);
        differences.ShouldAllBe(difference => difference.KindCode == DifferenceKindCodes.Modified);
        differences.ShouldAllBe(difference => difference.ChangeSummary == ChangeLabels.Enabled);
    }

    // islevi: Varsayilandan farkli kolon collation degisikliginin Modified bulgusu urettigini dogrular.
    [Fact]
    public void Should_Report_Column_Collation_Difference_As_Modified()
    {
        var source = SnapshotWithCollation("en_US", Table("public", "customers", ColumnWithDepth("name", collationName: "tr_TR")));
        var target = SnapshotWithCollation("en_US", Table("public", "customers", ColumnWithDepth("name", collationName: "en_US")));

        var difference = CreateManager().Compare(source, target, []).SchemaDifferences.ShouldHaveSingleItem();

        difference.ObjectTypeCode.ShouldBe(SchemaObjectTypeCodes.Column);
        difference.ChangeSummary.ShouldBe(ChangeLabels.Collation);
    }

    // islevi: Her kolon kendi veritabani varsayilanini kullandiginda kolon seviyesinde gurultu uretilmedigini dogrular.
    [Fact]
    public void Should_Not_Report_Column_Collation_When_Each_Uses_Its_Database_Default()
    {
        var source = SnapshotWithCollation("en_US", Table("public", "customers", ColumnWithDepth("name", collationName: "en_US")));
        var target = SnapshotWithCollation("tr_TR", Table("public", "customers", ColumnWithDepth("name", collationName: "tr_TR")));

        var differences = CreateManager().Compare(source, target, []).SchemaDifferences;

        differences.ShouldNotContain(difference => difference.ObjectTypeCode == SchemaObjectTypeCodes.Column);
        differences.ShouldContain(difference => difference.ObjectTypeCode == SchemaObjectTypeCodes.Database);
    }

    // islevi: Generated ifade farkinin raporlandigini, normalize-esit ifadenin susturuldugunu dogrular.
    [Fact]
    public void Should_Report_Generation_Expression_Difference_But_Ignore_Equal_Expression()
    {
        var source = Snapshot(Table("public", "orders", ColumnWithDepth("total", isGenerated: true, generationExpression: "price * quantity", isPersisted: true)));
        var changedTarget = Snapshot(Table("public", "orders", ColumnWithDepth("total", isGenerated: true, generationExpression: "price * (quantity + 1)", isPersisted: true)));
        var equalTarget = Snapshot(Table("public", "orders", ColumnWithDepth("total", isGenerated: true, generationExpression: "price*quantity", isPersisted: true)));

        var changed = CreateManager().Compare(source, changedTarget, []).SchemaDifferences.ShouldHaveSingleItem();
        var equal = CreateManager().Compare(source, equalTarget, []).SchemaDifferences;

        changed.ChangeSummary.ShouldBe(ChangeLabels.GenerationExpression);
        equal.ShouldBeEmpty();
    }

    // islevi: Identity seed ve increment farklarinin birlikte change summary'ye girdigini dogrular.
    [Fact]
    public void Should_Report_Identity_Seed_And_Increment_Differences()
    {
        var source = Snapshot(Table("public", "orders", ColumnWithDepth("id", isIdentity: true, identitySeed: "1", identityIncrement: "1")));
        var target = Snapshot(Table("public", "orders", ColumnWithDepth("id", isIdentity: true, identitySeed: "100", identityIncrement: "10")));

        var difference = CreateManager().Compare(source, target, []).SchemaDifferences.ShouldHaveSingleItem();

        difference.ChangeSummary.ShouldBe(string.Join(
            ComparisonCanonicalTextConstants.ChangeSummarySeparator,
            ChangeLabels.IdentitySeed,
            ChangeLabels.IdentityIncrement));
    }

    // islevi: Kolon comment farkinin normal Modified bulgusu olarak raporlandigini dogrular.
    [Fact]
    public void Should_Report_Column_Comment_Difference()
    {
        var source = Snapshot(Table("public", "orders", ColumnWithDepth("id", comment: "Internal identifier")));
        var target = Snapshot(Table("public", "orders", ColumnWithDepth("id", comment: "Public identifier")));

        var difference = CreateManager().Compare(source, target, []).SchemaDifferences.ShouldHaveSingleItem();

        difference.ChangeSummary.ShouldBe(ChangeLabels.Comment);
    }

    // islevi: Cross-engine kiyasta deferrable ve persisted destek farklarinin sahte Modified uretmedigini dogrular.
    [Fact]
    public void Cross_Engine_Should_Ignore_Deferrable_And_Persisted_Only_Differences()
    {
        var sourceConstraint = Constraint("FK_orders_customer", SchemaConstraintTypeCodes.ForeignKey, ["customer_id"]);
        sourceConstraint.IsDeferrable = true;
        sourceConstraint.IsInitiallyDeferred = true;
        var targetConstraint = Constraint("FK_orders_customer", SchemaConstraintTypeCodes.ForeignKey, ["customer_id"]);
        var sourceColumn = ColumnWithDepth("computed_total", isGenerated: true, generationExpression: "price * quantity", isPersisted: true);
        var targetColumn = ColumnWithDepth("computed_total", isGenerated: true, generationExpression: "price * quantity", isPersisted: false);
        SetCanonicalType(sourceColumn);
        SetCanonicalType(targetColumn);

        var source = SnapshotForEngine(DatabaseEngineCodes.PostgreSql, Table("public", "orders", [sourceColumn], constraints: [sourceConstraint]));
        var target = SnapshotForEngine(DatabaseEngineCodes.SqlServer, Table("public", "orders", [targetColumn], constraints: [targetConstraint]));

        CreateManager().Compare(source, target, []).SchemaDifferences.ShouldBeEmpty();
    }

    // islevi: Ayni motor kiyasında deferrable ve persisted farklarinin Exact Modified olarak korundugunu dogrular.
    [Fact]
    public void Same_Engine_Should_Compare_Deferrable_And_Persisted_Differences()
    {
        var sourceConstraint = Constraint("FK_orders_customer", SchemaConstraintTypeCodes.ForeignKey, ["customer_id"]);
        sourceConstraint.IsDeferrable = true;
        var targetConstraint = Constraint("FK_orders_customer", SchemaConstraintTypeCodes.ForeignKey, ["customer_id"]);
        var sourceColumn = ColumnWithDepth("computed_total", isGenerated: true, generationExpression: "price * quantity", isPersisted: true);
        var targetColumn = ColumnWithDepth("computed_total", isGenerated: true, generationExpression: "price * quantity", isPersisted: false);
        var pgSource = SnapshotForEngine(DatabaseEngineCodes.PostgreSql, Table("public", "orders", constraints: [sourceConstraint]));
        var pgTarget = SnapshotForEngine(DatabaseEngineCodes.PostgreSql, Table("public", "orders", constraints: [targetConstraint]));
        var sqlSource = SnapshotForEngine(DatabaseEngineCodes.SqlServer, Table("dbo", "orders", [sourceColumn]));
        var sqlTarget = SnapshotForEngine(DatabaseEngineCodes.SqlServer, Table("dbo", "orders", [targetColumn]));

        var deferrableDifference = CreateManager().Compare(pgSource, pgTarget, []).SchemaDifferences.ShouldHaveSingleItem();
        var persistedDifference = CreateManager().Compare(sqlSource, sqlTarget, []).SchemaDifferences.ShouldHaveSingleItem();

        deferrableDifference.ConfidenceCode.ShouldBe(ComparisonConfidenceCodes.Exact);
        deferrableDifference.ChangeSummary.ShouldBe(ChangeLabels.Deferrable);
        persistedDifference.ConfidenceCode.ShouldBe(ComparisonConfidenceCodes.Exact);
        persistedDifference.ChangeSummary.ShouldBe(ChangeLabels.Persisted);
    }

    // islevi: Veritabani varsayilan collation farkinin Database seviyesinde raporlandigini dogrular.
    [Fact]
    public void Should_Report_Database_Collation_Difference()
    {
        var source = SnapshotWithCollation("en_US");
        var target = SnapshotWithCollation("tr_TR");

        var difference = CreateManager().Compare(source, target, []).SchemaDifferences.ShouldHaveSingleItem();

        difference.ObjectTypeCode.ShouldBe(SchemaObjectTypeCodes.Database);
        difference.ChangeSummary.ShouldBe(ChangeLabels.DatabaseCollation);
    }

    // islevi: PostgreSQL numeric ile SQL Server decimal'in ayni Decimal ailesi ve sekliyle yalanci fark uretmedigini dogrular.
    [Fact]
    public void Cross_Engine_Numeric_And_Decimal_Should_Not_Differ()
    {
        var source = SnapshotForEngine(
            DatabaseEngineCodes.PostgreSql,
            Table(
                "public",
                "invoices",
                Column(
                    "total",
                    "numeric(10,2)",
                    isNullable: false,
                    numericPrecision: 10,
                    numericScale: 2,
                    canonicalDataType: CanonicalDataTypeCodes.Decimal,
                    fidelityCode: TypeMappingFidelityCodes.Exact)));
        var target = SnapshotForEngine(
            DatabaseEngineCodes.SqlServer,
            Table(
                "public",
                "invoices",
                Column(
                    "total",
                    "decimal(10,2)",
                    isNullable: false,
                    numericPrecision: 10,
                    numericScale: 2,
                    canonicalDataType: CanonicalDataTypeCodes.Decimal,
                    fidelityCode: TypeMappingFidelityCodes.Exact)));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldBeEmpty();
    }

    // islevi: Farkli kanonik ailelerin gercek tip farki olarak Canonical guvenle raporlandigini dogrular.
    [Fact]
    public void Cross_Engine_Integer_And_BigInteger_Should_Differ_With_Canonical_Confidence()
    {
        var source = SnapshotForEngine(
            DatabaseEngineCodes.PostgreSql,
            Table(
                "public",
                "customers",
                Column(
                    "id",
                    "integer",
                    isNullable: false,
                    canonicalDataType: CanonicalDataTypeCodes.Integer,
                    fidelityCode: TypeMappingFidelityCodes.Exact)));
        var target = SnapshotForEngine(
            DatabaseEngineCodes.SqlServer,
            Table(
                "public",
                "customers",
                Column(
                    "id",
                    "bigint",
                    isNullable: false,
                    canonicalDataType: CanonicalDataTypeCodes.BigInteger,
                    fidelityCode: TypeMappingFidelityCodes.Exact)));

        var difference = CreateManager()
            .Compare(source, target, new List<ComparisonScopeRule>())
            .SchemaDifferences
            .ShouldHaveSingleItem();

        difference.KindCode.ShouldBe(DifferenceKindCodes.Modified);
        difference.ChangeSummary.ShouldBe(ChangeLabels.DataType);
        difference.ConfidenceCode.ShouldBe(ComparisonConfidenceCodes.Canonical);
    }

    // islevi: Ayni Money ailesinin fark uretmedigini, kayipli SQL Server eslemesinin ise Approximate guven cozdurdugunu dogrular.
    [Fact]
    public void Cross_Engine_Money_Should_Not_Differ_But_Should_Resolve_Approximate_Confidence()
    {
        var sourceColumn = Column(
            "amount",
            "money",
            isNullable: false,
            canonicalDataType: CanonicalDataTypeCodes.Money,
            fidelityCode: TypeMappingFidelityCodes.Exact);
        var targetColumn = Column(
            "amount",
            "money",
            isNullable: false,
            canonicalDataType: CanonicalDataTypeCodes.Money,
            fidelityCode: TypeMappingFidelityCodes.Approximate);
        var source = SnapshotForEngine(DatabaseEngineCodes.PostgreSql, Table("public", "payments", sourceColumn));
        var target = SnapshotForEngine(DatabaseEngineCodes.SqlServer, Table("public", "payments", targetColumn));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());
        var confidenceCode = new ColumnTypeConfidenceResolver().Resolve(
            source.EngineCode,
            target.EngineCode,
            sourceColumn,
            targetColumn);

        findings.SchemaDifferences.ShouldBeEmpty();
        confidenceCode.ShouldBe(ComparisonConfidenceCodes.Approximate);
    }

    // islevi: Iki farkli eslenemeyen provider tipinin yutulmadan Incomparable bulgu urettigini dogrular.
    [Fact]
    public void Cross_Engine_Unmapped_Types_Should_Differ_With_Incomparable_Confidence()
    {
        var source = SnapshotForEngine(
            DatabaseEngineCodes.PostgreSql,
            Table(
                "public",
                "search_documents",
                Column("payload", EngineDataTypeNameCodes.PostgreSql.TsVector, isNullable: false)));
        var target = SnapshotForEngine(
            DatabaseEngineCodes.SqlServer,
            Table(
                "public",
                "search_documents",
                Column("payload", EngineDataTypeNameCodes.SqlServer.SqlVariant, isNullable: false)));

        var difference = CreateManager()
            .Compare(source, target, new List<ComparisonScopeRule>())
            .SchemaDifferences
            .ShouldHaveSingleItem();

        difference.KindCode.ShouldBe(DifferenceKindCodes.Modified);
        difference.ConfidenceCode.ShouldBe(ComparisonConfidenceCodes.Incomparable);
    }

    // islevi: Capraz motorda ayni ham metnin kanonik esleme yokken raw fallback ile esit sayilmadigini dogrular.
    [Fact]
    public void Cross_Engine_Unmapped_Type_Should_Not_Fall_Back_To_Raw_Text()
    {
        var source = SnapshotForEngine(
            DatabaseEngineCodes.PostgreSql,
            Table(
                "public",
                "search_documents",
                Column("payload", EngineDataTypeNameCodes.PostgreSql.TsVector, isNullable: false)));
        var target = SnapshotForEngine(
            DatabaseEngineCodes.SqlServer,
            Table(
                "public",
                "search_documents",
                Column("payload", EngineDataTypeNameCodes.PostgreSql.TsVector, isNullable: false)));

        var difference = CreateManager()
            .Compare(source, target, new List<ComparisonScopeRule>())
            .SchemaDifferences
            .ShouldHaveSingleItem();

        difference.ConfidenceCode.ShouldBe(ComparisonConfidenceCodes.Incomparable);
    }

    // islevi: Ayni capraz-motor girdisinin tekrarinda bulgu icerigi ve siralamasinin degismedigini dogrular.
    [Fact]
    public void Cross_Engine_Comparison_Should_Be_Deterministic()
    {
        var source = SnapshotForEngine(
            DatabaseEngineCodes.PostgreSql,
            Table(
                "public",
                "customers",
                Column(
                    "id",
                    "integer",
                    isNullable: false,
                    canonicalDataType: CanonicalDataTypeCodes.Integer,
                    fidelityCode: TypeMappingFidelityCodes.Exact)));
        var target = SnapshotForEngine(
            DatabaseEngineCodes.SqlServer,
            Table(
                "public",
                "customers",
                Column(
                    "id",
                    "bigint",
                    isNullable: false,
                    canonicalDataType: CanonicalDataTypeCodes.BigInteger,
                    fidelityCode: TypeMappingFidelityCodes.Exact)));
        var manager = CreateManager();

        var first = manager.Compare(source, target, new List<ComparisonScopeRule>());
        var second = manager.Compare(source, target, new List<ComparisonScopeRule>());

        BuildFindingSignatures(first).ShouldBe(BuildFindingSignatures(second));
    }

    // islevi: Kolon disi capraz-motor farklarinin mevcut Canonical guven davranisini korudugunu dogrular.
    [Fact]
    public void Cross_Engine_Non_Column_Difference_Should_Remain_Canonical()
    {
        var source = SnapshotForEngine(DatabaseEngineCodes.PostgreSql, Table("public", "legacy"));
        var target = SnapshotForEngine(DatabaseEngineCodes.SqlServer);

        var difference = CreateManager()
            .Compare(source, target, new List<ComparisonScopeRule>())
            .SchemaDifferences
            .ShouldHaveSingleItem();

        difference.ObjectTypeCode.ShouldBe(SchemaObjectTypeCodes.Table);
        difference.ConfidenceCode.ShouldBe(ComparisonConfidenceCodes.Canonical);
    }

    // ==========================================================================================
    // T12 (KBP-52) — HER atomik fark kategorisinin yakalandiginin kalici kaniti.
    // "Hicbir zerre fark gozden kacmaz" garantisinin regresyon muhafizi. Her test tek bir alani
    // degistirir ve tam olarak o alanin Modified/OnlyIn bulgusu + dogru ChangeSummary uretmesini bekler.
    // ==========================================================================================

    [Fact]
    public void Column_Nullable_Change_Is_Detected()
        => AssertColumnModified(
            CompareOneColumn(
                Column("email", "varchar", isNullable: false, maxLength: 100),
                Column("email", "varchar", isNullable: true, maxLength: 100)),
            "email",
            ChangeLabels.Nullable);

    [Fact]
    public void Column_MaxLength_Change_Is_Detected() // ornek: varchar(8) -> varchar(20)
        => AssertColumnModified(
            CompareOneColumn(
                Column("code", "character varying", isNullable: false, maxLength: 8),
                Column("code", "character varying", isNullable: false, maxLength: 20)),
            "code",
            ChangeLabels.MaxLength);

    [Fact]
    public void Column_NumericPrecision_Change_Is_Detected() // ornek: numeric(10,2) -> numeric(18,2)
        => AssertColumnModified(
            CompareOneColumn(
                Column("total", "numeric", isNullable: false, numericPrecision: 10, numericScale: 2),
                Column("total", "numeric", isNullable: false, numericPrecision: 18, numericScale: 2)),
            "total",
            ChangeLabels.NumericPrecision);

    [Fact]
    public void Column_NumericScale_Change_Is_Detected() // ornek: numeric(10,2) -> numeric(10,4)
        => AssertColumnModified(
            CompareOneColumn(
                Column("total", "numeric", isNullable: false, numericPrecision: 10, numericScale: 2),
                Column("total", "numeric", isNullable: false, numericPrecision: 10, numericScale: 4)),
            "total",
            ChangeLabels.NumericScale);

    [Fact]
    public void Column_Identity_Change_Is_Detected()
        => AssertColumnModified(
            CompareOneColumn(
                Column("id", "integer", isNullable: false, isIdentity: false),
                Column("id", "integer", isNullable: false, isIdentity: true)),
            "id",
            ChangeLabels.Identity);

    [Fact]
    public void Column_Default_Change_Is_Detected()
        => AssertColumnModified(
            CompareOneColumn(
                Column("status", "integer", isNullable: false, defaultValueSql: null),
                Column("status", "integer", isNullable: false, defaultValueSql: "0")),
            "status",
            ChangeLabels.Default);

    [Fact]
    public void Column_DataType_Change_Is_Detected()
        => AssertColumnModified(
            CompareOneColumn(
                Column("id", "integer", isNullable: false),
                Column("id", "bigint", isNullable: false)),
            "id",
            ChangeLabels.DataType);

    [Fact]
    public void Column_Multiple_Attribute_Changes_All_Appear_In_Summary()
    {
        var findings = CompareOneColumn(
            Column("code", "character varying", isNullable: false, maxLength: 8),
            Column("code", "text", isNullable: true, maxLength: 20));

        var difference = findings.SchemaDifferences.ShouldHaveSingleItem();
        difference.KindCode.ShouldBe(DifferenceKindCodes.Modified);
        var summary = difference.ChangeSummary.ShouldNotBeNull();
        summary.ShouldContain(ChangeLabels.DataType);
        summary.ShouldContain(ChangeLabels.Nullable);
        summary.ShouldContain(ChangeLabels.MaxLength);
    }

    [Fact]
    public void PrimaryKey_Removal_Is_Detected()
    {
        var source = Snapshot(Table("public", "orders",
            constraints: new[] { Constraint("PK_orders", SchemaConstraintTypeCodes.PrimaryKey, ["id"]) }));
        var target = Snapshot(Table("public", "orders"));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.PrimaryKey &&
            difference.KindCode == DifferenceKindCodes.OnlyInSource);
    }

    [Fact]
    public void Unique_Constraint_Column_Change_Is_Detected()
    {
        var source = Snapshot(Table("public", "users",
            constraints: new[] { Constraint("UQ_users_email", SchemaConstraintTypeCodes.Unique, ["email"]) }));
        var target = Snapshot(Table("public", "users",
            constraints: new[] { Constraint("UQ_users_email", SchemaConstraintTypeCodes.Unique, ["email", "tenant_id"]) }));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Unique &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary!.Contains(ChangeLabels.Columns));
    }

    [Fact]
    public void Check_Constraint_Definition_Change_Is_Detected()
    {
        var source = Snapshot(Table("public", "people",
            constraints: new[] { Constraint("CK_people_age", SchemaConstraintTypeCodes.Check, [], "age > 0") }));
        var target = Snapshot(Table("public", "people",
            constraints: new[] { Constraint("CK_people_age", SchemaConstraintTypeCodes.Check, [], "age >= 18") }));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Check &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary!.Contains(ChangeLabels.Definition));
    }

    [Fact]
    public void Index_Uniqueness_Change_Is_Detected()
    {
        var source = Snapshot(Table("public", "customers",
            indexes: new[] { Index("IX_customers_email", columns: ["email"], includedColumns: [], isUnique: false) }));
        var target = Snapshot(Table("public", "customers",
            indexes: new[] { Index("IX_customers_email", columns: ["email"], includedColumns: [], isUnique: true) }));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Index &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary!.Contains(ChangeLabels.Unique));
    }

    [Fact]
    public void Index_Key_Column_Change_Is_Detected()
    {
        var source = Snapshot(Table("public", "customers",
            indexes: new[] { Index("IX_customers_name", columns: ["last_name"], includedColumns: []) }));
        var target = Snapshot(Table("public", "customers",
            indexes: new[] { Index("IX_customers_name", columns: ["last_name", "first_name"], includedColumns: []) }));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Index &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary!.Contains(ChangeLabels.Columns));
    }

    [Fact]
    public void Index_Filter_Change_Is_Detected() // partial / filtered index
    {
        var source = Snapshot(Table("public", "customers",
            indexes: new[] { Index("IX_customers_active", columns: ["email"], includedColumns: [], filterDefinition: null) }));
        var target = Snapshot(Table("public", "customers",
            indexes: new[] { Index("IX_customers_active", columns: ["email"], includedColumns: [], filterDefinition: "is_active = true") }));

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.Index &&
            difference.KindCode == DifferenceKindCodes.Modified &&
            difference.ChangeSummary!.Contains(ChangeLabels.Filter));
    }

    [Fact]
    public void Dropped_Object_Is_Detected_As_OnlyInSource()
    {
        var source = Snapshot(objects: ObjectDefinition("public", "v_report", SchemaObjectTypeCodes.View,
            "CREATE VIEW public.v_report AS SELECT 1"));
        var target = Snapshot();

        var findings = CreateManager().Compare(source, target, new List<ComparisonScopeRule>());

        findings.SchemaDifferences.ShouldContain(difference =>
            difference.ObjectTypeCode == SchemaObjectTypeCodes.View &&
            difference.ObjectName == "v_report" &&
            difference.KindCode == DifferenceKindCodes.OnlyInSource);
    }

    [Fact]
    public void Identical_Rich_Snapshot_Produces_Zero_Differences() // false-positive muhafizi
    {
        var findings = CreateManager().Compare(RichSnapshot(), RichSnapshot(), new List<ComparisonScopeRule>());
        findings.SchemaDifferences.ShouldBeEmpty();
    }

    [Fact]
    public void Comparison_Is_Directionally_Symmetric() // A->B OnlyInSource == B->A OnlyInTarget
    {
        var a = Snapshot(
            Table("public", "only_in_a", Column("id", "integer", isNullable: false)),
            Table("public", "shared", Column("id", "integer", isNullable: false)));
        var b = Snapshot(
            Table("public", "only_in_b", Column("id", "integer", isNullable: false)),
            Table("public", "shared", Column("id", "bigint", isNullable: false)));

        var forward = CreateManager().Compare(a, b, new List<ComparisonScopeRule>());
        var backward = CreateManager().Compare(b, a, new List<ComparisonScopeRule>());

        forward.SchemaDifferences.Count(d => d.KindCode == DifferenceKindCodes.OnlyInSource)
            .ShouldBe(backward.SchemaDifferences.Count(d => d.KindCode == DifferenceKindCodes.OnlyInTarget));
        forward.SchemaDifferences.Count(d => d.KindCode == DifferenceKindCodes.OnlyInTarget)
            .ShouldBe(backward.SchemaDifferences.Count(d => d.KindCode == DifferenceKindCodes.OnlyInSource));
        forward.SchemaDifferences.Count(d => d.KindCode == DifferenceKindCodes.Modified)
            .ShouldBe(backward.SchemaDifferences.Count(d => d.KindCode == DifferenceKindCodes.Modified));
    }

    // islevi: Tek kolonu source/target tablosuna sarip karsilastirir; kolon-alani testlerinin ortak kurulumu.
    private static ComparisonFindings CompareOneColumn(SchemaColumnModel source, SchemaColumnModel target)
        => CreateManager().Compare(
            Snapshot(Table("public", "t", source)),
            Snapshot(Table("public", "t", target)),
            new List<ComparisonScopeRule>());

    // islevi: Determinizm testinde finding nesnelerini kararli deger tuple'larina indirger.
    private static List<(string SchemaName, string ObjectName, string? ChildName, string ObjectTypeCode, string KindCode, string ConfidenceCode, string? SourceDefinition, string? TargetDefinition, string? ChangeSummary)>
        BuildFindingSignatures(ComparisonFindings findings)
        => findings.SchemaDifferences
            .Select(difference => (
                difference.SchemaName,
                difference.ObjectName,
                difference.ChildName,
                difference.ObjectTypeCode,
                difference.KindCode,
                difference.ConfidenceCode,
                difference.SourceDefinition,
                difference.TargetDefinition,
                difference.ChangeSummary))
            .ToList();

    // islevi: Tam olarak bir kolon Modified bulgusu bekler ve ChangeSummary'nin beklenen etiketi icermesini dogrular.
    private static void AssertColumnModified(ComparisonFindings findings, string columnName, string expectedLabel)
    {
        var difference = findings.SchemaDifferences.ShouldHaveSingleItem();
        difference.ObjectTypeCode.ShouldBe(SchemaObjectTypeCodes.Column);
        difference.ChildName.ShouldBe(columnName);
        difference.KindCode.ShouldBe(DifferenceKindCodes.Modified);
        difference.ChangeSummary.ShouldNotBeNull();
        difference.ChangeSummary!.ShouldContain(expectedLabel);
    }

    // islevi: Her fark alanini dolduran zengin bir snapshot uretir; ayni degerlerle iki kez cagrilinca self-compare=0 kaniti verir.
    private static SchemaSnapshotModel RichSnapshot()
        => Snapshot(
            new List<SchemaTableModel>
            {
                Table(
                    "public",
                    "orders",
                    columns: new[]
                    {
                        Column("id", "integer", isNullable: false, isIdentity: true, ordinal: 1),
                        Column("code", "character varying", isNullable: false, maxLength: 20, ordinal: 2),
                        Column("total", "numeric", isNullable: true, numericPrecision: 18, numericScale: 2, defaultValueSql: "0", ordinal: 3)
                    },
                    indexes: new[]
                    {
                        Index("IX_orders_code", columns: ["code"], includedColumns: ["total"], isUnique: true, filterDefinition: "total > 0")
                    },
                    constraints: new[]
                    {
                        Constraint("PK_orders", SchemaConstraintTypeCodes.PrimaryKey, ["id"]),
                        Constraint("CK_orders_total", SchemaConstraintTypeCodes.Check, [], "total >= 0")
                    },
                    triggers: new[]
                    {
                        Trigger("trg_orders_audit", "CREATE TRIGGER trg_orders_audit BEFORE INSERT ON orders EXECUTE FUNCTION audit()")
                    })
            },
            new List<SchemaObjectDefinitionModel>
            {
                ObjectDefinition("public", "v_orders", SchemaObjectTypeCodes.View, "CREATE VIEW public.v_orders AS SELECT id, code FROM public.orders")
            });

    // islevi: Test icin manager ve bagimli saf servisleri kurar.
    private static SchemaComparisonManager CreateManager()
    {
        var normalizer = new SchemaDefinitionNormalizer();
        return new SchemaComparisonManager(
            new ComparisonScopeRuleEvaluator(),
            new SchemaCollectionComparer(normalizer),
            normalizer,
            new ColumnTypeConfidenceResolver());
    }

    // islevi: Test snapshot'ini tablo ve tablo disi nesnelerle kurar.
    private static SchemaSnapshotModel Snapshot(params SchemaTableModel[] tables)
        => Snapshot(tables.ToList(), new List<SchemaObjectDefinitionModel>());

    // islevi: Capraz-motor testleri icin belirtilen engine koduyla tablo snapshot'i kurar.
    private static SchemaSnapshotModel SnapshotForEngine(string engineCode, params SchemaTableModel[] tables)
        => Snapshot(engineCode, tables.ToList(), new List<SchemaObjectDefinitionModel>());

    // islevi: Test snapshot'ina veritabani varsayilan collation bilgisini ekler.
    private static SchemaSnapshotModel SnapshotWithCollation(string databaseCollation, params SchemaTableModel[] tables)
    {
        var snapshot = Snapshot(tables);
        snapshot.DatabaseCollationName = databaseCollation;
        return snapshot;
    }

    // islevi: Test snapshot'ini tek tablo disi nesneyle kurar.
    private static SchemaSnapshotModel Snapshot(SchemaObjectDefinitionModel objects)
        => Snapshot(new List<SchemaTableModel>(), new List<SchemaObjectDefinitionModel> { objects });

    // islevi: Test snapshot'ini tablo dizisi ve opsiyonel tek tablo disi nesneyle kurar.
    private static SchemaSnapshotModel Snapshot(
        SchemaTableModel table,
        SchemaObjectDefinitionModel objects)
        => Snapshot(new List<SchemaTableModel> { table }, new List<SchemaObjectDefinitionModel> { objects });

    // islevi: Test snapshot'inin ortak alanlarini tek yerde doldurur.
    private static SchemaSnapshotModel Snapshot(
        List<SchemaTableModel> tables,
        List<SchemaObjectDefinitionModel> objects)
        => Snapshot(DatabaseEngineCodes.PostgreSql, tables, objects);

    // islevi: Test snapshot'inin engine kodu dahil ortak alanlarini tek yerde doldurur.
    private static SchemaSnapshotModel Snapshot(
        string engineCode,
        List<SchemaTableModel> tables,
        List<SchemaObjectDefinitionModel> objects)
        => new()
        {
            EngineCode = engineCode,
            DatabaseName = "test_db",
            CollectedAt = DateTime.UtcNow,
            Tables = tables,
            Objects = objects
        };

    // islevi: Test icin sema/tablo modelini opsiyonel alt nesnelerle kurar.
    private static SchemaTableModel Table(string schema, string name, params SchemaColumnModel[] columns)
        => new()
        {
            Schema = schema,
            Name = name,
            Columns = columns.ToList()
        };

    // islevi: Test icin sema/tablo modelini opsiyonel alt nesnelerle kurar.
    private static SchemaTableModel Table(
        string schema,
        string name,
        SchemaColumnModel[]? columns = null,
        SchemaIndexModel[]? indexes = null,
        SchemaConstraintModel[]? constraints = null,
        SchemaTriggerModel[]? triggers = null)
        => new()
        {
            Schema = schema,
            Name = name,
            Columns = columns?.ToList() ?? new List<SchemaColumnModel>(),
            Indexes = indexes?.ToList() ?? new List<SchemaIndexModel>(),
            Constraints = constraints?.ToList() ?? new List<SchemaConstraintModel>(),
            Triggers = triggers?.ToList() ?? new List<SchemaTriggerModel>()
        };

    // islevi: Test icin kolon modelini tum atomik ozellikleriyle (uzunluk/precision/scale/identity/default) kurar.
    private static SchemaColumnModel Column(
        string name,
        string rawDataType,
        bool isNullable,
        int? maxLength = null,
        int? numericPrecision = null,
        int? numericScale = null,
        bool isIdentity = false,
        string? defaultValueSql = null,
        int ordinal = 0,
        string? canonicalDataType = null,
        string? fidelityCode = null)
        => new()
        {
            Name = name,
            RawDataType = rawDataType,
            IsNullable = isNullable,
            MaxLength = maxLength,
            NumericPrecision = numericPrecision,
            NumericScale = numericScale,
            IsIdentity = isIdentity,
            DefaultValueSql = defaultValueSql,
            Ordinal = ordinal,
            CanonicalDataType = canonicalDataType,
            TypeMappingFidelityCode = fidelityCode
        };

    // islevi: Test kolonuna KBP-703 collation/generated/identity/comment derinlik alanlarini ekler.
    private static SchemaColumnModel ColumnWithDepth(
        string name,
        string rawDataType = "integer",
        bool isIdentity = false,
        string? collationName = null,
        bool isGenerated = false,
        string? generationExpression = null,
        bool isPersisted = false,
        string? identitySeed = null,
        string? identityIncrement = null,
        string? comment = null)
    {
        var column = Column(name, rawDataType, isNullable: false, isIdentity: isIdentity);
        column.CollationName = collationName;
        column.IsGenerated = isGenerated;
        column.GenerationExpression = generationExpression;
        column.IsPersisted = isPersisted;
        column.IdentitySeed = identitySeed;
        column.IdentityIncrement = identityIncrement;
        column.Comment = comment;
        return column;
    }

    // islevi: Cross-engine test kolonuna kesin kanonik tip eslemesi ekler.
    private static void SetCanonicalType(SchemaColumnModel column)
    {
        column.CanonicalDataType = CanonicalDataTypeCodes.Integer;
        column.TypeMappingFidelityCode = TypeMappingFidelityCodes.Exact;
    }

    // islevi: Test icin index modelini tekillik/PK/filtre/tanim ozellikleriyle kurar.
    private static SchemaIndexModel Index(
        string name,
        string[] columns,
        string[] includedColumns,
        bool isUnique = false,
        bool isPrimaryKey = false,
        string? filterDefinition = null,
        string? definition = null)
        => new()
        {
            Name = name,
            Columns = columns.ToList(),
            IncludedColumns = includedColumns.ToList(),
            IsUnique = isUnique,
            IsPrimaryKey = isPrimaryKey,
            FilterDefinition = filterDefinition,
            Definition = definition
        };

    // islevi: Test icin FK olmayan constraint modelini (PK/Unique/Check) kurar.
    private static SchemaConstraintModel Constraint(
        string name,
        string typeCode,
        string[] columns,
        string? definition = null)
        => new()
        {
            Name = name,
            TypeCode = typeCode,
            Columns = columns.ToList(),
            Definition = definition,
            DeleteActionCode = SchemaReferentialActionCodes.NoAction,
            UpdateActionCode = SchemaReferentialActionCodes.NoAction
        };

    // islevi: Test icin constraint modelini kurar.
    private static SchemaConstraintModel Constraint(
        string name,
        string typeCode,
        string[] columns,
        string referencedTable,
        string[] referencedColumns,
        string deleteAction)
        => new()
        {
            Name = name,
            TypeCode = typeCode,
            Columns = columns.ToList(),
            ReferencedTable = referencedTable,
            ReferencedColumns = referencedColumns.ToList(),
            DeleteActionCode = deleteAction,
            UpdateActionCode = SchemaReferentialActionCodes.NoAction
        };

    // islevi: Test icin trigger modelini kurar.
    private static SchemaTriggerModel Trigger(string name, string definition, bool isEnabled = true)
        => new()
        {
            Name = name,
            Definition = definition,
            IsEnabled = isEnabled
        };

    // islevi: Test icin tablo disi nesne modelini kurar.
    private static SchemaObjectDefinitionModel ObjectDefinition(
        string schema,
        string name,
        string objectTypeCode,
        string definition)
        => new()
        {
            Schema = schema,
            Name = name,
            ObjectTypeCode = objectTypeCode,
            Definition = definition
        };

    // islevi: Test icin scope kuralini (opsiyonel nesne/child hedefiyle) kurar.
    private static ComparisonScopeRule Rule(
        string scopeKindCode,
        string schemaName,
        string? objectName = null,
        string? childName = null)
        => new()
        {
            ScopeKindCode = scopeKindCode,
            SchemaName = schemaName,
            ObjectName = objectName,
            ChildName = childName
        };
}

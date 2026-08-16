namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: DBMS katalog okuma sirasinda kullanilan sistem semasi, tablo adi ve kolon adi sabitlerini tek kaynakta toplar.
// sistemdeki gorevi: PostgreSQL/SQL Server discovery repository ve konfigurasyonlarinda ayni sabitler kullanilir; magic string ve daginik katalog kodlari engellenir.
public static class DatabaseMetadataCatalogConstants
{
    // Batch metadata/data sorgularini tek result-set'te birlestiren SQL token'i.
    public const string UnionAllSeparator = " union all ";

    public static class PostgreSql
    {
        // PostgreSQL identifier quote karakteri.
        public const string IdentifierQuote = "\"";

        // Quoted identifier icindeki quote karakterinin escape edilmis hali.
        public const string EscapedIdentifierQuote = "\"\"";

        // PostgreSQL sistem katalog semasi; kullanici scope secimine dahil edilmez.
        public const string PgCatalogSchema = "pg_catalog";

        // SQL standardi metadata semasi; kullanici scope secimine dahil edilmez.
        public const string InformationSchema = "information_schema";

        // PostgreSQL TOAST ic semalari; kullanici scope secimine dahil edilmez.
        public const string ToastSchemaPrefix = "pg_toast";

        // PostgreSQL "pg_" onekini tum sistem/gecici semalar (pg_catalog, pg_toast, pg_temp_N, pg_toast_temp_N) icin rezerve eder; kullanici semasi bu onekle baslayamaz, scope secimine hicbiri dahil edilmez.
        public const string SystemSchemaPrefix = "pg_";

        #region Katalog Tablo Adlari

        // pg_namespace: sema listesini tutan sistem tablosu.
        public const string PgNamespaceTable = "pg_namespace";

        // pg_class: tablo/view/sequence/index nesnelerini tutan sistem tablosu.
        public const string PgClassTable = "pg_class";

        // pg_proc: fonksiyon ve procedure'leri tutan sistem tablosu.
        public const string PgProcTable = "pg_proc";

        // pg_trigger: trigger'lari tutan sistem tablosu.
        public const string PgTriggerTable = "pg_trigger";

        // pg_attribute: kolon bilgilerini tutan sistem tablosu.
        public const string PgAttributeTable = "pg_attribute";

        // pg_attrdef: kolon default expression bilgilerini tutan sistem tablosu.
        public const string PgAttributeDefaultTable = "pg_attrdef";

        // pg_type: veri tiplerini tutan sistem tablosu.
        public const string PgTypeTable = "pg_type";

        // pg_index: indeks bilgilerini tutan sistem tablosu.
        public const string PgIndexTable = "pg_index";

        // pg_constraint: kisitlamalari (FK, PK, unique, check) tutan sistem tablosu.
        public const string PgConstraintTable = "pg_constraint";

        // pg_sequence: sequence ayarlarini tutan sistem tablosu.
        public const string PgSequenceTable = "pg_sequence";

        // pg_enum: enum tip etiketlerini tutan sistem tablosu.
        public const string PgEnumTable = "pg_enum";

        // pg_extension: kurulu extension'lari tutan sistem tablosu.
        public const string PgExtensionTable = "pg_extension";

        // pg_database: veritabani ad ve varsayilan collation bilgisini tutar.
        public const string PgDatabaseTable = "pg_database";

        // pg_collation: collation ad ve provider bilgisini tutar.
        public const string PgCollationTable = "pg_collation";

        // pg_depend: identity kolon ile sahip sequence arasindaki dependency'yi tutar.
        public const string PgDependTable = "pg_depend";

        // pg_description: kolon ve nesne aciklamalarini tutar.
        public const string PgDescriptionTable = "pg_description";

        // pg_settings: etkin server/session setting degerlerini salt-okuma olarak tutar.
        public const string PgSettingsTable = "pg_settings";

        #endregion

        #region Ortak Kolon Adlari

        // Nesne kimligi; pg_class, pg_namespace, pg_proc, pg_trigger, pg_type, pg_constraint tablolarinin birincil anahtari.
        public const string OidColumn = "oid";

        // PostgreSQL oid veri tipi; Npgsql uint CLR property'lerini yalnizca HasColumnType("oid") ile dogru okur, aksi halde Int64 uyumsuzlugu firlatir.
        public const string OidColumnType = "oid";

        // pg_settings ayar adi kolonu.
        public const string SettingNameColumn = "name";

        // pg_settings etkin ayar degeri kolonu.
        public const string SettingValueColumn = "setting";

        // PostgreSQL internal "char" (tek byte) veri tipi; Npgsql string CLR property'lerini yalnizca HasColumnType ile dogru okur, aksi halde char/String uyumsuzlugu firlatir. relkind, contype, prokind, attidentity, confdeltype, confupdtype alanlari bu tiptedir.
        public const string InternalCharColumnType = "\"char\"";

        #endregion

        #region pg_namespace Kolon Adlari

        // Sema adi.
        public const string NspNameColumn = "nspname";

        #endregion

        #region pg_class Kolon Adlari

        // Nesne adi (tablo/view/sequence adi).
        public const string RelNameColumn = "relname";

        // Nesne turu kodu (r = tablo, v = view, S = sequence, m = materialized view, p = partitioned table).
        public const string RelKindColumn = "relkind";

        // Ait oldugu sema kimligi (pg_namespace.oid).
        public const string RelNamespaceColumn = "relnamespace";

        #endregion

        #region pg_proc Kolon Adlari

        // Fonksiyon/procedure adi.
        public const string ProNameColumn = "proname";

        // Fonksiyon/procedure turu kodu (f = function, p = procedure).
        public const string ProKindColumn = "prokind";

        // Ait oldugu sema kimligi.
        public const string ProNamespaceColumn = "pronamespace";

        #endregion

        #region pg_trigger Kolon Adlari

        // Trigger adi.
        public const string TgNameColumn = "tgname";

        // Trigger'in bagli oldugu tablo kimligi (pg_class.oid).
        public const string TgRelIdColumn = "tgrelid";

        // Dahili (internal) trigger mi.
        public const string TgIsInternalColumn = "tgisinternal";

        // Trigger'in etkinlik durum kodu.
        public const string TgEnabledColumn = "tgenabled";

        #endregion

        #region pg_attribute Kolon Adlari

        // Ait oldugu tablo kimligi (pg_class.oid).
        public const string AttRelIdColumn = "attrelid";

        // Kolon adi.
        public const string AttNameColumn = "attname";

        // Kolonun veri tip kimligi (pg_type.oid).
        public const string AttTypIdColumn = "atttypid";

        // Kolon sirasi (> 0 kullanici kolonu, <= 0 sistem kolonu).
        public const string AttNumColumn = "attnum";

        // NOT NULL kisitlamasi var mi.
        public const string AttNotNullColumn = "attnotnull";

        // Default degeri var mi.
        public const string AttHasDefColumn = "atthasdef";

        // Kolon silinmis mi (DROP COLUMN sonrasi).
        public const string AttIsDroppedColumn = "attisdropped";

        // Tip-e-ozel uzunluk/hassasiyet/olcek bilgisi; varchar icin (atttypmod - 4) = MaxLength.
        public const string AttTypModColumn = "atttypmod";

        // Identity kolon turu ('a' = always, 'd' = by default, '' = degil).
        public const string AttIdentityColumn = "attidentity";

        // Generated kolon durum kodu.
        public const string AttGeneratedColumn = "attgenerated";

        // Kolonun pg_collation oid kimligi.
        public const string AttCollationColumn = "attcollation";

        #endregion

        #region pg_type Kolon Adlari

        // Veri tipi adi.
        public const string TypNameColumn = "typname";

        // Tipin ait oldugu sema kimligi (pg_namespace.oid).
        public const string TypNamespaceColumn = "typnamespace";

        // Tip tur kodu (e = enum, d = domain, b = base, ...).
        public const string TypTypeColumn = "typtype";

        // Domain tipinin dayandigi temel tip kimligi (pg_type.oid); domain degilse 0.
        public const string TypBaseTypeColumn = "typbasetype";

        #endregion

        #region pg_sequence Kolon Adlari

        // Sequence'in bagli oldugu relation kimligi (pg_class.oid).
        public const string SeqRelIdColumn = "seqrelid";

        // Sequence baslangic degeri.
        public const string SeqStartColumn = "seqstart";

        // Sequence artis miktari.
        public const string SeqIncrementColumn = "seqincrement";

        // Sequence maksimum degeri.
        public const string SeqMaxColumn = "seqmax";

        // Sequence minimum degeri.
        public const string SeqMinColumn = "seqmin";

        // Sequence cache boyutu.
        public const string SeqCacheColumn = "seqcache";

        // Sequence cycle davranisi.
        public const string SeqCycleColumn = "seqcycle";

        #endregion

        #region pg_enum Kolon Adlari

        // Etiketin ait oldugu enum tip kimligi (pg_type.oid).
        public const string EnumTypeIdColumn = "enumtypid";

        // Enum etiketinin siralama degeri.
        public const string EnumSortOrderColumn = "enumsortorder";

        // Enum etiket metni.
        public const string EnumLabelColumn = "enumlabel";

        #endregion

        #region pg_extension Kolon Adlari

        // Extension adi.
        public const string ExtNameColumn = "extname";

        // Extension'in kurulu oldugu sema kimligi (pg_namespace.oid).
        public const string ExtNamespaceColumn = "extnamespace";

        // Extension versiyonu.
        public const string ExtVersionColumn = "extversion";

        #endregion

        #region pg_index Kolon Adlari

        // Indeks nesne kimligi (pg_class.oid).
        public const string IndRelIdColumn = "indexrelid";

        // Indeksin ait oldugu tablo kimligi (pg_class.oid).
        public const string IndTableRelIdColumn = "indrelid";

        // Unique indeks mi.
        public const string IndIsUniqueColumn = "indisunique";

        // Primary key mi.
        public const string IndIsPrimaryColumn = "indisprimary";

        // Anahtar kolon sayisi; INCLUDE kolonlarini ayirt etmek icin kullanilir (PG 11+).
        public const string IndNKeyAttsColumn = "indnkeyatts";

        // Index kolon numaralari (int2vector); ilk IndNKeyAtts adet anahtar, kalani INCLUDE kolonudur.
        public const string IndKeyColumn = "indkey";

        // Partial index predicate expression'i.
        public const string IndPredicateColumn = "indpred";

        #endregion

        #region pg_constraint Kolon Adlari

        // Kisitlama adi.
        public const string ConNameColumn = "conname";

        // Kisitlama turu (c = check, f = FK, p = PK, u = unique).
        public const string ConTypeColumn = "contype";

        // Kisitlamanin ait oldugu tablo kimligi.
        public const string ConRelIdColumn = "conrelid";

        // FK ise referans aldigi tablo kimligi.
        public const string ConFRelIdColumn = "confrelid";

        // Constraint kaynak kolon numaralari.
        public const string ConKeyColumn = "conkey";

        // FK hedef kolon numaralari.
        public const string ConFKeyColumn = "confkey";

        // FK ON UPDATE davranis kodu.
        public const string ConFUpdateTypeColumn = "confupdtype";

        // FK ON DELETE davranis kodu.
        public const string ConFDeleteTypeColumn = "confdeltype";

        // Kisit mevcut veriye karsi dogrulanmis mi.
        public const string ConValidatedColumn = "convalidated";

        // Kisit ertelenebilir mi.
        public const string ConDeferrableColumn = "condeferrable";

        // Kisit varsayilan olarak ertelenmis mi.
        public const string ConDeferredColumn = "condeferred";

        #endregion

        #region pg_attrdef Kolon Adlari

        // Default'un ait oldugu tablo kimligi.
        public const string AdRelIdColumn = "adrelid";

        // Default'un ait oldugu kolon numarasi.
        public const string AdNumColumn = "adnum";

        // Default expression'in PostgreSQL internal parse tree degeri.
        public const string AdBinColumn = "adbin";

        #endregion

        #region pg_database / pg_collation Kolon Adlari

        public const string DatNameColumn = "datname";
        public const string DatCollateColumn = "datcollate";
        public const string CollNameColumn = "collname";
        public const string CollProviderColumn = "collprovider";

        #endregion

        #region pg_depend Kolon Adlari

        public const string DependClassIdColumn = "classid";
        public const string DependObjectIdColumn = "objid";
        public const string DependObjectSubIdColumn = "objsubid";
        public const string DependReferencedClassIdColumn = "refclassid";
        public const string DependReferencedObjectIdColumn = "refobjid";
        public const string DependReferencedObjectSubIdColumn = "refobjsubid";
        public const string DependTypeColumn = "deptype";

        #endregion

        #region pg_description Kolon Adlari

        public const string DescriptionObjectIdColumn = "objoid";
        public const string DescriptionCatalogIdColumn = "classoid";
        public const string DescriptionSubObjectIdColumn = "objsubid";
        public const string DescriptionTextColumn = "description";

        #endregion

        #region RelKind Kodlari

        // Normal tablo relkind kodu.
        public const string TableRelKind = "r";

        // Partitioned table relkind kodu.
        public const string PartitionedTableRelKind = "p";

        // View relkind kodu.
        public const string ViewRelKind = "v";

        // Materialized view relkind kodu; T4 listesinde view ailesine dahil edilir.
        public const string MaterializedViewRelKind = "m";

        // Sequence relkind kodu.
        public const string SequenceRelKind = "S";

        #endregion

        #region Tip Tur Kodlari

        // Enum type typtype kodu.
        public const string EnumTypeKind = "e";

        // Domain type typtype kodu.
        public const string DomainTypeKind = "d";

        #endregion

        #region ProKind Kodlari

        // Function prokind kodu.
        public const string FunctionProKind = "f";

        // Procedure prokind kodu.
        public const string ProcedureProKind = "p";

        #endregion

        #region Constraint Type Kodlari

        // Foreign key kisitlama turu.
        public const string ForeignKeyConType = "f";

        // Primary key kisitlama turu.
        public const string PrimaryKeyConType = "p";

        // Unique kisitlama turu.
        public const string UniqueConType = "u";

        // Check kisitlama turu.
        public const string CheckConType = "c";

        #endregion

        #region Referential Action Kodlari

        // NO ACTION davranisi.
        public const string NoActionReferentialAction = "a";

        // RESTRICT davranisi.
        public const string RestrictReferentialAction = "r";

        // CASCADE davranisi.
        public const string CascadeReferentialAction = "c";

        // SET NULL davranisi.
        public const string SetNullReferentialAction = "n";

        // SET DEFAULT davranisi.
        public const string SetDefaultReferentialAction = "d";

        #endregion

        // Index relkind kodu; pg_class'taki indeks nesnelerini ayirt eder.
        public const string IndexRelKind = "i";

        // Stored generated kolon durum kodu.
        public const string StoredGeneratedKind = "s";

        // Devre disi trigger durum kodu.
        public const string DisabledTriggerStatus = "D";

        // Identity sequence'in tablo/kolon dependency turu.
        public const string InternalDependencyType = "i";

        #region Tip Adi Kodlari (atttypmod cozumleme icin)

        // varchar tip adi (pg_type.typname); atttypmod dogrudan azami karakter uzunlugunu verir.
        public const string VarCharTypeName = "varchar";

        // char(n) tip adi (pg_type icinde bpchar); atttypmod dogrudan azami karakter uzunlugunu verir.
        public const string CharTypeName = "bpchar";

        // numeric/decimal tip adi (pg_type.typname); atttypmod'un ust 16 biti precision, alt 16 biti scale'dir.
        public const string NumericTypeName = "numeric";

        #endregion

        #region atttypmod Cozumleme Sabitleri

        // atttypmod, uzunluk/precision bilgisini VARHDRSZ (4 bayt) basligiyla tasir; gercek deger icin bu offset cikarilir.
        public const int TypeModifierHeaderSize = 4;

        // numeric atttypmod'unda precision, (typmod - header) degerinin ust 16 bitindedir; bu kadar saga kaydirilir.
        public const int NumericPrecisionShift = 16;

        // numeric atttypmod'unda alt 16 biti (precision/scale) ayiklamak icin maske.
        public const int TypeModifierLowWordMask = 0xFFFF;

        #endregion
    }

    public static class SqlServer
    {
        // SQL Server'in ad verilmemis PK/UQ/CK/DF/FK nesneleri icin urettigi sistem adi kalibi.
        public const string SystemGeneratedNamePattern = @"^(PK|UQ|CK|DF|FK)__.+__[0-9A-Fa-f]{8,}$";

        // SQL Server quoted identifier kapanis karakteri.
        public const string IdentifierClose = "]";

        // Quoted identifier icindeki kapanis karakterinin escape edilmis hali.
        public const string EscapedIdentifierClose = "]]";

        // SQL Server sistem katalog semasi; katalog tablolari bu semadan okunur.
        public const string SystemSchema = "sys";

        // sys.identity_columns ve sys.extended_properties degerlerinin SQL Server kolon tipi.
        public const string SqlVariantColumnType = "sql_variant";

        // SQL standardi metadata semasi; sequence detaylari sql_variant'a girmemek icin buradan okunur.
        public const string InformationSchema = "INFORMATION_SCHEMA";

        // SQL Server sistem schema_id ust siniri; kullanici semalari bu degerin altindaki rol/sistem semalarindan ayrilir.
        public const int SystemSchemaIdLimit = 16384;

        // SQL Server sistem/rol semalari; kullanici scope secimine dahil edilmez.
        public static readonly string[] SystemSchemaNames =
        {
            "sys",
            "INFORMATION_SCHEMA",
            "guest",
            "db_owner",
            "db_accessadmin",
            "db_securityadmin",
            "db_ddladmin",
            "db_backupoperator",
            "db_datareader",
            "db_datawriter",
            "db_denydatareader",
            "db_denydatawriter"
        };

        #region Katalog Tablo Adlari

        // sys.schemas tablosu.
        public const string SchemasTable = "schemas";

        // sys.objects tablosu.
        public const string ObjectsTable = "objects";

        // sys.triggers tablosu.
        public const string TriggersTable = "triggers";

        // sys.columns tablosu.
        public const string ColumnsTable = "columns";

        // sys.types tablosu.
        public const string TypesTable = "types";

        // sys.indexes tablosu.
        public const string IndexesTable = "indexes";

        // sys.index_columns tablosu.
        public const string IndexColumnsTable = "index_columns";

        // sys.foreign_keys tablosu.
        public const string ForeignKeysTable = "foreign_keys";

        // sys.foreign_key_columns tablosu.
        public const string ForeignKeyColumnsTable = "foreign_key_columns";

        // sys.default_constraints tablosu.
        public const string DefaultConstraintsTable = "default_constraints";

        // sys.check_constraints tablosu.
        public const string CheckConstraintsTable = "check_constraints";

        // sys.databases tablosu.
        public const string DatabasesTable = "databases";

        // sys.computed_columns tablosu.
        public const string ComputedColumnsTable = "computed_columns";

        // sys.identity_columns tablosu.
        public const string IdentityColumnsTable = "identity_columns";

        // sys.extended_properties tablosu.
        public const string ExtendedPropertiesTable = "extended_properties";

        #endregion

        #region Ortak Kolon Adlari

        // Nesne adi; schemas.name, objects.name, triggers.name.
        public const string NameColumn = "name";

        #endregion

        #region sys.schemas Kolon Adlari

        // Sema kimligi.
        public const string SchemaIdColumn = "schema_id";

        // Veritabani kimligi.
        public const string DatabaseIdColumn = "database_id";

        #endregion

        #region sys.objects Kolon Adlari

        // Nesne kimligi.
        public const string ObjectIdColumn = "object_id";

        // Nesne turu kodu (U, V, P, FN, IF, TF).
        public const string TypeColumn = "type";

        // Microsoft tarafindan olusturulmus mu.
        public const string IsMsShippedColumn = "is_ms_shipped";

        #endregion

        #region sys.triggers Kolon Adlari

        // Trigger'in bagli oldugu parent nesne kimligi.
        public const string ParentIdColumn = "parent_id";

        #endregion

        #region sys.columns Kolon Adlari

        // Kolon kimligi (tablo icinde benzersiz sira numarasi).
        public const string ColumnIdColumn = "column_id";

        // Sistem veri tipi kimligi.
        public const string SystemTypeIdColumn = "system_type_id";

        // Kullanici tanimli veri tipi kimligi.
        public const string UserTypeIdColumn = "user_type_id";

        // Kolon azami uzunlugu (byte cinsinden; nvarchar icin ikiye bolunur).
        public const string MaxLengthColumn = "max_length";

        // Sayisal hassasiyet.
        public const string PrecisionColumn = "precision";

        // Sayisal olcek.
        public const string ScaleColumn = "scale";

        // NULL kabul eder mi.
        public const string IsNullableColumn = "is_nullable";

        // Identity kolonu mu.
        public const string IsIdentityColumn = "is_identity";

        // Hesaplanmis kolon mu.
        public const string IsComputedColumn = "is_computed";

        // Kolon collation adi.
        public const string CollationNameColumn = "collation_name";

        #endregion

        #region sys.types Kolon Adlari

        // Kullanici tanimli tip kimligi.
        public const string UserTypeIdTypesColumn = "user_type_id";

        // Kullanici tanimli tip mi (alias/table type).
        public const string IsUserDefinedColumn = "is_user_defined";

        // Table type mu.
        public const string IsTableTypeColumn = "is_table_type";

        #endregion

        #region sys.indexes Kolon Adlari

        // Indeks kimligi (tablo icinde benzersiz).
        public const string IndexIdColumn = "index_id";

        // Birincil anahtar indeksi mi.
        public const string IsPrimaryKeyColumn = "is_primary_key";

        // Tekillik kisitlamasi var mi.
        public const string IsUniqueColumn = "is_unique";

        // Unique constraint mi (index degil).
        public const string IsUniqueConstraintColumn = "is_unique_constraint";

        // Indeks turu (0=heap, 1=clustered, 2=nonclustered...).
        public const string IndexTypeColumn = "type";

        // Kosullu indeks WHERE ifadesi; yoksa null.
        public const string FilterDefinitionColumn = "filter_definition";

        // Indeks/kisit devre disi mi.
        public const string IsDisabledColumn = "is_disabled";

        #endregion

        #region sys.index_columns Kolon Adlari

        // Kolon kimligi (sys.columns.column_id referansi).
        public const string IndexColumnIdColumn = "column_id";

        // Anahtar icindeki sira numarasi (1'den baslar); 0 = INCLUDE kolonu.
        public const string KeyOrdinalColumn = "key_ordinal";

        // INCLUDE kolonu mu.
        public const string IsIncludedColumnColumn = "is_included_column";

        #endregion

        #region sys.foreign_keys Kolon Adlari

        // FK'nin ait oldugu tablo kimligi.
        public const string ParentObjectIdColumn = "parent_object_id";

        // FK'nin referans aldigi tablo kimligi.
        public const string ReferencedObjectIdColumn = "referenced_object_id";

        // ON DELETE davranisi (0=NO ACTION, 1=CASCADE, 2=SET NULL, 3=SET DEFAULT).
        public const string DeleteReferentialActionColumn = "delete_referential_action";

        // ON UPDATE davranisi.
        public const string UpdateReferentialActionColumn = "update_referential_action";

        // FK mevcut veriye karsi dogrulanmamis mi.
        public const string IsNotTrustedColumn = "is_not_trusted";

        #endregion

        #region sys.foreign_key_columns Kolon Adlari

        // FK kisitlama kimligi.
        public const string ConstraintObjectIdColumn = "constraint_object_id";

        // FK kolon sira numarasi.
        public const string ConstraintColumnIdColumn = "constraint_column_id";

        // Kaynak tablo kolon kimligi.
        public const string ParentColumnIdColumn = "parent_column_id";

        // Hedef tablo kolon kimligi.
        public const string ReferencedColumnIdColumn = "referenced_column_id";

        #endregion

        #region sys.default_constraints Kolon Adlari

        // Default kisitlamanin ait oldugu kolon kimligi.
        public const string DefaultParentColumnIdColumn = "parent_column_id";

        // Default deger SQL ifadesi.
        public const string DefinitionColumn = "definition";

        #endregion

        #region sys.check_constraints Kolon Adlari

        // Check constraint'in ait oldugu tablo kimligi.
        public const string CheckParentObjectIdColumn = "parent_object_id";

        #endregion

        #region sys.computed_columns / sys.identity_columns Kolon Adlari

        public const string IsPersistedColumn = "is_persisted";
        public const string SeedValueColumn = "seed_value";
        public const string IncrementValueColumn = "increment_value";

        #endregion

        #region sys.extended_properties Kolon Adlari

        public const string ExtendedPropertyClassColumn = "class";
        public const string ExtendedPropertyMajorIdColumn = "major_id";
        public const string ExtendedPropertyMinorIdColumn = "minor_id";
        public const string ExtendedPropertyValueColumn = "value";

        // OBJECT_OR_COLUMN extended property sinifi.
        public const int ObjectOrColumnExtendedPropertyClass = 1;

        // Kolon aciklamasi icin yerlesik extended-property adi.
        public const string ColumnDescriptionPropertyName = "MS_Description";

        #endregion

        #region INFORMATION_SCHEMA.SEQUENCES Kolon Adlari

        // Sequence detaylarini tutan standart metadata view'i.
        public const string SequencesView = "SEQUENCES";

        // Sequence semasi.
        public const string SequenceSchemaColumn = "SEQUENCE_SCHEMA";

        // Sequence adi.
        public const string SequenceNameColumn = "SEQUENCE_NAME";

        // Sequence sayisal veri tipi.
        public const string SequenceDataTypeColumn = "DATA_TYPE";

        // Sequence baslangic degeri.
        public const string SequenceStartValueColumn = "START_VALUE";

        // Sequence minimum degeri.
        public const string SequenceMinimumValueColumn = "MINIMUM_VALUE";

        // Sequence maksimum degeri.
        public const string SequenceMaximumValueColumn = "MAXIMUM_VALUE";

        // Sequence artis miktari.
        public const string SequenceIncrementColumn = "INCREMENT";

        // Sequence cycle davranisi.
        public const string SequenceCycleOptionColumn = "CYCLE_OPTION";

        #endregion

        #region Object Type Kodlari

        // Kullanici tablosu object type kodu.
        public const string UserTableObjectType = "U";

        // View object type kodu.
        public const string ViewObjectType = "V";

        // Stored procedure object type kodu.
        public const string ProcedureObjectType = "P";

        // Scalar function object type kodu.
        public const string ScalarFunctionObjectType = "FN";

        // Inline table-valued function object type kodu.
        public const string InlineTableFunctionObjectType = "IF";

        // Table-valued function object type kodu.
        public const string TableFunctionObjectType = "TF";

        // DML trigger object type kodu; nesne listelemede trigger'lari sys.objects uzerinden yakalar.
        public const string TriggerObjectType = "TR";

        // Sequence object type kodu.
        public const string SequenceObjectType = "SO";

        #endregion

        #region Tip Adi Kodlari (uzunluk/precision cozumleme icin)

        // Tek-baytli karakter tipleri; sys.columns.max_length dogrudan azami karakter uzunlugudur.
        public const string VarCharTypeName = "varchar";
        public const string CharTypeName = "char";

        // Binary tipleri; sys.columns.max_length dogrudan azami bayt uzunlugudur.
        public const string VarBinaryTypeName = "varbinary";
        public const string BinaryTypeName = "binary";

        // Unicode karakter tipleri; sys.columns.max_length bayt cinsindendir, karakter icin ikiye bolunur.
        public const string NVarCharTypeName = "nvarchar";
        public const string NCharTypeName = "nchar";

        // Ondalik tipler; precision + scale dogrudan sys.columns'tan gelir.
        public const string DecimalTypeName = "decimal";
        public const string NumericTypeName = "numeric";

        #endregion

        #region Uzunluk Cozumleme Sabitleri

        // sys.columns.max_length -1 ise tip MAX'tir (varchar(max)/nvarchar(max)/varbinary(max)).
        public const int MaxLengthSentinel = -1;

        // Unicode (n-) tiplerinde bir karakter 2 bayttir; max_length bu degere bolunerek karakter uzunluguna cevrilir.
        public const int UnicodeBytesPerChar = 2;

        #endregion

        #region Referential Action Kodlari

        // NO ACTION davranisi.
        public const byte NoActionReferentialAction = 0;

        // CASCADE davranisi.
        public const byte CascadeReferentialAction = 1;

        // SET NULL davranisi.
        public const byte SetNullReferentialAction = 2;

        // SET DEFAULT davranisi.
        public const byte SetDefaultReferentialAction = 3;

        #endregion
    }
}

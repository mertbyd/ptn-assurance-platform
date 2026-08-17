namespace Ptn.DatabaseChecker.Constants;

// islevi: Database Checker HTTP rota ve Swagger grup adlarini kararli paket sozlesmesi olarak tanimlar.
// sistemdeki gorevi: Controller attribute'larindaki tuketiciye acik stringleri Domain.Shared sahibinde toplar.
/// <summary>Database Checker HTTP rota ve Swagger grup sozlesmesi.</summary>
public static class DatabaseCheckerHttpApiConstants
{
    // islevi: Swagger dokuman gruplarini adlandirir.
    // sistemdeki gorevi: Tum controller'larin ayni kararli API Explorer grup adlarini kullanmasini saglar.
    /// <summary>Kararli Swagger grup adlari.</summary>
    public static class Groups
    {
        /// <summary>Capability API grubu.</summary>
        public const string Capabilities = "Capabilities";
        /// <summary>Assertion API grubu.</summary>
        public const string Assertions = "Assertions";
        /// <summary>Karsilastirma API grubu.</summary>
        public const string Comparison = "Comparison";
        /// <summary>Baglanti API grubu.</summary>
        public const string Connections = "Connections";
        /// <summary>Tanim API grubu.</summary>
        public const string Definitions = "Definitions";
        /// <summary>Teshis API grubu.</summary>
        public const string Diagnosis = "Diagnosis";
        /// <summary>Lookup API grubu.</summary>
        public const string Lookups = "Lookups";
        /// <summary>Run API grubu.</summary>
        public const string Runs = "Runs";
        /// <summary>Salt-okunur projection API grubu.</summary>
        public const string Projections = "Projections";
        /// <summary>Sema kesif API grubu.</summary>
        public const string SchemaDiscovery = "SchemaDiscovery";
    }

    // islevi: Controller kok rotalarini adlandirir.
    // sistemdeki gorevi: Public kaynak adreslerinin controller dosyalarina dagilmasini engeller.
    /// <summary>Kararli controller kok rotalari.</summary>
    public static class Routes
    {
        /// <summary>Yazma kumesi capability kaynak rotasi.</summary>
        public const string WriteSetCapabilities = "capabilities/write-set";
        /// <summary>Assertion kaynak rotasi.</summary>
        public const string Assertions = "api/comparison/assertions";
        /// <summary>Karsilastirma tanimi kaynak rotasi.</summary>
        public const string ComparisonDefinitions = "api/definitions/comparison-definitions";
        /// <summary>Karsilastirma run kaynak rotasi.</summary>
        public const string ComparisonRuns = "api/comparison/runs";
        /// <summary>Veritabani baglantisi kaynak rotasi.</summary>
        public const string DatabaseConnections = "api/connections/database-connections";
        /// <summary>Teshis kaynak rotasi.</summary>
        public const string Diagnosis = "api/comparison/diagnosis";
        /// <summary>Salt-okunur projection kaynak rotasi.</summary>
        public const string Projections = "api/comparison/projections";
        /// <summary>Sema karsilastirma rotasi.</summary>
        public const string SchemaComparison = "api/comparison/schema-comparison";
        /// <summary>Sema kesif rotasi.</summary>
        public const string SchemaDiscovery = "api/comparison/schema-discovery";
        /* Lookup rotalarinin oneki modul adidir: iki checker ayni composition host'ta compose
         * edildiginde ortak "api/lookups" isim alani sahipsiz kalir ve ayni adli lookup iki kez
         * kaydedilirse Swagger uretimi ile rota cozumlemesi belirsizlesir. */
        /// <summary>Karsilastirma guveni lookup rotasi.</summary>
        public const string ComparisonConfidences = "api/database-comparison/lookups/comparison-confidences";
        /// <summary>Run durumu lookup rotasi.</summary>
        public const string ComparisonRunStatuses = "api/database-comparison/lookups/comparison-run-statuses";
        /// <summary>Karsilastirma turu lookup rotasi.</summary>
        public const string ComparisonTypes = "api/database-comparison/lookups/comparison-types";
        /// <summary>Veritabani motoru lookup rotasi.</summary>
        public const string DatabaseEngines = "api/database-comparison/lookups/database-engines";
        /// <summary>Fark turu lookup rotasi.</summary>
        public const string DifferenceKinds = "api/database-comparison/lookups/difference-kinds";
        /// <summary>Rapor formati lookup rotasi.</summary>
        public const string ReportFormats = "api/database-comparison/lookups/report-formats";
        /// <summary>Sema nesne turu lookup rotasi.</summary>
        public const string SchemaObjectTypes = "api/database-comparison/lookups/schema-object-types";
        /// <summary>Kapsam turu lookup rotasi.</summary>
        public const string ScopeKinds = "api/database-comparison/lookups/scope-kinds";
    }

    // islevi: Controller alt rota segmentlerini adlandirir.
    // sistemdeki gorevi: Kimlik ve use-case segmentlerinin tum action attribute'larinda ayni yazilmasini saglar.
    /// <summary>Kararli action rota segmentleri.</summary>
    public static class Segments
    {
        /// <summary>Write-set capability yoklama segmenti.</summary>
        public const string WriteSetProbe = "probe";
        /// <summary>Write-set capture segmenti.</summary>
        public const string WriteSetCapture = "capture";
        /// <summary>Write-set kaynak birakma segmenti.</summary>
        public const string WriteSetRelease = "release";
        /// <summary>Kimlikle tek kaynak segmenti.</summary>
        public const string EntityById = "{id}";
        /// <summary>Satir assertion segmenti.</summary>
        public const string RowAssertion = "row";
        /// <summary>Sayim assertion segmenti.</summary>
        public const string CountAssertion = "count";
        /// <summary>Yokluk assertion segmenti.</summary>
        public const string AbsentAssertion = "absent";
        /// <summary>Toplu assertion segmenti.</summary>
        public const string BatchAssertion = "batch";
        /// <summary>Assertion turetilebilirlik segmenti.</summary>
        public const string AssertionDerivability = "derivability";
        /// <summary>Projection satir okuma segmenti.</summary>
        public const string ProjectionRows = "rows";
        /// <summary>Baglanti pasiflestirme segmenti.</summary>
        public const string ConnectionPassivation = "{id}/passivate";
        /// <summary>Baglanti testi segmenti.</summary>
        public const string ConnectionTest = "{id}/test-connection";
        /// <summary>Run detay segmenti.</summary>
        public const string RunDetail = "{id}/detail";
        /// <summary>Run calistirma segmenti.</summary>
        public const string RunExecution = "execute";
        /// <summary>Run rapor segmenti.</summary>
        public const string RunReport = "{id}/report";
        /// <summary>Run bulgu segmenti.</summary>
        public const string RunFindings = "{id}/findings";
        /// <summary>Baglanti semalari segmenti.</summary>
        public const string ConnectionSchemas = "{connectionId}/schemas";
        /// <summary>Baglanti nesneleri segmenti.</summary>
        public const string ConnectionObjects = "{connectionId}/objects";
        /// <summary>Baglanti snapshot segmenti.</summary>
        public const string ConnectionSnapshot = "{connectionId}/snapshot";
        /// <summary>Baglanti sema parmak izi segmenti.</summary>
        public const string ConnectionSchemaFingerprint = "{connectionId}/fingerprint";
        /// <summary>Tek tablo tanimi segmenti.</summary>
        public const string TableDescription = "{connectionId}/tables/{schema}/{table}/describe";
    }
}

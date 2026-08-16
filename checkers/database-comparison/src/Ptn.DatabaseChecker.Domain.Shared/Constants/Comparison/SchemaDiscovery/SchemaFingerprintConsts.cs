namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Merkle sema muhrunun algoritma kimligini, seviye etiketlerini ve bilesen sirasini kararli paket sozlesmesi olarak tanimlar.
// sistemdeki gorevi: Hesaplayici, public cevap kontrati ve PACKAGE-README ayni muhur protokolunu farkli magic string'lerle yazmaz; tuketici eski muhru "kaydi" saymadan once AlgorithmVersion'i bu sahipten okur.
/// <summary>Sema parmak izi algoritma ve kanoniklik sozlesmesi.</summary>
public static class SchemaFingerprintConsts
{
    /// <summary>Muhur formulunun kararli kodu: seviye seviye SHA-256 Merkle zinciri.</summary>
    public const string AlgorithmCode = "SHA256-MERKLE";

    /// <summary>Bilesen kumesi, sirasi veya normalizasyonu degistiginde artan formul surumu.</summary>
    public const int AlgorithmVersion = 1;

    /// <summary>Uretilen buyuk harfli onaltilik SHA-256 muhrunun karakter uzunlugu.</summary>
    public const int FingerprintLength = 64;

    // islevi: Merkle zincirinin dort seviyesini birbirinden ayiran alan-ayrimi etiketlerini tanimlar.
    // sistemdeki gorevi: Bir seviyenin kanonik metni baska bir seviyeninkiyle ayni degeri uretemez.
    /// <summary>Kararli seviye etiketleri.</summary>
    public static class Levels
    {
        /// <summary>Kolon seviyesi etiketi.</summary>
        public const string Column = "column";

        /// <summary>Tablo seviyesi etiketi.</summary>
        public const string Table = "table";

        /// <summary>Sema seviyesi etiketi.</summary>
        public const string Schema = "schema";

        /// <summary>Snapshot seviyesi etiketi.</summary>
        public const string Snapshot = "snapshot";
    }

    // islevi: Her seviyenin muhre giren bilesenlerini degismez sirasiyla ilan eder.
    // sistemdeki gorevi: Tuketici muhru kendi tarafinda yeniden uretmek istedigine bu sirayi kaynak koda bakmadan okur.
    /// <summary>Seviye basina degismez bilesen sirasi.</summary>
    public static class ComponentOrder
    {
        /// <summary>Kolon muhrunun bilesen sirasi.</summary>
        public const string Column =
            "Name,RawDataType,CanonicalDataType,MaxLength,NumericPrecision,NumericScale,IsNullable," +
            "DefaultValueSql,IsGenerated,GenerationExpression,IsPersisted,CollationName,IsIdentity," +
            "IdentitySeed,IdentityIncrement";

        /// <summary>Tablo muhrunun bilesen sirasi.</summary>
        public const string Table =
            "SchemaName,TableName,ColumnFingerprints,Constraints,Indexes,Triggers";

        /// <summary>Sema muhrunun bilesen sirasi.</summary>
        public const string Schema =
            "SchemaName,TableFingerprints,ObjectDefinitions";

        /// <summary>Snapshot muhrunun bilesen sirasi.</summary>
        public const string Snapshot =
            "AlgorithmCode,AlgorithmVersion,EngineCode,DatabaseCollationName,CollationProviderCode," +
            "SchemaFingerprints";
    }
}

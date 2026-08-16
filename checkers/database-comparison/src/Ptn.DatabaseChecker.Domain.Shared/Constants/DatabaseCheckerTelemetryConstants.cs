namespace Ptn.DatabaseChecker.Constants;

// islevi: Database Checker ActivitySource, span ve izinli attribute adlarini kararli gozlemlenebilirlik sozlesmesi olarak tanimlar.
// sistemdeki gorevi: Test Module once/sonra olcumlerinin span adlarini katmanlara dagilan magic stringlerden korur.
/// <summary>Database Checker OpenTelemetry ActivitySource sozlesmesi.</summary>
public static class DatabaseCheckerTelemetryConstants
{
    /// <summary>Consumer tracer provider'in dinleyecegi ActivitySource adi.</summary>
    public const string SourceName = "CheckNexus.DatabaseComparison";

    // islevi: Olculebilir use-case span adlarini gruplar.
    // sistemdeki gorevi: Assertion, diagnosis ve bulgu sorgusu olcumlerini ayni adlarla yayar.
    /// <summary>Kararli Activity span adlari.</summary>
    public static class Activities
    {
        /// <summary>Tek assertion calisma span'i.</summary>
        public const string AssertRow = "checknexus.db.assert.row";
        /// <summary>Toplu assertion calisma span'i.</summary>
        public const string AssertBatch = "checknexus.db.assert.batch";
        /// <summary>Dinamik diagnosis calisma span'i.</summary>
        public const string DiagnosisRun = "checknexus.db.diagnosis.run";
        /// <summary>Sayfali bulgu sorgusu span'i.</summary>
        public const string FindingsQuery = "checknexus.db.findings.query";
    }

    // islevi: Span'larda izinli ve dusuk-kardinaliteli attribute adlarini gruplar.
    // sistemdeki gorevi: Hucre, host, kullanici, secret path ve ham hata mesaji eklenmesini onleyen dar API saglar.
    /// <summary>Kararli ve izinli span attribute adlari.</summary>
    public static class Attributes
    {
        /// <summary>Veritabani motor kodu attribute'u.</summary>
        public const string DatabaseSystemName = "db.system.name";
        /// <summary>Veritabani ad alani attribute'u.</summary>
        public const string DatabaseNamespace = "db.namespace";
        /// <summary>Kararli is sonucu kodu attribute'u.</summary>
        public const string OutcomeCode = "checknexus.outcome_code";
        /// <summary>Assertion deneme sayisi attribute'u.</summary>
        public const string AttemptCount = "checknexus.attempt_count";
        /// <summary>Diagnosis probe sayisi attribute'u.</summary>
        public const string ProbeCount = "checknexus.probe_count";
        /// <summary>Olculen use-case suresi attribute'u.</summary>
        public const string DurationMilliseconds = "checknexus.duration_ms";
    }

    // islevi: Kendi sonuc kodu olmayan olcumlerin dusuk-kardinaliteli sonuc kodlarini gruplar.
    // sistemdeki gorevi: Findings ve bos diagnosis sonucunu ham metne baglamadan etiketler.
    /// <summary>Telemetry-only kararli sonuc kodlari.</summary>
    public static class Outcomes
    {
        /// <summary>Use-case basariyla tamamlandi.</summary>
        public const string Completed = "Completed";
        /// <summary>Diagnosis uygulanabilir hipotez uretmedi.</summary>
        public const string NoHypothesis = "NoHypothesis";
    }
}

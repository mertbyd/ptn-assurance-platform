namespace Ptn.TestModule.ExceptionCodes.Runs;

// islevi: Kosum kaydi, ortam baglama ve terminal yazim invariant hatalarini kodlar.
// sistemdeki gorevi: Beklenen domain retlerini yerellestirilebilir ve makine-okur sozlesmeye baglar.
/// <summary>
/// Test kosum dunyasinin kararli BusinessException kodlarini tasir.
/// </summary>
public static class TestModuleRunErrorCodes
{
    /// <summary>Kosum hata kodlarinin ortak namespace on ekidir.</summary>
    private const string Prefix = "TestModule.Run";

    /// <summary>Istenen mantiksal ortamin tenant ayarinda bulunmadigini bildirir.</summary>
    public const string EnvironmentNotBound = $"{Prefix}:EnvironmentNotBound";

    /// <summary>API ve veritabani hedeflerinin farkli ortam anahtarlari tasidigini bildirir.</summary>
    public const string EnvironmentMismatch = $"{Prefix}:EnvironmentMismatch";

    /// <summary>Kosumun Pending disinda bir durumdan claim edilmeye calisildigini bildirir.</summary>
    public const string RunAlreadyClaimed = $"{Prefix}:RunAlreadyClaimed";

    /// <summary>Ayni kosum ve deneme ciftine ikinci terminal satiri yazildigini bildirir.</summary>
    public const string AttemptAlreadyWritten = $"{Prefix}:AttemptAlreadyWritten";

    /// <summary>Passed hukmunun sorun alani tasidigini bildirir.</summary>
    public const string PassedOutcomeCarriesFailureData = $"{Prefix}:PassedOutcomeCarriesFailureData";

    /// <summary>Trace kimliginin W3C kucuk harfli hex biciminde olmadigini bildirir.</summary>
    public const string InvalidTraceId = $"{Prefix}:InvalidTraceId";

    /// <summary>Diagnosis raporunun 4 KB satir ici sinirini astigini bildirir.</summary>
    public const string DiagnosisReportTooLarge = $"{Prefix}:DiagnosisReportTooLarge";

    /// <summary>Tarihsel kosum kaydinin silinmeye calisildigini bildirir.</summary>
    public const string RunDeletionNotAllowed = $"{Prefix}:RunDeletionNotAllowed";

    /// <summary>Fingerprint degerinin SHA-256 bicimine uymadigini bildirir.</summary>
    public const string InvalidFingerprint = $"{Prefix}:InvalidFingerprint";

    /// <summary>Kayma bulunmadan kayma terminal hukmu uretilmeye calisildigini bildirir.</summary>
    public const string MaterialDriftRequired = $"{Prefix}:MaterialDriftRequired";

    /// <summary>Bulgu kaynaginin desteklenen checker kodlarindan biri olmadigini bildirir.</summary>
    public const string SourceCheckerNotSupported = $"{Prefix}:SourceCheckerNotSupported";

    /// <summary>Kosum suresinin negatif verildigini bildirir.</summary>
    public const string DurationInvalid = $"{Prefix}:DurationInvalid";

    /// <summary>Public kosum girdilerinin kararli FluentValidation hata kodlarini tasir.</summary>
    public static class Validation
    {
        /// <summary>Senaryo kimliginin bos Guid oldugunu bildirir.</summary>
        public const string ScenarioIdInvalid = $"{Prefix}:Validation:ScenarioIdInvalid";

        /// <summary>Test anahtarinin verilmedigini bildirir.</summary>
        public const string TestKeyRequired = $"{Prefix}:Validation:TestKeyRequired";

        /// <summary>Test anahtarinin kalici siniri astigini bildirir.</summary>
        public const string TestKeyTooLong = $"{Prefix}:Validation:TestKeyTooLong";

        /// <summary>Ortam anahtarinin verilmedigini bildirir.</summary>
        public const string EnvironmentKeyRequired = $"{Prefix}:Validation:EnvironmentKeyRequired";

        /// <summary>Ortam anahtarinin kalici siniri astigini bildirir.</summary>
        public const string EnvironmentKeyTooLong = $"{Prefix}:Validation:EnvironmentKeyTooLong";

        /// <summary>Tetikleyici turu kodunun verilmedigini bildirir.</summary>
        public const string TriggerKindRequired = $"{Prefix}:Validation:TriggerKindRequired";

        /// <summary>Tetikleyici turu kodunun kalici siniri astigini bildirir.</summary>
        public const string TriggerKindTooLong = $"{Prefix}:Validation:TriggerKindTooLong";

        /// <summary>Tetikleyici referansinin kalici siniri astigini bildirir.</summary>
        public const string TriggerRefTooLong = $"{Prefix}:Validation:TriggerRefTooLong";

        /// <summary>Kanonik girdilerin verilmedigini bildirir.</summary>
        public const string CanonicalInputsRequired = $"{Prefix}:Validation:CanonicalInputsRequired";

        /// <summary>Fingerprint biciminin gecersiz oldugunu bildirir.</summary>
        public const string FingerprintInvalid = $"{Prefix}:Validation:FingerprintInvalid";

        /// <summary>Runner referansinin kalici siniri astigini bildirir.</summary>
        public const string RunnerRefTooLong = $"{Prefix}:Validation:RunnerRefTooLong";

        /// <summary>Terminal hukum kodunun verilmedigini bildirir.</summary>
        public const string OutcomeRequired = $"{Prefix}:Validation:OutcomeRequired";

        /// <summary>Terminal hukum kodunun kalici siniri astigini bildirir.</summary>
        public const string OutcomeTooLong = $"{Prefix}:Validation:OutcomeTooLong";

        /// <summary>Hata kategorisi kodunun kalici siniri astigini bildirir.</summary>
        public const string FailureCategoryTooLong = $"{Prefix}:Validation:FailureCategoryTooLong";

        /// <summary>Hata kodunun kalici siniri astigini bildirir.</summary>
        public const string ErrorCodeTooLong = $"{Prefix}:Validation:ErrorCodeTooLong";

        /// <summary>Hata ayrintisinin kalici siniri astigini bildirir.</summary>
        public const string DetailTooLong = $"{Prefix}:Validation:DetailTooLong";

        /// <summary>Adim sira numarasinin bir tabanli olmadigini bildirir.</summary>
        public const string StepOrdinalInvalid = $"{Prefix}:Validation:StepOrdinalInvalid";

        /// <summary>Adim adinin kalici siniri astigini bildirir.</summary>
        public const string StepNameTooLong = $"{Prefix}:Validation:StepNameTooLong";

        /// <summary>Adim yolunun kalici siniri astigini bildirir.</summary>
        public const string StepPathTooLong = $"{Prefix}:Validation:StepPathTooLong";

        /// <summary>Dal yolunun kalici siniri astigini bildirir.</summary>
        public const string BranchPathTooLong = $"{Prefix}:Validation:BranchPathTooLong";

        /// <summary>Kosum suresinin negatif oldugunu bildirir.</summary>
        public const string DurationInvalid = $"{Prefix}:Validation:DurationInvalid";

        /// <summary>HAR blob adinin kalici siniri astigini bildirir.</summary>
        public const string HarBlobNameTooLong = $"{Prefix}:Validation:HarBlobNameTooLong";

        /// <summary>Diagnosis raporunun satir ici bayt sinirini astigini bildirir.</summary>
        public const string DiagnosisReportTooLarge = $"{Prefix}:Validation:DiagnosisReportTooLarge";

        /// <summary>Bulgu koleksiyonunun null verildigini bildirir.</summary>
        public const string FindingsRequired = $"{Prefix}:Validation:FindingsRequired";

        /// <summary>Bulgu kaynak checker kodunun verilmedigini bildirir.</summary>
        public const string SourceCheckerRequired = $"{Prefix}:Validation:SourceCheckerRequired";

        /// <summary>Bulgu kaynak checker kodunun desteklenmedigini bildirir.</summary>
        public const string SourceCheckerInvalid = $"{Prefix}:Validation:SourceCheckerInvalid";

        /// <summary>Karsilastirma turu kodunun verilmedigini bildirir.</summary>
        public const string ComparisonKindRequired = $"{Prefix}:Validation:ComparisonKindRequired";

        /// <summary>Karsilastirma turu kodunun kalici siniri astigini bildirir.</summary>
        public const string ComparisonKindTooLong = $"{Prefix}:Validation:ComparisonKindTooLong";

        /// <summary>Kural referansinin kalici siniri astigini bildirir.</summary>
        public const string RuleRefTooLong = $"{Prefix}:Validation:RuleRefTooLong";

        /// <summary>Bulgu konumunun verilmedigini bildirir.</summary>
        public const string LocationRequired = $"{Prefix}:Validation:LocationRequired";

        /// <summary>Bulgu konumunun kalici siniri astigini bildirir.</summary>
        public const string LocationTooLong = $"{Prefix}:Validation:LocationTooLong";

        /// <summary>Hedef gorunen adinin kalici siniri astigini bildirir.</summary>
        public const string TargetDisplayNameTooLong = $"{Prefix}:Validation:TargetDisplayNameTooLong";

        /// <summary>Bulgu mesajinin verilmedigini bildirir.</summary>
        public const string MessageRequired = $"{Prefix}:Validation:MessageRequired";

        /// <summary>Bulgu mesajinin kalici siniri astigini bildirir.</summary>
        public const string MessageTooLong = $"{Prefix}:Validation:MessageTooLong";

        /// <summary>Beklenen veya gozlenen degerin kalici siniri astigini bildirir.</summary>
        public const string ValueTooLong = $"{Prefix}:Validation:ValueTooLong";

        /// <summary>Kanit ozetinin kalici siniri astigini bildirir.</summary>
        public const string EvidenceSummaryTooLong = $"{Prefix}:Validation:EvidenceSummaryTooLong";

        /// <summary>Gozlem zamaninin negatif oldugunu bildirir.</summary>
        public const string ObservedAtInvalid = $"{Prefix}:Validation:ObservedAtInvalid";

        /// <summary>Deneme sayisinin negatif oldugunu bildirir.</summary>
        public const string AttemptCountInvalid = $"{Prefix}:Validation:AttemptCountInvalid";
    }
}

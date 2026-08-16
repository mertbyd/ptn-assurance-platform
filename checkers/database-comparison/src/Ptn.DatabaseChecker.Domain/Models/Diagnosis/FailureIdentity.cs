using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.DatabaseChecker.Constants.Diagnosis;

namespace Ptn.DatabaseChecker.Models.Diagnosis;

// islevi: Hata kodu sinifi, kaynak, kimlik guveni, nesne referanslari ve koddan cikarilmis temel olgulari tasir.
// sistemdeki gorevi: Kurallarin provider mesajina veya hata kodu esitligine bakmadan dogrulanmis katalog olgulariyla calismasini saglar.
public sealed class FailureIdentity
{
    public string SourceKindCode { get; set; } = string.Empty;
    public string EngineCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string CodeClassCode { get; set; } = string.Empty;
    public string ConfidenceCode { get; set; } = DiagnosisConfidenceCodes.Low;
    public bool IndicatesMissingRow { get; set; }
    public bool IndicatesTimedOut { get; set; }
    public bool IndicatesValueMismatch { get; set; }
    public bool IndicatesMissingColumn { get; set; }
    public bool IndicatesUniqueViolation { get; set; }
    public bool IndicatesForeignKeyViolation { get; set; }
    public bool IndicatesGeneratedColumnWrite { get; set; }
    public bool SupportsServerSettingProbe { get; set; }
    public List<ObjectReference> ObjectReferences { get; set; } = new();

    // islevi: Assertion outcome kodunu bir kez temel olgulara cevirip motor-bagimsiz yuksek guvenli kimlik kurar.
    public static FailureIdentity FromAssertion(string engineCode, FailureSignal.AssertionFailureSignal signal)
        => new()
        {
            SourceKindCode = FailureSourceKindCodes.Assertion,
            EngineCode = engineCode,
            Code = signal.OutcomeCode,
            CodeClassCode = FailureCodeClassCodes.Assertion,
            ConfidenceCode = DiagnosisConfidenceCodes.High,
            IndicatesMissingRow = signal.OutcomeCode is AssertionOutcomeCodes.RowNotFound or AssertionOutcomeCodes.TimedOut,
            IndicatesTimedOut = signal.OutcomeCode == AssertionOutcomeCodes.TimedOut,
            IndicatesValueMismatch = signal.OutcomeCode == AssertionOutcomeCodes.ValueMismatch,
            IndicatesMissingColumn = signal.OutcomeCode == AssertionOutcomeCodes.ColumnNotFound,
            ObjectReferences = new List<ObjectReference>
            {
                new() { SchemaName = signal.SchemaName, TableName = signal.TableName }
            }
        };

    // islevi: Dogrulanmayan provider referansi atildiginda kimlik guvenini fail-closed Low seviyesine dusurur.
    public void Downgrade()
    {
        ConfidenceCode = DiagnosisConfidenceCodes.Low;
    }
}

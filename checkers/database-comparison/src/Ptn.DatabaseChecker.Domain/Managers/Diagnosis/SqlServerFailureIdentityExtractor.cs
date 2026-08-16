using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis;

// islevi: SQL Server assertion veya yalniz hata numarasini mesaj sablonu ayrismadan FailureIdentity'ye cikarir.
// sistemdeki gorevi: SQL Server ad cikarimini kapsam disinda tutup database-exception kimligini her zaman Low guvenle sinirlar.
[ExposeServices(typeof(IFailureIdentityExtractor))]
/// <summary>SQL Server failure sinyalini mesaj ayrismadan guven dereceli kimlige cevirir.</summary>
public sealed class SqlServerFailureIdentityExtractor : IFailureIdentityExtractor, ITransientDependency
{
    /// <summary>SQL Server kararli motor kodunu dondurur.</summary>
    public string EngineCode => DatabaseEngineCodes.SqlServer;

    // islevi: Assertion'i ortak olgulara, provider hatasini yalniz numara kodlu dusuk guvenli kimlige cevirir.
    /// <summary>Assertion veya SQL Server hata numarasindan failure kimligi cikarir.</summary>
    public FailureIdentity Extract(FailureSignal signal)
        => signal.Assertion is not null
            ? FailureIdentity.FromAssertion(EngineCode, signal.Assertion)
            : new FailureIdentity
            {
                SourceKindCode = FailureSourceKindCodes.DatabaseException,
                EngineCode = EngineCode,
                Code = signal.DbException!.SqlState,
                CodeClassCode = FailureCodeClassCodes.SqlServerNumber,
                ConfidenceCode = DiagnosisConfidenceCodes.Low
            };
}

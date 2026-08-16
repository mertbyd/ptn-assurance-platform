using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Diagnosis;

// islevi: PostgreSQL assertion veya yapilandirilmis SQLSTATE alanlarini mesaj parse etmeden FailureIdentity'ye cikarir.
// sistemdeki gorevi: SQLSTATE sinif-23 alan garantisini High, diger siniflari Low guvenle isaretleyen motor bilesenidir.
[ExposeServices(typeof(IFailureIdentityExtractor))]
/// <summary>PostgreSQL failure sinyalini yapilandirilmis, guven dereceli kimlige cevirir.</summary>
public sealed class PostgreSqlFailureIdentityExtractor : IFailureIdentityExtractor, ITransientDependency
{
    /// <summary>PostgreSQL kararli motor kodunu dondurur.</summary>
    public string EngineCode => DatabaseEngineCodes.PostgreSql;

    // islevi: Sinyal kaynagina gore ortak assertion veya PostgreSQL yapilandirilmis alan cikarimini secer.
    /// <summary>Assertion veya yapilandirilmis PostgreSQL hatasindan failure kimligi cikarir.</summary>
    public FailureIdentity Extract(FailureSignal signal)
    {
        var identity = signal.Assertion is not null
            ? FailureIdentity.FromAssertion(EngineCode, signal.Assertion)
            : ExtractDatabaseException(signal.DbException!);
        identity.SupportsServerSettingProbe = true;
        return identity;
    }

    // islevi: SQLSTATE sinif guveni ile provider nesne alanlarini tek PostgreSQL kimliginde toplar.
    private FailureIdentity ExtractDatabaseException(FailureSignal.DatabaseExceptionFailureSignal signal)
    {
        var isIntegrityClass = signal.SqlState.StartsWith(
            PostgreSqlSqlStateCodes.IntegrityConstraintClassPrefix,
            System.StringComparison.Ordinal);
        return new FailureIdentity
        {
            SourceKindCode = FailureSourceKindCodes.DatabaseException,
            EngineCode = EngineCode,
            Code = signal.SqlState,
            CodeClassCode = isIntegrityClass
                ? FailureCodeClassCodes.IntegrityConstraint
                : FailureCodeClassCodes.SqlState,
            ConfidenceCode = isIntegrityClass
                ? DiagnosisConfidenceCodes.High
                : DiagnosisConfidenceCodes.Low,
            IndicatesUniqueViolation = signal.SqlState == PostgreSqlSqlStateCodes.UniqueViolation,
            IndicatesForeignKeyViolation = signal.SqlState == PostgreSqlSqlStateCodes.ForeignKeyViolation,
            IndicatesGeneratedColumnWrite = signal.SqlState == PostgreSqlSqlStateCodes.GeneratedAlways,
            ObjectReferences = new() { ExtractObjectReference(signal) }
        };
    }

    // islevi: Npgsql'in yapilandirilmis ad alanlarini providerFields sozlugunden birebir alir; mesaj/Detail ayrismaz.
    private static ObjectReference ExtractObjectReference(
        FailureSignal.DatabaseExceptionFailureSignal signal)
        => new()
        {
            SchemaName = ReadField(signal, PostgreSqlSqlStateCodes.ProviderFields.SchemaName),
            TableName = ReadField(signal, PostgreSqlSqlStateCodes.ProviderFields.TableName),
            ColumnName = ReadField(signal, PostgreSqlSqlStateCodes.ProviderFields.ColumnName),
            ConstraintName = ReadField(signal, PostgreSqlSqlStateCodes.ProviderFields.ConstraintName)
        };

    // islevi: Tek yapilandirilmis provider alanini bos degeri null'a indirerek okur.
    private static string? ReadField(
        FailureSignal.DatabaseExceptionFailureSignal signal,
        string fieldName)
        => signal.ProviderFields.TryGetValue(fieldName, out var value) &&
           !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
}

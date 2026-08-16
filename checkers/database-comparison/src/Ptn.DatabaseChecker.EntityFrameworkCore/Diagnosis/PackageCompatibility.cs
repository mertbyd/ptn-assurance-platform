using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Diagnosis;

// islevi: 0.1.x EF paketindeki PostgreSQL extractor tipini Domain Manager uygulamasina yonlendirir.
// sistemdeki gorevi: Eski assembly/type kimligini korur; runtime DI yalniz yeni manager'i kaydeder.
/// <summary>Eski PostgreSQL failure extractor tipi icin paket uyumluluk kabugu.</summary>
[DisableConventionalRegistration]
[Obsolete("Use Ptn.DatabaseChecker.Managers.Diagnosis.PostgreSqlFailureIdentityExtractor.")]
public sealed class PostgreSqlFailureIdentityExtractor : IFailureIdentityExtractor, ITransientDependency
{
    private readonly global::Ptn.DatabaseChecker.Managers.Diagnosis.PostgreSqlFailureIdentityExtractor _manager = new();

    /// <inheritdoc />
    public string EngineCode => DatabaseEngineCodes.PostgreSql;

    /// <inheritdoc />
    public FailureIdentity Extract(FailureSignal signal) => _manager.Extract(signal);
}

// islevi: 0.1.x EF paketindeki SQL Server extractor tipini Domain Manager uygulamasina yonlendirir.
// sistemdeki gorevi: Eski assembly/type kimligini korur; runtime DI yalniz yeni manager'i kaydeder.
/// <summary>Eski SQL Server failure extractor tipi icin paket uyumluluk kabugu.</summary>
[DisableConventionalRegistration]
[Obsolete("Use Ptn.DatabaseChecker.Managers.Diagnosis.SqlServerFailureIdentityExtractor.")]
public sealed class SqlServerFailureIdentityExtractor : IFailureIdentityExtractor, ITransientDependency
{
    private readonly global::Ptn.DatabaseChecker.Managers.Diagnosis.SqlServerFailureIdentityExtractor _manager = new();

    /// <inheritdoc />
    public string EngineCode => DatabaseEngineCodes.SqlServer;

    /// <inheritdoc />
    public FailureIdentity Extract(FailureSignal signal) => _manager.Extract(signal);
}

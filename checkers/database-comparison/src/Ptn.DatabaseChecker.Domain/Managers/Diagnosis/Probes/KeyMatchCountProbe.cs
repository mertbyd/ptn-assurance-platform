using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Interface.Diagnosis;
using Ptn.DatabaseChecker.Managers.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Diagnosis.Probes;

// islevi: Katalogda dogrulanmis dar kimlik anahtariyla eslesen kesin satir sayisini salt-okuma olarak olcer.
// sistemdeki gorevi: Tam assertion anahtari bulunmazken farkli scope'ta ayni unique kimligin varligini KBP-704 CountByKeyAsync ile kanitlar.
[ExposeServices(typeof(IDiagnosisProbe))]
public sealed class KeyMatchCountProbe : DomainService, IDiagnosisProbe, ITransientDependency
{
    private DatabaseConnectionInfoFactory ConnectionInfoFactory
        => LazyServiceProvider.LazyGetRequiredService<DatabaseConnectionInfoFactory>();

    private IEngineComponentResolver<IDatabaseDataComparisonRepository> RepositoryResolver
        => LazyServiceProvider.LazyGetRequiredService<IEngineComponentResolver<IDatabaseDataComparisonRepository>>();

    public string ProbeKindCode => ProbeKindCodes.KeyMatchCount;

    // islevi: Dogrulanmis kimlik anahtarinin kesin sayimini Found veya Missing kanitina cevirir.
    public async Task<ProbeEvidence> RunAsync(
        DatabaseConnection connection,
        ProbeRequest request,
        ValueRetentionPolicy retentionPolicy,
        CancellationToken cancellationToken = default)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        var count = await repository.CountByKeyAsync(
            info,
            request.Structure!,
            request.KeyValues,
            cancellationToken);
        return new ProbeEvidence
        {
            ProbeKindCode = ProbeKindCode,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = count > 0 ? ProbeKindCodes.Facts.Found : ProbeKindCodes.Facts.Missing,
            ObservedCount = count
        };
    }
}

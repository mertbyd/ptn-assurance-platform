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

// islevi: Katalogda dogrulanmis tablo ve anahtarla en fazla bir satirin varligini salt-okuma olarak yoklar.
// sistemdeki gorevi: RowNeverCreated, RowCreatedLate, FK parent ve unique duplicate kurallarina KBP-704 ReadRowsByKeyAsync kaniti verir.
[ExposeServices(typeof(IDiagnosisProbe))]
public sealed class RowExistsProbe : DomainService, IDiagnosisProbe, ITransientDependency
{
    private DatabaseConnectionInfoFactory ConnectionInfoFactory
        => LazyServiceProvider.LazyGetRequiredService<DatabaseConnectionInfoFactory>();

    private IEngineComponentResolver<IDatabaseDataComparisonRepository> RepositoryResolver
        => LazyServiceProvider.LazyGetRequiredService<IEngineComponentResolver<IDatabaseDataComparisonRepository>>();

    public string ProbeKindCode => ProbeKindCodes.RowExists;

    // islevi: Dogrulanmis anahtarla tek satir okuyup Found veya Missing yapilandirilmis kaniti dondurur.
    public async Task<ProbeEvidence> RunAsync(
        DatabaseConnection connection,
        ProbeRequest request,
        ValueRetentionPolicy retentionPolicy,
        CancellationToken cancellationToken = default)
    {
        var info = await ConnectionInfoFactory.BuildAsync(connection, cancellationToken);
        var repository = RepositoryResolver.Resolve(connection.Engine.Code);
        var rows = await repository.ReadRowsByKeyAsync(
            info,
            request.Structure!,
            request.KeyValues,
            1,
            cancellationToken);
        return new ProbeEvidence
        {
            ProbeKindCode = ProbeKindCode,
            HypothesisKindCode = request.HypothesisKindCode,
            FactCode = rows.Count > 0 ? ProbeKindCodes.Facts.Found : ProbeKindCodes.Facts.Missing,
            ObservedCount = rows.Count
        };
    }
}

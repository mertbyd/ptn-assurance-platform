using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Services.SchemaDiscovery;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Database checker sema servisini Bridge bilgi portuna baglar.
// sistemdeki gorevi: Checker I/O'sunu Application'da tutup sema kararlarini Manager'a birakir.
[RemoteService(IsEnabled = false)]
public class SchemaKnowledgeAppService : TestModuleAppService, ISchemaKnowledgePort
{
    private static readonly SchemaKnowledgeMapper Mapper = new();
    private readonly ISchemaDiscoveryAppService _appService;
    private readonly SchemaKnowledgeManager _manager;

    // Database checker public sema servisini anti-corruption sinirina baglar.
    public SchemaKnowledgeAppService(ISchemaDiscoveryAppService appService, SchemaKnowledgeManager manager)
    {
        _appService = appService;
        _manager = manager;
    }

    // Tek tablo adresini checker'a sorup Bridge tablo tanimina cevirir.
    public async Task<PtnTableDescription> DescribeTableAsync(PtnTableQuery query, CancellationToken cancellationToken)
    {
        var result = await _appService.DescribeTableAsync(
            query.ConnectionId,
            query.DbSchemaName,
            query.TableName,
            cancellationToken);
        return _manager.CompleteDescription(Mapper.Map(result), result.SchemaName);
    }

    // Tum kullanici semalarini kanonik Bridge snapshot'ina cevirir.
    public async Task<PtnSchemaSnapshot> GetSnapshotAsync(Guid connectionId, CancellationToken cancellationToken)
    {
        var result = await _appService.GetSnapshotAsync(connectionId, new List<string>(), cancellationToken);
        return Mapper.Map(result);
    }

    // Siralanmis sema fotografini JSON'a cevirip sha256 fingerprint dondurur.
    public async Task<string> GetSchemaFingerprintAsync(Guid connectionId, CancellationToken cancellationToken)
    {
        return _manager.GetFingerprint(await GetSnapshotAsync(connectionId, cancellationToken));
    }
}

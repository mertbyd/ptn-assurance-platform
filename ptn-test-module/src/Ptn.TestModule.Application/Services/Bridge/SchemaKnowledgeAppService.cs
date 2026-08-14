using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.DatabaseChecker.Services.SchemaDiscovery;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Database checker sema servisini Bridge bilgi use-case'lerine baglar.
// sistemdeki gorevi: Checker I/O'sunu Application'da tutup sema kararlarini Manager'a birakir.
[RemoteService(IsEnabled = false)]
public class SchemaKnowledgeAppService : TestModuleAppService, ISchemaKnowledgeAppService
{
    private static readonly SchemaKnowledgeMapper Mapper = new();
    private readonly ISchemaDiscoveryAppService _appService;
    private readonly SchemaKnowledgeManager _manager;
    private readonly IValidator<TableQueryDto> _tableQueryValidator;

    // Database checker public sema servisini anti-corruption sinirina baglar.
    public SchemaKnowledgeAppService(
        ISchemaDiscoveryAppService appService,
        SchemaKnowledgeManager manager,
        IValidator<TableQueryDto> tableQueryValidator)
    {
        _appService = appService;
        _manager = manager;
        _tableQueryValidator = tableQueryValidator;
    }

    // Public tablo sorgusunu Domain modeline ve tablo tanimini DTO'ya map eder.
    public async Task<TableDescriptionDto> DescribeTableAsync(
        TableQueryDto input,
        CancellationToken cancellationToken)
    {
        await _tableQueryValidator.ValidateAndThrowAsync(input, cancellationToken);
        var query = Mapper.Map(input);
        var result = await _appService.DescribeTableAsync(
            query.ConnectionId,
            query.DbSchemaName,
            query.TableName,
            cancellationToken);
        return Mapper.Map(_manager.CreateDescription(Mapper.Map(result)));
    }

    // Public snapshot istegini Domain sonucundan DTO'ya map eder.
    async Task<SchemaSnapshotDto> ISchemaKnowledgeAppService.GetSnapshotAsync(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        var result = await _appService.GetSnapshotAsync(connectionId, new List<string>(), cancellationToken);
        return Mapper.Map(Mapper.Map(result));
    }

    // Siralanmis sema fotografini JSON'a cevirip sha256 fingerprint dondurur.
    public async Task<string> GetSchemaFingerprintAsync(Guid connectionId, CancellationToken cancellationToken)
    {
        var result = await _appService.GetSnapshotAsync(connectionId, new List<string>(), cancellationToken);
        return _manager.GetFingerprint(Mapper.Map(result));
    }
}

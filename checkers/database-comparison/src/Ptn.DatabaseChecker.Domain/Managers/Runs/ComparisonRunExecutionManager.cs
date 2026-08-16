using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Interface.Definitions;
using Ptn.DatabaseChecker.Interface.Runs;
using Ptn.DatabaseChecker.Managers.Comparison;
using Ptn.DatabaseChecker.Models.Comparison.Findings;
using Ptn.DatabaseChecker.Models.Comparison.Scope;
using Ptn.DatabaseChecker.Models.Runs;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Ptn.DatabaseChecker.Managers.Runs;

// islevi: Pending bir run'i sahiplenir, tarifi ve baglantilari dogrular, motoru calistirir ve terminal sonucu kalici kayda uygular.
// sistemdeki gorevi: Background job icindeki "Pending -> Running -> Completed/Failed" yasam dongusunu ve comparison I/O orkestrasyonunu AppService'ten uzak tutar; client durum belirleyemez.
public class ComparisonRunExecutionManager : DomainService
{
    // Tarif kaynak/hedef baglanti ve mod navigation'lariyla okunur; motor ture ve baglantiya buradan ulasir.
    private IComparisonDefinitionRepository DefinitionRepository
        => LazyServiceProvider.LazyGetRequiredService<IComparisonDefinitionRepository>();

    // Pending run'in snapshot baglantilari ve comparison type'i dahil tam kaydini execution icin okur.
    private IComparisonRunRepository RunRepository
        => LazyServiceProvider.LazyGetRequiredService<IComparisonRunRepository>();

    // Baglantilar Engine navigation'iyla okunur; secret cozumu + motor secimi bunu gerektirir.
    private IDatabaseConnectionRepository ConnectionRepository
        => LazyServiceProvider.LazyGetRequiredService<IDatabaseConnectionRepository>();

    // Durum lookup'i kararli Code'dan Id'ye cozmek icin okunur.
    private IRepository<ComparisonRunStatus, Guid> StatusRepository
        => LazyServiceProvider.LazyGetRequiredService<IRepository<ComparisonRunStatus, Guid>>();

    // Iki snapshot okuma + ture gore diff motoru orkestrasyonu bu domain servisinde isletilir.
    private SchemaComparisonExecutionManager SchemaExecutionManager
        => LazyServiceProvider.LazyGetRequiredService<SchemaComparisonExecutionManager>();

    // islevi: Aktif tariften kuyruga alinacak Pending run'in degismez baglanti/mod snapshot alanlarini hazirlar.
    // sistemdeki gorevi: HTTP istegi motor I/O'suna girmeden tarih kaydini olusturur; aktiflik guard'lari kuyruga hatali is yazilmasini engeller.
    public async Task<ComparisonRunExecutionResultModel> PrepareAsync(Guid comparisonDefinitionId)
    {
        var definition = await LoadDefinitionAsync(comparisonDefinitionId);
        await LoadAccessibleConnectionAsync(definition.SourceConnectionId);
        await LoadAccessibleConnectionAsync(definition.TargetConnectionId);
        var pendingStatusId = await ResolveStatusIdAsync(ComparisonRunStatusCodes.Pending);

        return BuildPendingResult(definition, pendingStatusId);
    }

    // islevi: Kuyruktaki run'in uygulama DB verilerini kisa UOW'de operasyonel execution baglamina donusturur.
    // sistemdeki gorevi: Definition kuyruk sonrasinda baglanti/mod degistirse bile run source/target/type snapshot'ini korur; scope runtime'ini kalici definition alanina yazmadan job isteginden alir.
    public async Task<ComparisonRunExecutionContextModel> BuildExecutionContextAsync(
        Guid comparisonRunId,
        List<ComparisonScopeRule> scopeRules)
    {
        var run = await LoadRunAsync(comparisonRunId);
        var definition = await LoadDefinitionAsync(run.ComparisonDefinitionId!.Value);
        var sourceConnection = await LoadExecutionConnectionAsync(run.SourceConnectionId);
        var targetConnection = await LoadExecutionConnectionAsync(run.TargetConnectionId);
        var completedStatusId = await ResolveStatusIdAsync(ComparisonRunStatusCodes.Completed);

        return new ComparisonRunExecutionContextModel
        {
            ComparisonRunId = run.Id,
            ComparisonDefinitionId = definition.Id,
            SourceConnection = sourceConnection,
            TargetConnection = targetConnection,
            ComparisonTypeId = run.ComparisonTypeId,
            ComparisonTypeCode = run.ComparisonType.Code,
            SourceRoleCode = definition.SourceRoleCode,
            ScopeRules = scopeRules,
            CompletedStatusId = completedStatusId,
            StartedAt = run.StartedAt ?? Clock.Now
        };
    }

    // islevi: UOW'den ayrilmis execution baglamiyla dis veritabanlarini okuyup comparison sonuc modelini uretir.
    // sistemdeki gorevi: Uzun source/target I/O'su uygulama DB transaction'i tutmadan calisir; completion persistence background job'in sonraki kisa UOW'sindedir.
    public async Task<ComparisonRunExecutionResultModel> ExecuteAsync(ComparisonRunExecutionContextModel context)
    {
        var findings = await SchemaExecutionManager.CompareSchemasAsync(
            context.SourceConnection,
            context.TargetConnection,
            new List<string>(),
            context.ScopeRules,
            context.ComparisonTypeCode,
            context.SourceRoleCode);
        var completedAt = Clock.Now;

        return BuildCompletedResult(context, findings, context.StartedAt, completedAt);
    }

    // islevi: Pending run'i navigation ve owned alanlariyla yukler; ad-hoc/eksik tarifli kaydi execution icin reddeder.
    private async Task<ComparisonRun> LoadRunAsync(Guid comparisonRunId)
    {
        var run = await RunRepository.FindWithDetailsAsync(comparisonRunId);
        if (run is null || !run.ComparisonDefinitionId.HasValue)
        {
            throw new BusinessException(GeneralExceptionCodes.NotFound);
        }

        return run;
    }

    // islevi: Tarifi kaynak/hedef baglanti ve mod navigation'lariyla yukler; yoksa not-found firlatir.
    private async Task<ComparisonDefinition> LoadDefinitionAsync(Guid comparisonDefinitionId)
    {
        var definition = await DefinitionRepository.FindWithDetailsAsync(comparisonDefinitionId);
        if (definition is null)
        {
            throw new BusinessException(GeneralExceptionCodes.NotFound);
        }

        if (!definition.IsActive)
        {
            throw new BusinessException(ComparisonDefinitionExceptionCodes.Inactive);
        }

        return definition;
    }

    // islevi: Baglantiyi Engine navigation'iyla (secret cozumu + motor secimi icin) yukler; yoksa not-found firlatir.
    private async Task<DatabaseConnection> LoadAccessibleConnectionAsync(Guid connectionId)
    {
        var connection = await ConnectionRepository.FindWithDetailsAsync(connectionId);
        return EnsureActiveConnection(connection);
    }

    // islevi: Worker baglantisini kullanici claim'i yerine aktif tenant siniriyla yukler.
    private async Task<DatabaseConnection> LoadExecutionConnectionAsync(Guid connectionId)
    {
        var connection = await ConnectionRepository.FindForExecutionAsync(connectionId);
        return EnsureActiveConnection(connection);
    }

    // islevi: Eksik veya pasif baglantiyi HTTP ve worker okuma yollarinda ayni house kodlariyla reddeder.
    private static DatabaseConnection EnsureActiveConnection(DatabaseConnection? connection)
    {
        if (connection is null)
        {
            throw new BusinessException(GeneralExceptionCodes.NotFound);
        }

        if (!connection.IsActive)
        {
            throw new BusinessException(DatabaseConnectionExceptionCodes.Inactive);
        }

        return connection;
    }

    // islevi: Kararli durum kodunu (Completed) lookup Id'sine cozer; seed edilmemisse not-found firlatir.
    private async Task<Guid> ResolveStatusIdAsync(string statusCode)
    {
        var status = await StatusRepository.FirstOrDefaultAsync(x => x.Code == statusCode);
        if (status is null)
        {
            // Durum lookup'i seed edilir; eksikse kullanici hatasi degil sistem/config hatasidir.
            throw new BusinessException(GeneralExceptionCodes.InvalidOperation);
        }

        return status.Id;
    }

    // islevi: Aktif tariften motor calismadan once saklanacak Pending run modelini kurar.
    private static ComparisonRunExecutionResultModel BuildPendingResult(
        ComparisonDefinition definition,
        Guid statusId)
        => new()
        {
            ComparisonDefinitionId = definition.Id,
            SourceConnectionId = definition.SourceConnectionId,
            TargetConnectionId = definition.TargetConnectionId,
            ComparisonTypeId = definition.ComparisonTypeId,
            StatusId = statusId,
            Findings = new ComparisonFindings()
        };

    // islevi: Run snapshot'i + motor bulgularindan kalici Completed sonuc modelini kurar.
    private static ComparisonRunExecutionResultModel BuildCompletedResult(
        ComparisonRunExecutionContextModel context,
        ComparisonFindings findings,
        DateTime startedAt,
        DateTime completedAt)
        => new()
        {
            ComparisonDefinitionId = context.ComparisonDefinitionId,
            SourceConnectionId = context.SourceConnection.Id,
            TargetConnectionId = context.TargetConnection.Id,
            ComparisonTypeId = context.ComparisonTypeId,
            StatusId = context.CompletedStatusId,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            SchemaDifferenceCount = findings.SchemaDifferences.Count,
            DataDifferenceCount = findings.DataDifferences.Count,
            MigrationDifferenceCount = findings.MigrationDifferences.Count,
            Findings = findings
        };
}

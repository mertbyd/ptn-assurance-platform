using System;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Application.BackgroundJobs.Shared;
using Ptn.DatabaseChecker.Application.Mappers.Comparison;
using Ptn.DatabaseChecker.Application.Mappers.Runs;
using Ptn.DatabaseChecker.BackgroundJobs.Runs;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Managers.Runs;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.Application.BackgroundJobs.Runs;

// islevi: Pending comparison run'i HTTP isteginden bagimsiz calistirip kalici durum ve sonucunu tamamlar.
// sistemdeki gorevi: Running/Completed/Failed gecislerini ayri UOW'lerde manager'a uygulatir ve notification altyapisindan bagimsiz kalir.
public class ComparisonRunExecutionBackgroundJob
    : DatabaseCheckerTenantBackgroundJob<ComparisonRunExecutionJobArgs>, ITransientDependency
{
    // Mapperly job payload'indaki scope DTO'larini domain motor modeline tasir.
    private static readonly SchemaComparisonMapper SchemaMapper = new();
    // Mapperly sonuc modelini mevcut run entity'sine uygular; property kopyalama job'a sizmaz.
    private static readonly ComparisonRunMapper Mapper = new();
    // Run status gecislerini ve persistence kararlarini yoneten domain manager.
    private readonly ComparisonRunManager _runManager;
    // Source/target okuma ve comparison engine orkestrasyonunu yoneten domain manager.
    private readonly ComparisonRunExecutionManager _executionManager;
    public ComparisonRunExecutionBackgroundJob(
        ComparisonRunManager runManager,
        ComparisonRunExecutionManager executionManager,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ICancellationTokenProvider cancellationTokenProvider)
        : base(currentTenant, unitOfWorkManager, cancellationTokenProvider)
    {
        _runManager = runManager;
        _executionManager = executionManager;
    }

    // islevi: Run'i claim eder, motoru UOW disinda calistirir ve terminal sonucu ayri UOW'de saklar.
    // sistemdeki gorevi: Uzun suren dis veritabani I/O'su sirasinda uygulama DB transaction'i acik tutmaz; her terminal hata run tarihinde Failed olarak kalir.
    protected override async Task ExecuteInTenantAsync(ComparisonRunExecutionJobArgs args)
    {
        var shouldExecute = await RunInUnitOfWorkAsync(
            () => _runManager.StartExecutionAsync(args.ComparisonRunId));
        if (!shouldExecute)
        {
            return;
        }

        try
        {
            var scopeRules = SchemaMapper.MapToScopeRules(args.ScopeRules);
            var executionContext = await RunInUnitOfWorkAsync(
                () => _executionManager.BuildExecutionContextAsync(args.ComparisonRunId, scopeRules));
            var result = await _executionManager.ExecuteAsync(executionContext);
            await RunInUnitOfWorkAsync(
                () => _runManager.CompleteExecutionAsync(
                    args.ComparisonRunId,
                    result,
                    Mapper.MapExecutionResultToEntity));
        }
        catch (Exception exception)
        {
            // Background-job siniri terminal hatayi kalici Failed tarihe cevirmek zorundadir; aksi halde HTTP disina tasinan hata kaybolur.
            await RunInUnitOfWorkAsync(
                () => _runManager.FailExecutionAsync(args.ComparisonRunId, ResolveErrorCode(exception)));
            return;
        }
    }

    // islevi: Persist edilecek guvenli run hata kodunu exception tipinden secer.
    // sistemdeki gorevi: BusinessException house kodunu korur; beklenmeyen exception message/connection/secret detaylarini API tarihcesine sizdirmaz.
    private static string ResolveErrorCode(Exception exception)
        => exception is BusinessException businessException && !string.IsNullOrWhiteSpace(businessException.Code)
            ? businessException.Code
            : ComparisonRunExceptionCodes.ExecutionFailed;
}

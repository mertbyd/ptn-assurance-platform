using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Services.Assertions;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Database checker assertion servisini Bridge portuna baglar.
// sistemdeki gorevi: Checker I/O'sunu Application katmaninda, DTO-model eslemesini Mapperly'de tutar.
[RemoteService(IsEnabled = false)]
public class DatabaseOracleAppService : TestModuleAppService, IDatabaseOracleAppService, IDatabaseOraclePort
{
    private static readonly DatabaseOracleMapper Mapper = new();
    private readonly IDatabaseAssertionAppService _appService;
    private readonly DatabaseOracleManager _manager;
    private readonly IValidator<DatabaseAssertionRequestDto> _assertionValidator;
    private readonly IValidator<DatabaseAssertionBatchRequestDto> _batchValidator;
    private readonly IValidator<ProjectionRequestDto> _projectionValidator;

    // Database checker public servisini anti-corruption sinirina baglar.
    public DatabaseOracleAppService(
        IDatabaseAssertionAppService appService,
        DatabaseOracleManager manager,
        IValidator<DatabaseAssertionRequestDto> assertionValidator,
        IValidator<DatabaseAssertionBatchRequestDto> batchValidator,
        IValidator<ProjectionRequestDto> projectionValidator)
    {
        _appService = appService;
        _manager = manager;
        _assertionValidator = assertionValidator;
        _batchValidator = batchValidator;
        _projectionValidator = projectionValidator;
    }

    // Public satir assertion girdisini Domain modeline ve sonucunu DTO'ya map eder.
    public async Task<AssertionResultDto> AssertRowAsync(
        DatabaseAssertionRequestDto input,
        CancellationToken cancellationToken)
    {
        await _assertionValidator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(await ((IDatabaseOraclePort)this).AssertRowAsync(Mapper.Map(input), cancellationToken));
    }

    // Public count assertion girdisini Domain modeline ve sonucunu DTO'ya map eder.
    public async Task<AssertionResultDto> AssertCountAsync(
        DatabaseAssertionRequestDto input,
        CancellationToken cancellationToken)
    {
        await _assertionValidator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(await ((IDatabaseOraclePort)this).AssertCountAsync(Mapper.Map(input), cancellationToken));
    }

    // Public absence assertion girdisini Domain modeline ve sonucunu DTO'ya map eder.
    public async Task<AssertionResultDto> AssertAbsentAsync(
        DatabaseAssertionRequestDto input,
        CancellationToken cancellationToken)
    {
        await _assertionValidator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(await ((IDatabaseOraclePort)this).AssertAbsentAsync(Mapper.Map(input), cancellationToken));
    }

    // Public batch DTO'sunu Domain request listesine ve sonuclari DTO listesine map eder.
    public async Task<IReadOnlyList<AssertionResultDto>> AssertBatchAsync(
        DatabaseAssertionBatchRequestDto input,
        CancellationToken cancellationToken)
    {
        await _batchValidator.ValidateAndThrowAsync(input, cancellationToken);
        var results = await ((IDatabaseOraclePort)this).AssertBatchAsync(Mapper.Map(input.Requests), cancellationToken);
        return Mapper.Map(results);
    }

    // Public projeksiyon girdisini Domain modeline ve sonucunu DTO'ya map eder.
    public async Task<ProjectionResultDto> ProjectAsync(
        ProjectionRequestDto input,
        CancellationToken cancellationToken)
    {
        await _projectionValidator.ValidateAndThrowAsync(input, cancellationToken);
        return Mapper.Map(await ((IDatabaseOraclePort)this).ProjectAsync(Mapper.Map(input), cancellationToken));
    }

    // Satir assertion sonucunu normalize Bridge modeline cevirir.
    async Task<PtnAssertionResult> IDatabaseOraclePort.AssertRowAsync(PtnDatabaseAssertionRequest request, CancellationToken cancellationToken)
    {
        return _manager.Normalize(Mapper.Map(
            await _appService.AssertRowAsync(Mapper.Map(request), cancellationToken)));
    }

    // Count assertion sonucunu normalize Bridge modeline cevirir.
    async Task<PtnAssertionResult> IDatabaseOraclePort.AssertCountAsync(PtnDatabaseAssertionRequest request, CancellationToken cancellationToken)
    {
        return _manager.Normalize(Mapper.Map(
            await _appService.AssertCountAsync(Mapper.Map(request), cancellationToken)));
    }

    // Absence assertion sonucunu normalize Bridge modeline cevirir.
    async Task<PtnAssertionResult> IDatabaseOraclePort.AssertAbsentAsync(PtnDatabaseAssertionRequest request, CancellationToken cancellationToken)
    {
        return _manager.Normalize(Mapper.Map(
            await _appService.AssertAbsentAsync(Mapper.Map(request), cancellationToken)));
    }

    // Assertion listesini tek checker cagrisi uzerinden sirali Bridge sonuclarina cevirir.
    async Task<IReadOnlyList<PtnAssertionResult>> IDatabaseOraclePort.AssertBatchAsync(
        IReadOnlyList<PtnDatabaseAssertionRequest> requests,
        CancellationToken cancellationToken)
    {
        var results = await _appService.AssertBatchAsync(Mapper.Map(requests), cancellationToken);
        return _manager.Normalize(Mapper.Map(results));
    }

    // Checker projeksiyon ucu bulunmadigi icin kanit yoklugunu Unavailable olarak bildirir.
    Task<PtnProjectionResult> IDatabaseOraclePort.ProjectAsync(PtnProjectionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_manager.CreateUnavailableProjection());
    }

}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Services.Assertions;
using Ptn.TestModule.Dtos.Bridge.Database;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Mappers.Bridge;
using Volo.Abp;

namespace Ptn.TestModule.Services.Bridge;

// islevi: Database checker assertion servisini Bridge use-case'lerine baglar.
// sistemdeki gorevi: Checker I/O'sunu Application katmaninda, DTO-model eslemesini Mapperly'de tutar.
[RemoteService(IsEnabled = false)]
public class DatabaseOracleAppService : TestModuleAppService, IDatabaseOracleAppService
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
        var result = await _appService.AssertRowAsync(Mapper.Map(Mapper.Map(input)), cancellationToken);
        return Mapper.Map(_manager.Normalize(Mapper.Map(result)));
    }

    // Public count assertion girdisini Domain modeline ve sonucunu DTO'ya map eder.
    public async Task<AssertionResultDto> AssertCountAsync(
        DatabaseAssertionRequestDto input,
        CancellationToken cancellationToken)
    {
        await _assertionValidator.ValidateAndThrowAsync(input, cancellationToken);
        var result = await _appService.AssertCountAsync(Mapper.Map(Mapper.Map(input)), cancellationToken);
        return Mapper.Map(_manager.Normalize(Mapper.Map(result)));
    }

    // Public absence assertion girdisini Domain modeline ve sonucunu DTO'ya map eder.
    public async Task<AssertionResultDto> AssertAbsentAsync(
        DatabaseAssertionRequestDto input,
        CancellationToken cancellationToken)
    {
        await _assertionValidator.ValidateAndThrowAsync(input, cancellationToken);
        var result = await _appService.AssertAbsentAsync(Mapper.Map(Mapper.Map(input)), cancellationToken);
        return Mapper.Map(_manager.Normalize(Mapper.Map(result)));
    }

    // Public batch DTO'sunu Domain request listesine ve sonuclari DTO listesine map eder.
    public async Task<IReadOnlyList<AssertionResultDto>> AssertBatchAsync(
        DatabaseAssertionBatchRequestDto input,
        CancellationToken cancellationToken)
    {
        await _batchValidator.ValidateAndThrowAsync(input, cancellationToken);
        var results = await _appService.AssertBatchAsync(Mapper.Map(Mapper.Map(input.Requests)), cancellationToken);
        return Mapper.Map(_manager.Normalize(Mapper.Map(results)));
    }

    // Public projeksiyon girdisini Domain modeline ve sonucunu DTO'ya map eder.
    public async Task<ProjectionResultDto> ProjectAsync(
        ProjectionRequestDto input,
        CancellationToken cancellationToken)
    {
        await _projectionValidator.ValidateAndThrowAsync(input, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return Mapper.Map(_manager.CreateUnavailableProjection());
    }
}

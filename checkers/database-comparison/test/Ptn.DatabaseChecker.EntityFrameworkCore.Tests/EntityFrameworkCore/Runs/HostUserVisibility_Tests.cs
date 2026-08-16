using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Definitions;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Entities.Runs;
using Ptn.DatabaseChecker.Interface.Definitions;
using Ptn.DatabaseChecker.Interface.Runs;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Runs;

// islevi: Host baglamindaki comparison definition ve run okumalarini olusturan kullaniciya sinirlar.
// sistemdeki gorevi: Ortak SaaS hostunda bir kullanicinin baska kullanicinin tarifini veya sonucunu gormesini engeller.
public class HostUserVisibility_Tests : DatabaseCheckerEntityFrameworkCoreTestBase
{
    private readonly IRepository<DatabaseConnection, Guid> _connectionRepository;
    private readonly IRepository<DatabaseEngine, Guid> _engineRepository;
    private readonly IRepository<ComparisonType, Guid> _comparisonTypeRepository;
    private readonly IRepository<ComparisonRunStatus, Guid> _statusRepository;
    private readonly IComparisonDefinitionRepository _definitionRepository;
    private readonly IComparisonRunRepository _runRepository;
    private readonly ICurrentPrincipalAccessor _principalAccessor;

    public HostUserVisibility_Tests()
    {
        _connectionRepository = GetRequiredService<IRepository<DatabaseConnection, Guid>>();
        _engineRepository = GetRequiredService<IRepository<DatabaseEngine, Guid>>();
        _comparisonTypeRepository = GetRequiredService<IRepository<ComparisonType, Guid>>();
        _statusRepository = GetRequiredService<IRepository<ComparisonRunStatus, Guid>>();
        _definitionRepository = GetRequiredService<IComparisonDefinitionRepository>();
        _runRepository = GetRequiredService<IComparisonRunRepository>();
        _principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    [Fact]
    public async Task Definition_And_Run_Should_Not_Leak_Between_Host_Users()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var fixture = await CreateFixtureAsync(ownerId);

        await WithUnitOfWorkAsync(async () =>
        {
            using (_principalAccessor.Change(BuildPrincipal(ownerId)))
            {
                (await _definitionRepository.FindWithDetailsAsync(fixture.DefinitionId)).ShouldNotBeNull();
                (await _runRepository.FindHeaderAsync(fixture.RunId)).ShouldNotBeNull();
            }

            using (_principalAccessor.Change(BuildPrincipal(otherUserId)))
            {
                (await _definitionRepository.FindWithDetailsAsync(fixture.DefinitionId)).ShouldBeNull();
                (await _runRepository.FindHeaderAsync(fixture.RunId)).ShouldBeNull();
            }
        });
    }

    private async Task<(Guid DefinitionId, Guid RunId)> CreateFixtureAsync(Guid ownerId)
    {
        return await WithUnitOfWorkAsync(async () =>
        {
            using (_principalAccessor.Change(BuildPrincipal(ownerId)))
            {
                var engine = await _engineRepository.FirstAsync();
                var comparisonType = await _comparisonTypeRepository.FirstAsync();
                var status = await _statusRepository.FirstAsync();
                var source = BuildConnection(engine.Id, "visibility-source");
                var target = BuildConnection(engine.Id, "visibility-target");
                await _connectionRepository.InsertAsync(source, autoSave: true);
                await _connectionRepository.InsertAsync(target, autoSave: true);

                var definition = new ComparisonDefinition(Guid.NewGuid())
                {
                    Name = "visibility-definition",
                    SourceConnectionId = source.Id,
                    TargetConnectionId = target.Id,
                    ComparisonTypeId = comparisonType.Id,
                    IsActive = true
                };
                await _definitionRepository.InsertAsync(definition, autoSave: true);

                var run = new ComparisonRun(Guid.NewGuid())
                {
                    ComparisonDefinitionId = definition.Id,
                    SourceConnectionId = source.Id,
                    TargetConnectionId = target.Id,
                    ComparisonTypeId = comparisonType.Id,
                    StatusId = status.Id
                };
                await _runRepository.InsertAsync(run, autoSave: true);
                return (definition.Id, run.Id);
            }
        });
    }

    private static DatabaseConnection BuildConnection(Guid engineId, string name)
    {
        return new DatabaseConnection(Guid.NewGuid())
        {
            EngineId = engineId,
            Name = $"{name}-{Guid.NewGuid():N}",
            Host = "localhost",
            Port = 5432,
            DatabaseName = "visibility",
            VaultSecretPath = $"database-checker/test/{Guid.NewGuid():N}",
            IsActive = true
        };
    }

    private static ClaimsPrincipal BuildPrincipal(Guid userId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(AbpClaimTypes.UserId, userId.ToString()) },
            authenticationType: "HostUserVisibilityTest"));
    }
}

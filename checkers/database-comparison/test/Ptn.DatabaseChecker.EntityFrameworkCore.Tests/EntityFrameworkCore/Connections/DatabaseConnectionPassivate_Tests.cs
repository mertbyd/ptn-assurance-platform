using System;
using System.Linq;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Services.Connections;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Connections;

// islevi: Baglantiyi pasife cekme akisinin basarili sonucu cevap olarak dondurebilmesini pinler.
// sistemdeki gorevi: IPassivable global filtresi pasif satiri her sorgudan disladigi icin "kaydet sonra Id ile geri oku"
// kalibi bu akista calismaz - kayit pasiflesir ama geri okuma null doner ve basarili islem NotFound'a cevrilir
// (canli olarak yasandi: passivate 404 donuyordu). PassivateAsync cevabini bu yuzden elindeki entity'den uretir.
// Test hem sonucu (DTO doner, patlamaz) hem de nedenini (geri okuma gercekten null) sabitler.
public class DatabaseConnectionPassivate_Tests : DatabaseCheckerEntityFrameworkCoreTestBase
{
    private readonly IDatabaseConnectionAppService _connectionAppService;
    private readonly IDatabaseConnectionRepository _connectionRepository;
    private readonly IRepository<DatabaseEngine, Guid> _engineRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;

    public DatabaseConnectionPassivate_Tests()
    {
        _connectionAppService = GetRequiredService<IDatabaseConnectionAppService>();
        _connectionRepository = GetRequiredService<IDatabaseConnectionRepository>();
        _engineRepository = GetRequiredService<IRepository<DatabaseEngine, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Passivate_Should_Return_The_Passivated_Connection()
    {
        var tenantId = Guid.NewGuid();
        var connectionId = await CreateConnectionAsync(tenantId);

        var dto = await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                return await _connectionAppService.PassivateAsync(connectionId);
            }
        });

        dto.Id.ShouldBe(connectionId);
        dto.IsActive.ShouldBeFalse();
        // Cevap geri okumadan uretiliyor; Engine Include'u yine de tasindigini dogrula (EngineCode bos kalmamali).
        dto.EngineCode.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Passivated_Connection_Should_Not_Be_Returned_By_Read_Queries()
    {
        var tenantId = Guid.NewGuid();
        var connectionId = await CreateConnectionAsync(tenantId);

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                await _connectionAppService.PassivateAsync(connectionId);
            }
        });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                // Pasiflestikten sonra kayit kendi okuma sorgusuyla gorulemez.
                // PassivateAsync'in cevabini geri okumaya baglamak bu yuzden basarili islemi 404'e cevirir.
                (await _connectionRepository.FindWithDetailsAsync(connectionId)).ShouldBeNull();
            }
        });
    }

    private async Task<Guid> CreateConnectionAsync(Guid tenantId)
    {
        var engineId = await WithUnitOfWorkAsync(async () =>
            (await _engineRepository.GetListAsync()).First().Id);
        var connectionId = _guidGenerator.Create();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantId))
            {
                await _connectionRepository.InsertAsync(
                    new DatabaseConnection(connectionId)
                    {
                        EngineId = engineId,
                        Name = "passivate-target",
                        Host = "localhost",
                        Port = 5432,
                        DatabaseName = "probe_db",
                        VaultSecretPath = "test/probe",
                        IsActive = true
                    },
                    autoSave: true);
            }
        });

        return connectionId;
    }
}

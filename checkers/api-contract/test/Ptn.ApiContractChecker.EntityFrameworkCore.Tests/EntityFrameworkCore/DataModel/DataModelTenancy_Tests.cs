using Ptn.ApiContractChecker.Entities.Sources;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.DataModel;

// islevi: Yeni IMultiTenant veri modelinin ABP tenant filtresiyle izolasyonunu dogrular.
// sistemdeki gorevi: Bir tenant'in SpecSource satirinin baska tenant veya host sorgusuna sizmasini engelleyen negatif yol testidir.
[Collection(EfCoreIntegrationCollection.Name)]
public class DataModelTenancy_Tests : ApiContractCheckerEntityFrameworkCoreTestBase
{
    private readonly IRepository<SpecSource, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;

    public DataModelTenancy_Tests()
    {
        _repository = GetRequiredService<IRepository<SpecSource, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    // Tenant A kaynaginin Tenant B ve host baglamindan gorunmedigini dogrular.
    [Fact]
    public async Task SpecSource_Should_Be_Isolated_By_Tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantA))
            {
                await _repository.InsertAsync(
                    new SpecSource(Guid.NewGuid(), "orders", "https://orders.test", null, tenantA),
                    autoSave: true);
            }
        });

        await WithUnitOfWorkAsync(async () =>
        {
            using (_currentTenant.Change(tenantA))
            {
                (await _repository.GetCountAsync()).ShouldBe(1);
            }

            using (_currentTenant.Change(tenantB))
            {
                (await _repository.GetCountAsync()).ShouldBe(0);
            }

            (await _repository.GetCountAsync()).ShouldBe(0);
        });
    }
}

using System;
using Nexum.Abp.Foundation.EntityFrameworkCore.Repositories;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Interface.Lookups;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.TestModule.EntityFrameworkCore.Repository.Lookups;

// islevi: Test hukmu lookup'inin EF Core depo uygulamasidir.
// sistemdeki gorevi: Sorgu, sayfalama ve kalicilik davranisi Foundation lookup deposundan gelir; bu tip yalniz DbContext'i baglar.
public class EfCoreTestOutcomeStatusRepository
    : EfCoreLookupRepository<TestModuleDbContext, TestOutcomeStatus, Guid>, ITestOutcomeStatusRepository
{
    public EfCoreTestOutcomeStatusRepository(IDbContextProvider<TestModuleDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}

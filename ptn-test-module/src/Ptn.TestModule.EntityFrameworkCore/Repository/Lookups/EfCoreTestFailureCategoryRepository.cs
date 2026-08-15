using System;
using Nexum.Abp.Foundation.EntityFrameworkCore.Repositories;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Interface.Lookups;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.TestModule.EntityFrameworkCore.Repository.Lookups;

// islevi: Bulgu kategorisi lookup'inin EF Core depo uygulamasidir.
// sistemdeki gorevi: Sorgu, sayfalama ve kalicilik davranisi Foundation lookup deposundan gelir; bu tip yalniz DbContext'i baglar.
public class EfCoreTestFailureCategoryRepository
    : EfCoreLookupRepository<TestModuleDbContext, TestFailureCategory, Guid>, ITestFailureCategoryRepository
{
    public EfCoreTestFailureCategoryRepository(IDbContextProvider<TestModuleDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}

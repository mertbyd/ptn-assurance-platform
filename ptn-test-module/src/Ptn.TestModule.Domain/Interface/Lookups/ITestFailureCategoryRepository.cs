using System;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Entities.Lookups;

namespace Ptn.TestModule.Interface.Lookups;

// islevi: Bulgu kategorisi lookup'inin depo sozlesmesidir.
// sistemdeki gorevi: Foundation lookup depo yuzeyini tipe baglar; ek uye tasimaz, sorgu ihtiyaci dogdugunda burada acilir.
public interface ITestFailureCategoryRepository : ILookupRepository<TestFailureCategory, Guid>
{
}

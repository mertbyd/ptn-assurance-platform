using System;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Entities.Lookups;

namespace Ptn.TestModule.Interface.Lookups;

// islevi: Senaryo yayin durumu lookup'inin depo sozlesmesidir.
// sistemdeki gorevi: Foundation lookup depo yuzeyini tipe baglar; ek uye tasimaz, sorgu ihtiyaci dogdugunda burada acilir.
public interface ITestScenarioStateRepository : ILookupRepository<TestScenarioState, Guid>
{
}

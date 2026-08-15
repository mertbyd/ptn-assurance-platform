using System;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Entities.Lookups;

namespace Ptn.TestModule.Interface.Lookups;

// islevi: Tetikleme turu lookup'inin depo sozlesmesidir.
// sistemdeki gorevi: Foundation lookup depo yuzeyini tipe baglar; ek uye tasimaz, sorgu ihtiyaci dogdugunda burada acilir.
public interface ITestTriggerKindRepository : ILookupRepository<TestTriggerKind, Guid>
{
}

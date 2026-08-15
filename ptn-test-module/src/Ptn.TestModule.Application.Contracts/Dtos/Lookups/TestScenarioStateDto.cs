using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Dtos.Lookups;

// islevi: Senaryo yayin durumu lookup satirinin public okunabilir gorunumunu tasir.
// sistemdeki gorevi: Ajanin yalniz Published surumun kosuldugunu HTTP'den dogrulamasini saglar.
/// <summary>Bir senaryo yayin durumu lookup satirinin public gorunumudur.</summary>
public sealed class TestScenarioStateDto : LookupDto<Guid>
{
}

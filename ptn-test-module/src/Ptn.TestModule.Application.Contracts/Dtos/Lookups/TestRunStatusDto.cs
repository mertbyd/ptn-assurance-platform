using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Dtos.Lookups;

// islevi: Kosum durumu lookup satirinin public okunabilir gorunumunu tasir.
// sistemdeki gorevi: Ajanin kosum durumu kodlarini Domain.Shared kaynagini okumadan HTTP'den kesfetmesini saglar.
/// <summary>Bir kosum durumu lookup satirinin public gorunumudur.</summary>
public sealed class TestRunStatusDto : LookupDto<Guid>
{
}

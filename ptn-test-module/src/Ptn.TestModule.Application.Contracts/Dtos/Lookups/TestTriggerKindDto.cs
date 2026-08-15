using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Dtos.Lookups;

// islevi: Tetikleme turu lookup satirinin public okunabilir gorunumunu tasir.
// sistemdeki gorevi: Ajanin tetikleme turu kodlarini Domain.Shared kaynagini okumadan HTTP'den kesfetmesini saglar.
/// <summary>Bir tetikleme turu lookup satirinin public gorunumudur.</summary>
public sealed class TestTriggerKindDto : LookupDto<Guid>
{
}

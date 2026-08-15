using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Dtos.Lookups;

// islevi: Bulgu kategorisi lookup satirinin public okunabilir gorunumunu tasir.
// sistemdeki gorevi: Ajanin bulgu kaynagi kodlarini Domain.Shared kaynagini okumadan HTTP'den kesfetmesini saglar.
/// <summary>Bir bulgu kategorisi lookup satirinin public gorunumudur.</summary>
public sealed class TestFailureCategoryDto : LookupDto<Guid>
{
}

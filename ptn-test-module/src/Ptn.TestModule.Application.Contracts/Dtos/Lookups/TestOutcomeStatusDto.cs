using System;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.Dtos.Lookups;

// islevi: Test hukmu lookup satirini build politikasiyla birlikte public gorunume cevirir.
// sistemdeki gorevi: Hangi hukmun build'i kirdigini kod dallanmasi olmadan HTTP tuketicisine bildirir.
/// <summary>Bir test hukmu lookup satirinin public gorunumudur.</summary>
public sealed class TestOutcomeStatusDto : LookupDto<Guid>
{
    /// <summary>Bu hukmun build'i kirip kirmadigini bildirir.</summary>
    public bool BreaksBuild { get; set; }
}

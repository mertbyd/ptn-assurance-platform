using System;

namespace Ptn.TestModule.Models.Runs;

// islevi: HAR ve kosum retention esiklerini tek bir parcali purge teslimine tasir.
// sistemdeki gorevi: Background job'i setting parse ve zaman hesaplama kararlarindan arindirir.
/// <summary>Tek retention purge tesliminin kararli esiklerini tasir.</summary>
public sealed class RunRetentionPlan
{
    /// <summary>Bu zamandan once tamamlanan HAR artefaktlari silinir.</summary>
    public DateTime HarCompletedBefore { get; init; }

    /// <summary>Bu zamandan once tamamlanan kosum satirlari silinir.</summary>
    public DateTime RunCompletedBefore { get; init; }

    /// <summary>Her sorguda islenecek azami satir sayisidir.</summary>
    public int BatchSize { get; init; }
}

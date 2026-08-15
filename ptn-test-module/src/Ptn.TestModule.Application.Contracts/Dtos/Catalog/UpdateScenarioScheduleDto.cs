namespace Ptn.TestModule.Dtos.Catalog;

// islevi: Yayinlanmis senaryo surumune yazilacak cron zamanlamasini tasir.
// sistemdeki gorevi: "Her gece 02:00" gibi takvim ifadelerini tek public alanda kabul eder (PLAN-0003 TM-29).
/// <summary>Senaryo zamanlama guncelleme girdisidir.</summary>
public class UpdateScenarioScheduleDto
{
    /// <summary>UTC olarak yorumlanan bes veya alti alanli cron ifadesidir.</summary>
    public string? ScheduleCron { get; set; }

    /// <summary>Zamanlamanin acik olup olmadigidir; kapaliyken cron alani yok sayilir.</summary>
    public bool ScheduleEnabled { get; set; }
}

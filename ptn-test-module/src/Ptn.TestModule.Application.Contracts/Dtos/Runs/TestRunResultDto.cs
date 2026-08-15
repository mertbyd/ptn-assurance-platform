using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Kalici terminal sonucunu bulgulariyla birlikte public gorunume cevirir.
// sistemdeki gorevi: Result aggregate ve cocuklarini tek okuma sozlesmesinde toplar.
/// <summary>Bir test kosumu denemesinin public terminal sonucudur.</summary>
public sealed class TestRunResultDto : CreationAuditedEntityDto<Guid>
{
    /// <summary>Sonucun ait oldugu TestRun aggregate kimligidir.</summary>
    public Guid TestRunId { get; set; }

    /// <summary>Ayni kosum icindeki bir tabanli deneme numarasidir.</summary>
    public int Attempt { get; set; }

    /// <summary>Terminal hukum lookup kimligidir.</summary>
    public Guid OutcomeStatusId { get; set; }

    /// <summary>Kosumun milisaniye cinsinden suresidir.</summary>
    public int DurationMs { get; set; }

    /// <summary>Opsiyonel hata kategorisi lookup kimligidir.</summary>
    public Guid? FailureCategoryId { get; set; }

    /// <summary>Kararli ve makine-okur hata kodudur.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Bu olusuma ozel RFC 9457 detail metnidir.</summary>
    public string? Detail { get; set; }

    /// <summary>Basarisiz adimin bir tabanli sira numarasidir.</summary>
    public int? FailedStepOrdinal { get; set; }

    /// <summary>Basarisiz adimin gorunen adidir.</summary>
    public string? FailedStepName { get; set; }

    /// <summary>Basarisiz adimin makine-okur yoludur.</summary>
    public string? FailedStepPath { get; set; }

    /// <summary>Kosumun terminale giderken izledigi dal yoludur.</summary>
    public string? TakenBranchPath { get; set; }

    /// <summary>Basariyla tamamlanan son adimin bir tabanli sira numarasidir.</summary>
    public int? LastCompletedOrdinal { get; set; }

    /// <summary>Satir icinde saklanan yapilandirilmis diagnosis JSON metnidir.</summary>
    public string? DiagnosisReport { get; set; }

    /// <summary>Terminal sonucuyla atomik yazilan sirali bulgulardir.</summary>
    public List<TestResultFindingDto> Findings { get; set; } = [];

    /// <summary>Kaydi sahiplenen tenant kimligidir.</summary>
    public Guid? TenantId { get; set; }
}

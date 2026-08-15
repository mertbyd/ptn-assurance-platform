using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bir kosumun runner'a verilecek belge, girdi, severity ve butce sozlesmesini tasir.
// sistemdeki gorevi: Planner'in dogruladigi semantik kosum istegini surec sinirina tek modelle gecirir.
/// <summary>
/// Dis Arazzo runner'ina gonderilecek dogrulanmis kosum istegini tasir.
/// </summary>
public class WorkflowRunRequest
{
    /// <summary>Kosulacak Arazzo 1.0.1 belgesinin tam metnidir.</summary>
    public string Document { get; set; } = string.Empty;

    /// <summary>Ortam degiskeniyle gecirilecek runtime girdi sozlugudur.</summary>
    public IReadOnlyDictionary<string, string> Inputs { get; set; } =
        new Dictionary<string, string>();

    /// <summary>Dort Respect kontrolunun her kosumda acikca set edilen severity haritasidir.</summary>
    public IReadOnlyDictionary<string, string> SeverityMap { get; set; } =
        new Dictionary<string, string>();

    /// <summary>Tum senaryonun runner tarafindaki azami suresidir.</summary>
    public int ExecutionTimeoutSeconds { get; set; }

    /// <summary>Tek bir HTTP cagrisinin runner tarafindaki azami suresidir.</summary>
    public int MaxFetchTimeoutSeconds { get; set; }

    /// <summary>Kosumu operasyonel izle baglayan W3C trace kimligidir.</summary>
    public string TraceId { get; set; } = string.Empty;
}

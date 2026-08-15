using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: UoW disinda icra edilecek kosumun belge, girdi, ortam ve malzeme kayma baglamini tasir.
// sistemdeki gorevi: Hazirlik UoW'unda toplanan her seyi tek modelle icra ve yargi asamalarina tasir (ADR-0015 §B).
/// <summary>
/// Bir kosumun UoW disi icra baglamini tasir.
/// </summary>
public class TestRunExecutionContext
{
    /// <summary>Icra edilecek kosumun kimligidir.</summary>
    public Guid TestRunId { get; set; }

    /// <summary>Senaryonun kosulacak derlenmis Arazzo belgesidir.</summary>
    public string CompiledDocument { get; set; } = string.Empty;

    /// <summary>Belgeden okunan surum, kriter ve adim kimligi olgularidir.</summary>
    public WorkflowDocumentFacts DocumentFacts { get; set; } = new();

    /// <summary>Runner'a ortam degiskeniyle gecirilecek girdi sozlugudur.</summary>
    public IReadOnlyDictionary<string, string> Inputs { get; set; } =
        new Dictionary<string, string>();

    /// <summary>Kosum aninda cozulmus mantiksal ortam baglamasidir.</summary>
    public TestRunEnvironmentBinding EnvironmentBinding { get; set; } = new();

    /// <summary>Senaryo malzeme muhurlerinin kosum ani karsilastirma sonucudur.</summary>
    public TestRunMaterialDrift MaterialDrift { get; set; } = new();

    /// <summary>Kosumu operasyonel izle baglayan W3C trace kimligidir.</summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>Artefaktin yazilacagi tenant kimligidir.</summary>
    public Guid? TenantId { get; set; }
}

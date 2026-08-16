using System;

namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Tek operasyon adayinin adres, puan ve alan baglamalarini tasir.
// sistemdeki gorevi: Istemciye sirali ve tipli esleme adayi sunar.
public sealed class OperationSuggestionDto
{
    /// <summary>
    /// Checker envanteri satirinin kapali ve kararli referans kimligini belirtir.
    /// </summary>
    public Guid ReferenceId { get; set; }
    /// <summary>
    /// Ilgili kaynagin kararli kimligini veya referansini belirtir.
    /// </summary>
    public string? SourceOperationId { get; set; }
    /// <summary>
    /// HTTP operasyonunun yontemini belirtir.
    /// </summary>
    public string SourceMethod { get; set; } = string.Empty;
    /// <summary>
    /// Ilgili verinin kararli yol veya pointer adresini belirtir.
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;
    /// <summary>
    /// Karar veya eslesme icin kullanilan sayisal olcuyu belirtir.
    /// </summary>
    public int Score { get; set; }
    /// <summary>
    /// Sonuca ait aciklayici oge veya adaylari kararli sirada listeler.
    /// </summary>
    public List<FieldBindingDto> Bindings { get; set; } = [];
}

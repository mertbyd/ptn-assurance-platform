namespace Ptn.TestModule.Dtos.Bridge.Api;

// islevi: Tek operasyon adayinin adres, puan ve alan baglamalarini tasir.
// sistemdeki gorevi: Istemciye sirali ve tipli esleme adayi sunar.
public sealed class OperationSuggestionDto
{
    public string? SourceOperationId { get; set; }
    public string SourceMethod { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<FieldBindingDto> Bindings { get; set; } = [];
}

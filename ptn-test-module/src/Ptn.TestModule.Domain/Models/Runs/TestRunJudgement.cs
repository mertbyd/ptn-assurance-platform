namespace Ptn.TestModule.Models.Runs;

// islevi: Icra ve yargi asamalarindan cikan terminal hukmu ile saklanan artefakt adini birlikte tasir.
// sistemdeki gorevi: Job'in ayri yeni UoW'da yapacagi terminal yazim icin gereken her seyi tek modelde toplar.
/// <summary>
/// Bir kosumun yargi sonucunu ve artefakt referansini tasir.
/// </summary>
public class TestRunJudgement
{
    /// <summary>Kalicilastirilacak terminal hukum ve bulgu modelidir.</summary>
    public TestRunTerminalModel Terminal { get; set; } = new();

    /// <summary>Artefakt saklandiysa BLOB deposundaki adidir.</summary>
    public string? HarBlobName { get; set; }
}

namespace Ptn.TestModule.Models.Runs;

// islevi: Sandbox reset stratejisi ile ayri connection-string adini birlikte tasir.
// sistemdeki gorevi: Application I/O servisinin ayar yorumlamadan dogrulanmis reset planini uygulamasini saglar.
/// <summary>
/// Bir test ortami icin dogrulanmis sandbox reset planidir.
/// </summary>
public class SandboxResetPlan
{
    /// <summary>Uygulanacak desteklenen reset stratejisi kodudur.</summary>
    public string StrategyCode { get; set; } = string.Empty;

    /// <summary>Checker hedefinden ayri yazma yetkili connection-string adidir.</summary>
    public string ConnectionStringName { get; set; } = string.Empty;
}

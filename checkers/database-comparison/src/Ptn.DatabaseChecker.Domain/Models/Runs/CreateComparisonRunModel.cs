using System;

namespace Ptn.DatabaseChecker.Models.Runs;

// islevi: Yeni calistirma (infaz kaydi) icin manager'in ihtiyac duydugu alanlari tasir.
// sistemdeki gorevi: ComparisonRunManager baglanti/mod/durum/tarif FK varliklarini bu model uzerinden dogrular; snapshot alanlari run'in kendi ustundedir.
public class CreateComparisonRunModel
{
    // Hangi tariften dogdu; null = ad-hoc calistirma.
    public Guid? ComparisonDefinitionId { get; set; }

    // O an kullanilan referans baglanti (snapshot).
    public Guid SourceConnectionId { get; set; }

    // O an kullanilan denetlenen baglanti (snapshot).
    public Guid TargetConnectionId { get; set; }

    // O an kullanilan karsilastirma modu lookup kimligi (Guid lookup Id, snapshot).
    public Guid ComparisonTypeId { get; set; }

    // Yasam dongusu durumu lookup kimligi (Guid lookup Id); tetiklemede Pending yazilir.
    public Guid StatusId { get; set; }
}

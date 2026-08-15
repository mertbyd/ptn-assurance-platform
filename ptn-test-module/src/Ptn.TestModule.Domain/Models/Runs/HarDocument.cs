using System.Collections.Generic;

namespace Ptn.TestModule.Models.Runs;

// islevi: Yorumlanmis HAR belgesinin tum entry'lerini ve ureten arac meta verisini tasir.
// sistemdeki gorevi: Yargi asamasinin ham JSON yerine tipli tek girdi uzerinden calismasini saglar.
/// <summary>
/// Runner'in urettigi HAR belgesinin yorumlanmis halini tasir.
/// </summary>
public class HarDocument
{
    /// <summary>HAR icindeki tum istek/yanit ciftleridir.</summary>
    public IReadOnlyList<HarEntryModel> Entries { get; set; } = [];

    /// <summary>HAR'i ureten aracin adidir.</summary>
    public string CreatorName { get; set; } = string.Empty;

    /// <summary>HAR'i ureten aracin surumudur.</summary>
    public string CreatorVersion { get; set; } = string.Empty;

    /// <summary>Adim kimligi cozulemeyen entry bulunup bulunmadigidir.</summary>
    public bool HasUnboundEntries { get; set; }
}

using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Agent;

// islevi: Profil bilgisi sorgusunun profil anahtarini ve kapali kavram kodlarini tasir.
// sistemdeki gorevi: Bilgi yuzeyini serbest soru metni yerine sozluk-kisitli kaynak secimine indirger.
public sealed class KnowledgeRequest
{
    public string ProfileKey { get; set; } = string.Empty;
    public Guid ConnectionId { get; set; }
    public List<string> ConceptCodes { get; set; } = [];
    public string ResponseFormat { get; set; } = string.Empty;
}

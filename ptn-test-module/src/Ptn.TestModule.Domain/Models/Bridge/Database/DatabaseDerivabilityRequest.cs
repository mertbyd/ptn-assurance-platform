using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge.Database;

// islevi: Bir baglantiya ait DB assertion adreslerini domain turetilebilirlik isteginde toplar.
// sistemdeki gorevi: Application orkestrasyonunun checker DTO'suna dogrudan baglanmasini engeller.
public sealed class DatabaseDerivabilityRequest
{
    public Guid ConnectionId { get; set; }
    public List<DatabaseDerivabilityAddress> Assertions { get; set; } = [];
}

using System;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bir kosumun ortam kilidi adini ve edinme bekleme suresini tasir.
// sistemdeki gorevi: Background job'in anahtar veya timeout karari vermeden ABP kilidini edinmesini saglar.
/// <summary>
/// Tenant ve ortam bazli dogrulanmis kosum eszamanlilik planidir.
/// </summary>
public class RunConcurrencyPlan
{
    /// <summary>ABP distributed lock sistemine verilecek kararli ortam anahtaridir.</summary>
    public string LockName { get; set; } = string.Empty;

    /// <summary>Kilit sahibinin bitmesini beklemek icin taninan azami suredir.</summary>
    public TimeSpan WaitTimeout { get; set; }
}

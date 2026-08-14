using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Fingerprint hesabina giren sema, tablo ve kolon kimliklerini kanonik veri olarak tasir.
// sistemdeki gorevi: Profil paketinin baglandigi sema surumunu provider DTO'sundan bagimsiz muhurlar.
public sealed class PtnSchemaSnapshot
{
    public List<PtnSchemaTable> Tables { get; set; } = [];
}

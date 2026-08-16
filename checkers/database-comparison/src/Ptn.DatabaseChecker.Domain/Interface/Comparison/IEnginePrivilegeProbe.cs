using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Interface.Comparison;

// islevi: Acik hedef baglantida motor-ozel salt-okuma yetki sorgusunu calistiran bilesen sozlesmesidir.
// sistemdeki gorevi: Connection tester motor secimini resolver ile yapar; provider SQL'i EF katmaninda kalirken sonuc domain modeline iner.
public interface IEnginePrivilegeProbe : IEngineComponent
{
    // islevi: Mevcut acik baglantinin yazma ve yonetici rollerini yan etki olmadan raporlar.
    Task<EnginePrivilegeProbeResult> ProbeAsync(
        DbConnection connection,
        CancellationToken cancellationToken = default);
}

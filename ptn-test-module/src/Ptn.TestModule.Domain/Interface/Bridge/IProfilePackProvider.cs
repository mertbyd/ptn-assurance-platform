using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Interface.Bridge;

// islevi: Surumlu kopru profil paketini kalici ortamdan yukleyen portu tanimlar.
// sistemdeki gorevi: Domain manager'ini dosya sistemi, YAML ve ayar altyapisindan ayirir.
public interface IProfilePackProvider
{
    // Profil anahtarina ait dogrulanmis veri kabugunu yukler.
    Task<PtnProfilePack> LoadAsync(string profileKey, CancellationToken cancellationToken);
}

using System.Threading.Tasks;
using Ptn.ApiContractChecker.Models.Secrets;

namespace Ptn.ApiContractChecker.Interface.Secrets;

// islevi: Secret deposu uzerinde spec kaynagi kimlik bilgisi okuma/yazma/silme portunu tanimlar.
// sistemdeki gorevi: Modulun VaultSharp veya baska bir saglayiciya baglanmadan Composition Host tarafindan uygulanmasini saglar.
public interface ISecretProvider
{
    // Verilen KV v2 path'indeki kimlik bilgisini cozer; yoksa BusinessException(SecretNotFound).
    Task<ApiCredentialModel> GetApiCredentialAsync(string path);

    // Verilen path'e kimlik bilgisini yazar (varsa yeni versiyon olusturur).
    Task SetAsync(string path, ApiCredentialModel credential);

    // Verilen path'teki secret'i soft-delete eder.
    Task DeleteAsync(string path);
}

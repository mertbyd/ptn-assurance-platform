using System.Threading.Tasks;
using Ptn.DatabaseChecker.Models.Secrets;

namespace Ptn.DatabaseChecker.Interface.Secrets;

// islevi: Secret deposu uzerinde veritabani kimlik bilgisi okuma/yazma/silme portunu tanimlar.
// sistemdeki gorevi: Modulun VaultSharp veya baska bir saglayiciya baglanmadan Composition Host tarafindan uygulanmasini saglar.
public interface ISecretProvider
{
    // Verilen KV v2 path'indeki kimlik bilgisini cozer; yoksa BusinessException(SecretNotFound).
    Task<DatabaseCredentialModel> GetDatabaseCredentialAsync(string path);

    // Verilen path'e kimlik bilgisini yazar (varsa yeni versiyon olusturur).
    Task SetAsync(string path, DatabaseCredentialModel credential);

    // Verilen path'teki secret'i soft-delete eder.
    Task DeleteAsync(string path);
}

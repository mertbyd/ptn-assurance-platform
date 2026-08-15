using System.Data.Common;

namespace Ptn.TestModule.Interface.Runs;

// islevi: Yazma yetkili sandbox connection-string'ini iliskisel provider baglantisina cevirir.
// sistemdeki gorevi: Application sandbox adapter'ini Npgsql paketinden ayirirken checker hedef baglantisini disarida tutar.
/// <summary>Sandbox reset icin provider-owned veritabani baglantisi olusturur.</summary>
public interface ITestDataSandboxConnectionFactory
{
    /// <summary>Verilen ayri sandbox connection-string'i icin kapali bir provider baglantisi olusturur.</summary>
    DbConnection Create(string connectionString);
}

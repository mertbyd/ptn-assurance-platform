namespace Ptn.ApiContractChecker.Constants.Snapshots;

// islevi: Degismez spec iceriginin hash ve medya tipi sinirlarini tanimlar.
// sistemdeki gorevi: Hash dogrulamasi ile veritabani kolon uzunluklarini ayni kaynaga baglar.
public static class SpecContentConsts
{
    public const int HashLength = 64;
    public static readonly string HashColumnType = $"character({HashLength})";
    public const string ContentColumnType = "text";

    // Ham spec govdesinin kolon adi; hafif okuma yollarinin secmemesi gereken tek kolondur.
    public const string ContentColumnName = "content";
    public const int MaxMediaTypeLength = 64;
}

namespace Ptn.ApiContractChecker.Constants.Snapshots;

// islevi: SpecSnapshot uzerindeki opsiyonel API surumu ve gecmis listesindeki kisa hash sinirlarini tanimlar.
// sistemdeki gorevi: Snapshot kurulumu, EF kolon eslemesi ve okuma yuzeyinin ayni uzunluk sozlesmesini kullanmasini saglar.
public static class SpecSnapshotConsts
{
    public const int MaxApiVersionLength = 64;

    // Gecmis listesinde iki anlik goruntuyu ayirt etmeye yeten kanonik hash on eki.
    public const int ShortHashLength = 12;
}

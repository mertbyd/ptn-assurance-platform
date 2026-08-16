namespace Ptn.DatabaseChecker.Models.Comparison;

// islevi: Bir veri karsilastirmasinda bulgu degerlerinin hangi mod ve HMAC salt'i ile saklanacagini tasir.
// sistemdeki gorevi: Ayar cozumunu saf TableDataComparisonManager'dan ayirir; salt DTO, log veya exception metnine cikmaz.
public sealed class ValueRetentionPolicy
{
    public string ModeCode { get; }
    public string Salt { get; }

    // islevi: Cozulmus saklama modu ve HMAC salt'ini tek calisma-zamani politikasinda kurar.
    public ValueRetentionPolicy(string modeCode, string salt)
    {
        ModeCode = modeCode;
        Salt = salt;
    }
}

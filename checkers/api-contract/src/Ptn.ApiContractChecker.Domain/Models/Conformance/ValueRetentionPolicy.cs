namespace Ptn.ApiContractChecker.Models.Conformance;

// islevi: Bulgu degerlerinin hangi mod ve HMAC salt'i ile tutulacagini tasir.
// sistemdeki gorevi: Tenant-aware setting cozumunu saf redaction adimindan ayiran degismez calisma zamani deger nesnesidir.
public sealed class ValueRetentionPolicy
{
    public string ModeCode { get; }
    public string Salt { get; }

    // islevi: Cozulmus retention kodu ile gizli hash salt'ini tek politikada kurar.
    public ValueRetentionPolicy(string modeCode, string salt)
    {
        ModeCode = modeCode;
        Salt = salt;
    }
}

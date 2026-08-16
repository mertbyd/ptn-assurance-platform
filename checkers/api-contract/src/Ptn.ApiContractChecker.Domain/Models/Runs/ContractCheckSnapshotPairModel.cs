namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Bir run'in iki degismez snapshot icerigini tek repository sonucunda tasir.
// sistemdeki gorevi: BuildContext UOW'si kapanmadan base/target fotografini tam alir ve Execute adiminda repository ihtiyacini bitirir.
public class ContractCheckSnapshotPairModel
{
    // Run'in referans snapshot'indaki degismez ham spec metni.
    public string BaseContent { get; set; } = default!;

    // Run'in aday snapshot'indaki degismez ham spec metni.
    public string TargetContent { get; set; } = default!;
}

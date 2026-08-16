namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir OpenAPI security requirement icindeki birlikte gereken semalari tasir.
// sistemdeki gorevi: Guvenlik alternatifleri ile iclerindeki AND semalarinin siradan etkilenmeden karsilastirilmasini saglar.
public class SpecSecurityRequirementModel
{
    // Ayni requirement icinde birlikte saglanmasi gereken guvenlik semalari.
    public List<SpecSecuritySchemeModel> Schemes { get; set; } = new();
}

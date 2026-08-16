namespace Ptn.TestModule.Models.Runs;

// islevi: Korumali hedefe gonderilecek tek kimlik basligini tasir.
// sistemdeki gorevi: Yalniz bellekte yasar; DTO'ya, log'a, test_runs satirina, RunnerRef'e ve HAR'a girmez.
/// <summary>
/// Kosum aninda cozulmus API kimlik basligini tasir.
/// </summary>
public class RunCredential
{
    /// <summary>Istege eklenecek kimlik basliginin adidir.</summary>
    public string HeaderName { get; set; } = string.Empty;

    /// <summary>Kimlik basliginin tam degeridir; hicbir kalici yuzeye yazilmaz.</summary>
    public string HeaderValue { get; set; } = string.Empty;
}

namespace Ptn.ApiContractChecker.Interface.Formats;

// islevi: Bir spec formatina (Swagger 2.0 / OAS 3.0 / 3.1 / 3.2) ozel bilesenlerin ortak kimligi: hangi formati konustuklarini kararli kodla bildirirler.
// sistemdeki gorevi: Spec okuyucu gibi format-ozel bilesenler bunu implemente eder; SpecFormatComponentResolver dogru olani bu kodla secer (acik/kapali).
public interface ISpecFormatComponent
{
    // Bilesenin konustugu formatin kararli kodu (SpecFormatCodes.*); lookup satirinin Code'u ile eslesir.
    string FormatCode { get; }
}

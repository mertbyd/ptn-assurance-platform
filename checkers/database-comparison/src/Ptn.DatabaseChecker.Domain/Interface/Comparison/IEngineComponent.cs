namespace Ptn.DatabaseChecker.Interface.Comparison;

// islevi: Bir motora (PostgreSql/SqlServer) ozel bilesenlerin ortak kimligi: hangi motoru konustuklarini kararli kodla bildirirler.
// sistemdeki gorevi: Sema okuyucu, baglanti test edici gibi motor-ozel bilesenler bunu implemente eder; EngineComponentResolver dogru olani bu kodla secer (acik/kapali).
public interface IEngineComponent
{
    // Bilesenin konustugu motorun kararli kodu (DatabaseEngineCodes.*); lookup satirinin Code'u ile eslesir.
    string EngineCode { get; }
}

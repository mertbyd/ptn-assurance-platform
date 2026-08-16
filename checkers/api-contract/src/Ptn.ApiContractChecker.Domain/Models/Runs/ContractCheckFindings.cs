namespace Ptn.ApiContractChecker.Models.Runs;

// islevi: Bir contract check run'inin tum bulgularini tek owned JSON govdesinde toplar.
// sistemdeki gorevi: Agir rapor govdesini run basligindan ayirir ve terminal sayaclarin tek kaynagini olusturur.
public class ContractCheckFindings
{
    public List<Finding> Items { get; private set; } = [];

    // EF Core JSON materializasyonu icin parametresiz ctor.
    protected ContractCheckFindings()
    {
    }

    // Bulgulari run'a ait bagimsiz bir liste kopyasi olarak kurar.
    public ContractCheckFindings(IEnumerable<Finding> items)
    {
        Items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
    }

    // Bulgusuz terminal sonuclar icin bos ve gecerli owned govde olusturur.
    public static ContractCheckFindings Empty()
    {
        return new ContractCheckFindings([]);
    }
}

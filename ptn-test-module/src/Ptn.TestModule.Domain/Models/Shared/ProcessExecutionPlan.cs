using System.Collections.Generic;

namespace Ptn.TestModule.Models.Shared;

// islevi: Bir dis surecin executable, argument, ortam, girdi dosyasi, cikti dosyasi ve butce kararlarinin tamamini tasir.
// sistemdeki gorevi: Manager'in verdigi cagri kararini surec sinirina veri olarak gecirir; sinirda yorumlanacak hicbir sey birakmaz.
/// <summary>
/// Dis surec cagrisinin tam ve kararli planini tasir.
/// </summary>
public class ProcessExecutionPlan
{
    /// <summary>Sureci baslatan executable adidir.</summary>
    public string Executable { get; set; } = string.Empty;

    /// <summary>Shell birlestirmesi yapilmadan gecirilecek argument listesidir.</summary>
    public IReadOnlyList<string> Arguments { get; set; } = [];

    /// <summary>Secret tasiyan degerlerin process listesine dusmeden gectigi ortam degiskenleridir.</summary>
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; set; } =
        new Dictionary<string, string>();

    /// <summary>Gecici calisma klasorunun tahmin edilemez kokune eklenen ad bolumudur.</summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>Surec baslamadan once calisma klasorune yazilacak dosyalardir.</summary>
    public IReadOnlyList<ProcessInputFile> InputFiles { get; set; } = [];

    /// <summary>Surec bittikten sonra geri okunacak artefaktlarin calisma klasorune gore yollaridir.</summary>
    public IReadOnlyList<string> OutputFilePaths { get; set; } = [];

    /// <summary>Surecin sert oldurulecegi azami suredir.</summary>
    public int TimeoutMs { get; set; }

    /// <summary>Surec hic baslatilamazsa firlatilacak kararli hata kodudur.</summary>
    public string StartFailureErrorCode { get; set; } = string.Empty;

    /// <summary>Surec butceyi asarsa firlatilacak kararli hata kodudur.</summary>
    public string TimeoutErrorCode { get; set; } = string.Empty;
}

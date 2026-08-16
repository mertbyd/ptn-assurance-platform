using System.Text;
using System.Text.Json;
using Ptn.ApiContractChecker.Models.Runs;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Comparison;

// islevi: Oracle fixture klasorunu bulur, spec ciftlerini okur ve bulgulari kararli metin satirlarina indirger.
// sistemdeki gorevi: oasdiff oracle turunun .NET tarafi ile PowerShell tarafinin ayni kanonik satir bicimini paylasmasini saglar.
public static class SpecComparisonOracleFixture
{
    public const string DirectoryName = "Fixtures";
    public const string BaseDocumentFileName = "base.json";
    public const string TargetDocumentFileName = "target.json";
    public const string ExpectedFindingsFileName = "expected.json";

    private const string ProjectFileName = "Ptn.ApiContractChecker.EntityFrameworkCore.Tests.csproj";

    // Fixture klasorunu test ciktisinin yaninda cozer.
    public static string RootPath =>
        Path.Combine(AppContext.BaseDirectory, "EntityFrameworkCore", "Comparison", DirectoryName);

    // Beklenen dosyalarinin gercekten commit edildigi kaynak agaci fixture klasorunu cozer.
    public static string SourceRootPath =>
        Path.Combine(FindProjectDirectory(), "EntityFrameworkCore", "Comparison", DirectoryName);

    // Her fixture klasorunu xunit Theory verisi olarak listeler.
    public static IEnumerable<object[]> Cases()
    {
        return Directory
            .EnumerateDirectories(RootPath)
            .Select(directory => new object[] { Path.GetFileName(directory) })
            .OrderBy(data => (string)data[0], StringComparer.Ordinal);
    }

    // Bir fixture dosyasinin tam yolunu verir.
    public static string PathOf(string caseName, string fileName)
    {
        return Path.Combine(RootPath, caseName, fileName);
    }

    // Spec dosyasini okuyucunun bekledigi ham bayt dizisine cevirir.
    public static byte[] ReadDocument(string caseName, string fileName)
    {
        return File.ReadAllBytes(PathOf(caseName, fileName));
    }

    // Beklenen bulgu satirlarini diskten okur.
    public static IReadOnlyList<string> ReadExpected(string caseName)
    {
        var path = PathOf(caseName, ExpectedFindingsFileName);
        return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path)) ?? [];
    }

    // Bulgu satirlarini insan-okur ve diff'lenebilir JSON dizisi olarak kaynak agacina yazar.
    public static void WriteExpected(string caseName, IReadOnlyList<string> lines)
    {
        var json = JsonSerializer.Serialize(lines, new JsonSerializerOptions { WriteIndented = true });
        var path = Path.Combine(SourceRootPath, caseName, ExpectedFindingsFileName);
        File.WriteAllText(path, json + Environment.NewLine);
    }

    // Test ciktisindan yukari yurayerek test projesinin kaynak klasorunu bulur.
    private static string FindProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, ProjectFileName)))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
               ?? throw new InvalidOperationException($"{ProjectFileName} bulunamadi.");
    }

    // Bulguyu sirasi korunan tek satirlik kararli metne indirger.
    public static string Describe(Finding finding)
    {
        var builder = new StringBuilder()
            .Append(finding.KindCode)
            .Append(" | ")
            .Append(finding.SeverityCode)
            .Append(" | ")
            .Append(finding.DirectionCode)
            .Append(" | ")
            .Append(DescribeAddress(finding.Address));

        if (finding.OldValue is not null || finding.NewValue is not null)
        {
            builder.Append(" | ").Append(finding.OldValue ?? "-").Append(" -> ").Append(finding.NewValue ?? "-");
        }

        return builder.ToString();
    }

    // Adresin yalniz dolu bilesenlerini kararli sirada birlestirir.
    private static string DescribeAddress(FindingAddress address)
    {
        var parts = new List<string>();
        Append(parts, "method", address.HttpMethod);
        Append(parts, "path", address.Path);
        Append(parts, "operation", address.OperationId);
        Append(parts, "schema", address.SchemaName);
        Append(parts, "property", address.PropertyPath);
        Append(parts, "parameter", address.ParameterName);
        Append(parts, "status", address.ResponseStatus);
        Append(parts, "media", address.MediaType);
        return string.Join(";", parts);
    }

    // Dolu adres bilesenini ad=deger olarak listeye ekler.
    private static void Append(List<string> parts, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add($"{name}={value}");
        }
    }
}

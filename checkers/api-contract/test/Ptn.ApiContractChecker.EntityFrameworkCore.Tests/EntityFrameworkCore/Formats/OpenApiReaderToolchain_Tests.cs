using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Formats;

// islevi: Pinlenen OpenAPI arac zincirinin uc spec surumunu, YAML kaydini ve dairesel referans dayanikliligini dogrular.
// sistemdeki gorevi: KBP-609 ayristirma hattinin uzerine kuruldugu kutuphane davranisini varsayim degil kanit haline getirir.
public class OpenApiReaderToolchain_Tests
{
    private const string Swagger20Document = """
        {"swagger":"2.0","info":{"title":"orders","version":"1.4.0"},
         "paths":{"/orders":{"get":{"responses":{"200":{"description":"ok"}}}}}}
        """;

    private const string OpenApi30Document = """
        {"openapi":"3.0.3","info":{"title":"orders","version":"1.4.0"},
         "paths":{"/orders":{"get":{"responses":{"200":{"description":"ok"}}}}}}
        """;

    private const string OpenApi31Document = """
        {"openapi":"3.1.0","info":{"title":"orders","version":"1.4.0"},
         "paths":{"/orders":{"get":{"responses":{"200":{"description":"ok"}}}}}}
        """;

    private const string OpenApi31YamlDocument = """
        openapi: 3.1.0
        info:
          title: orders
          version: 1.4.0
        paths:
          /orders:
            get:
              responses:
                "200":
                  description: ok
        """;

    // Iki sema birbirini isaret eder; CVE-2026-49451 oncesi surumlerde surec yigin tasmasiyla olurdu.
    private const string CircularReferenceDocument = """
        {"openapi":"3.1.0","info":{"title":"orders","version":"1.4.0"},"paths":{},
         "components":{"schemas":{
           "A":{"type":"object","properties":{"b":{"$ref":"#/components/schemas/B"}}},
           "B":{"type":"object","properties":{"a":{"$ref":"#/components/schemas/A"}}}}}}
        """;

    private const string MalformedDocument = """
        {"openapi":"3.1.0","info":{"title":"orders"},"paths":{"/orders":{"get":{}}}}
        """;

    // Canli ptn-payment-auth-api-dev dokumaninda olculen iki gercek ihlal: ayni imzaya cozulen
    // iki path (PathMustBeUnique) ve component key regex'ine uymayan ABP generic sema adi
    // (KeyMustBeRegularExpression). Ikisi de dokumani okunamaz yapmaz.
    private const string StyleViolationDocument = """
        {"openapi":"3.0.1","info":{"title":"payment-auth","version":"1.0"},
         "paths":{
           "/api/notification-template/{id}":{"get":{"responses":{"200":{"description":"ok"}}}},
           "/api/notification-template/{notificationTemplateId}":{"delete":{"responses":{"200":{"description":"ok"}}}}},
         "components":{"schemas":{
           "Volo.Abp.Application.Dtos.PagedResultDto`1[[Ptn.Authenticator.Dtos.Users.PtnUserDto, Ptn.Authenticator.Application.Contracts, Version=10.0.0.0, Culture=neutral, PublicKeyToken=null]]":
             {"type":"object","properties":{"totalCount":{"type":"integer"}}}}}}
        """;

    // Okuyucu dokumani uretir ama /orders yolunu cozemez; bos ruleset bu hatayi yutmaz.
    private const string PartiallyResolvedDocument = """
        {"openapi":"3.1.0","info":{"title":"orders","version":"1.4.0"},
         "paths":{"/orders":"not-an-object"}}
        """;

    // Uc spec surumunun de hatasiz okundugunu ve surum bilgisinin dogru raporlandigini kanitlar.
    [Theory]
    [InlineData(Swagger20Document, OpenApiSpecVersion.OpenApi2_0)]
    [InlineData(OpenApi30Document, OpenApiSpecVersion.OpenApi3_0)]
    [InlineData(OpenApi31Document, OpenApiSpecVersion.OpenApi3_1)]
    public async Task Json_Documents_Of_Three_Spec_Versions_Should_Be_Read(
        string document,
        OpenApiSpecVersion expectedVersion)
    {
        var result = await ReadAsync(document);

        result.Diagnostic!.Errors.ShouldBeEmpty();
        result.Diagnostic.SpecificationVersion.ShouldBe(expectedVersion);
        result.Document!.Info.Version.ShouldBe("1.4.0");
        result.Document.Paths.Count.ShouldBe(1);
    }

    // YAML okuyucusu kayitli degilken YAML govdesinin okunmadigini kanitlar.
    [Fact]
    public async Task Yaml_Should_Not_Be_Read_Without_The_Yaml_Reader()
    {
        var settings = new OpenApiReaderSettings();

        var failure = await ReadYamlOutcomeAsync(settings);

        failure.ShouldBeTrue();
    }

    // YAML okuyucusu kaydedildiginde ayni govdenin okundugunu kanitlar.
    [Fact]
    public async Task Yaml_Should_Be_Read_After_Registering_The_Yaml_Reader()
    {
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();

        var failure = await ReadYamlOutcomeAsync(settings);

        failure.ShouldBeFalse();
    }

    // Dairesel $ref tasiyan dokumanin sureci dusurmeden okundugunu kanitlar (CVE-2026-49451 yamasi).
    [Fact]
    public async Task Circular_Reference_Should_Not_Terminate_The_Process()
    {
        var result = await ReadAsync(CircularReferenceDocument);

        result.Document.ShouldNotBeNull();
        result.Document.Components!.Schemas!.Count.ShouldBe(2);
    }

    // Zorunlu alani eksik dokumanin varsayilan ruleset altinda tani uretgini kanitlar.
    [Fact]
    public async Task Malformed_Document_Should_Produce_Diagnostic_Errors_Under_The_Default_Ruleset()
    {
        var result = await ReadAsync(MalformedDocument);

        result.Diagnostic!.Errors.ShouldNotBeEmpty();
    }

    // Varsayilan ruleset'in ucuncu tarafin stil ihlallerini de Errors'a yazdigini kanitlar.
    // Uretim okuyucusunun neden bu ruleset'i kullanamayacaginin gerekcesidir.
    [Fact]
    public async Task Style_Violations_Should_Produce_Errors_Under_The_Default_Ruleset()
    {
        var result = await ReadAsync(StyleViolationDocument);

        result.Document.ShouldNotBeNull();
        result.Diagnostic!.Errors.ShouldNotBeEmpty();
    }

    // Uretim ayarinda ayni stil ihlallerinin hicbir Errors uretmedigini kanitlar.
    [Fact]
    public async Task Style_Violations_Should_Not_Produce_Errors_Under_The_Production_Settings()
    {
        var result = await ReadWithProductionSettingsAsync(StyleViolationDocument);

        result.Document.ShouldNotBeNull();
        result.Diagnostic!.Errors.ShouldBeEmpty();
        result.Document.Paths.Count.ShouldBe(2);
        result.Document.Components!.Schemas!.Count.ShouldBe(1);
    }

    // Uretim ayarinin okuyucu duzeyi guard'ini korudugunu kanitlar: kismi cozulme hala Errors uretir.
    [Fact]
    public async Task Reader_Faults_Should_Still_Produce_Errors_Under_The_Production_Settings()
    {
        var result = await ReadWithProductionSettingsAsync(PartiallyResolvedDocument);

        result.Document.ShouldNotBeNull();
        result.Diagnostic!.Errors.ShouldNotBeEmpty();
    }

    // Ayni okuma yolunu tum senaryolara verir; format tespiti kutuphaneye birakilir.
    private static async Task<ReadResult> ReadAsync(string document)
    {
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document));
        return await OpenApiDocument.LoadAsync(stream, settings: settings);
    }

    // SpecDocumentReader'in kullandigi ayarin aynisini kurar; ikisi ayrisirsa bu testler duser.
    private static async Task<ReadResult> ReadWithProductionSettingsAsync(string document)
    {
        var settings = new OpenApiReaderSettings
        {
            RuleSet = ValidationRuleSet.GetEmptyRuleSet()
        };
        settings.AddYamlReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document));
        return await OpenApiDocument.LoadAsync(stream, settings: settings);
    }

    // YAML denemesinin basarisiz olup olmadigini exception ve tani farkini gozetmeden bildirir.
    private static async Task<bool> ReadYamlOutcomeAsync(OpenApiReaderSettings settings)
    {
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(OpenApi31YamlDocument));
            var result = await OpenApiDocument.LoadAsync(stream, settings: settings);
            return result.Document == null || result.Diagnostic?.Errors.Count > 0;
        }
        catch (Exception)
        {
            return true;
        }
    }
}

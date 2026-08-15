using Ptn.TestModule.Managers.Compilation;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Catalog;

// islevi: Derlenmis belgeden dokunulan operasyon kumesinin cikarilmasini dogrular.
// sistemdeki gorevi: Kapsam raporunun pay tarafinin regresyon kapisidir; payda bu tarafta hesaplanmaz.
public class ScenarioCoverageManagerTests
{
    private const string CompiledDocument = """
        arazzo: 1.0.1
        sourceDescriptions:
          - name: databaseChecker
            url: http://db/openapi.json
        workflows:
          - workflowId: checkout
            steps:
              - stepId: create-order
                operationId: createOrder
              - stepId: read-order
                operationId: getOrder
              - stepId: assert-row
                operationPath: '{$sourceDescriptions.databaseChecker.url}#/paths/~1assertions~1row/post'
        """;

    // Sozlesme adimlarinin operationId degerleri tekil ve sirali dondurulmelidir.
    [Fact]
    public void Compiled_document_should_yield_its_touched_api_operations()
    {
        var operations = ArazzoCompilerManager.ReadTouchedOperations(CompiledDocument);

        operations.ShouldBe(["createOrder", "getOrder"]);
    }

    // Database Checker adimlari API operasyonu sayilmamalidir.
    [Fact]
    public void Database_checker_steps_should_not_count_as_api_operations()
    {
        var operations = ArazzoCompilerManager.ReadTouchedOperations(CompiledDocument);

        operations.ShouldNotContain(operation => operation.Contains("databaseChecker"));
    }

    // Bos veya bozuk belge kapsam raporunu kirmamalidir.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("this: is: not: arazzo")]
    public void A_missing_or_broken_document_should_contribute_nothing(string document)
    {
        ArazzoCompilerManager.ReadTouchedOperations(document).ShouldBeEmpty();
    }
}

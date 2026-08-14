using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Compilation;
using Ptn.TestModule.Interface.Compilation;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Managers.Compilation;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Compilation;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Compilation;

// islevi: x-checknexus-db derlemesinin profil cozumu, endpoint ve deterministik hash kurallarini dogrular.
// sistemdeki gorevi: Runner plugin'i veya ozel DSL olmadan uretilen Arazzo 1.0.1 adim sozlesmesini korur.
public class ArazzoCompilerManagerTests
{
    private static readonly Guid SpecSnapshotId = Guid.NewGuid();

    // Uc DB operation uzantisini somut profil kolonlari ve gercek checker HTTP yollarina derler.
    [Fact]
    public async Task Should_compile_all_database_assertion_operations_deterministically()
    {
        var linter = new LinterStub(isValid: true);
        var manager = CreateManager(linter);

        var first = await manager.CompileAsync(SourceDocument, CreateProfilePack(), SpecSnapshotId);
        var second = await manager.CompileAsync(SourceDocument, CreateProfilePack(), SpecSnapshotId);

        first.CompiledAssertionCount.ShouldBe(3);
        first.IsSchemaValid.ShouldBeTrue();
        first.CompiledHash.Length.ShouldBe(64);
        first.CompiledHash.ShouldBe(second.CompiledHash);
        first.CompiledDocument.ShouldBe(second.CompiledDocument);
        first.CompiledDocument.ShouldNotContain("x-checknexus-db");
        first.CompiledDocument.ShouldContain("~1api~1comparison~1assertions~1row/post");
        first.CompiledDocument.ShouldContain("~1api~1comparison~1assertions~1count/post");
        first.CompiledDocument.ShouldContain("~1api~1comparison~1assertions~1absent/post");
        first.CompiledDocument.ShouldContain("schemaName: identity");
        first.CompiledDocument.ShouldContain("tableName: users");
        first.CompiledDocument.ShouldContain("columnName: email");
        first.CompiledDocument.ShouldContain("$response.body#/data/passed == true");
        linter.LastDocument.ShouldBe(second.CompiledDocument);
    }

    // Respect'in desteklemedigi XPath criterion bulunan belgeyi process cagrisi oncesinde reddeder.
    [Fact]
    public async Task Should_reject_xpath_before_lint_process()
    {
        var linter = new LinterStub(isValid: true);
        var manager = CreateManager(linter);
        var source = SourceDocument.Replace(
            "type: jsonpath",
            "type: xpath",
            StringComparison.Ordinal);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.CompileAsync(source, CreateProfilePack(), SpecSnapshotId));

        exception.Code.ShouldBe(TestModuleCompilationErrorCodes.XPathCriteriaUnsupported);
        linter.CallCount.ShouldBe(0);
    }

    // Profil columnMap'inde olmayan kavramsal kolon icin fiziksel kolon adi tahmin etmez.
    [Fact]
    public async Task Should_reject_unbound_concept_column()
    {
        var manager = CreateManager(new LinterStub(isValid: true));
        var source = SourceDocument.Replace(
            "naturalKey:",
            "unknownRole:",
            StringComparison.Ordinal);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.CompileAsync(source, CreateProfilePack(), SpecSnapshotId));

        exception.Code.ShouldBe(TestModuleCompilationErrorCodes.ConceptColumnNotBound);
    }

    // Redocly sema sonucunu Published kapisinin tuketebilecegi bool ve tani alanlarinda tasir.
    [Fact]
    public async Task Should_propagate_redocly_schema_failure()
    {
        var manager = CreateManager(new LinterStub(isValid: false, diagnostics: "struct error"));

        var result = await manager.CompileAsync(SourceDocument, CreateProfilePack(), SpecSnapshotId);

        result.IsSchemaValid.ShouldBeFalse();
        result.LintDiagnostics.ShouldBe("struct error");
    }

    // Gercek profil kurali ve test linter'i ile derleyici use-case'ini kurar.
    private static ArazzoCompilerManager CreateManager(IArazzoDocumentLinter linter)
        => new(new ProfilePackManager(), linter);

    // Onayli Subject kavramini somut users tablo ve kolonlarina baglar.
    private static ProfilePack CreateProfilePack()
        => new()
        {
            ProfileKey = "unit-profile",
            Revision = "1",
            DbSchemaFingerprint = "sha256:profile",
            Bindings =
            [
                new ConceptBinding
                {
                    ConceptCode = PtnConceptCodes.Subject,
                    DbSchemaName = "identity",
                    TableName = "users",
                    ColumnMap = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["identity"] = "id",
                        ["naturalKey"] = "email"
                    },
                    PatternCode = PtnBindingPatternCodes.SemanticEntity,
                    StateCode = PtnBindingStateCodes.Approved,
                    ApprovedBy = "reviewer"
                }
            ]
        };

    private const string SourceDocument = """
        arazzo: 1.0.1
        info:
          title: Database assertion compilation
          version: 1.0.1
        sourceDescriptions:
          - name: databaseChecker
            url: ./database-checker.openapi.yaml
            type: openapi
        workflows:
          - workflowId: verify-subject
            inputs:
              type: object
              properties:
                dbConnectionId: { type: string, format: uuid }
                subjectId: { type: string }
                email: { type: string }
            steps:
              - stepId: verify-subject-row
                x-checknexus-db:
                  operation: assertRow
                  concept: Subject
                  key:
                    identity: '{$inputs.subjectId}'
                  expect:
                    naturalKey:
                      matcher: equals
                      value: '{$inputs.email}'
                  timeoutMs: 5000
                  pollIntervalMs: 250
                successCriteria:
                  - context: $response.body
                    condition: $.data
                    type: jsonpath
              - stepId: count-subjects
                x-checknexus-db:
                  operation: assertCount
                  concept: Subject
                  key:
                    naturalKey: '{$inputs.email}'
                  cardinality: atLeast 1
              - stepId: verify-subject-absent
                x-checknexus-db:
                  operation: assertAbsent
                  concept: Subject
                  key:
                    identity: missing
        """;

    // islevi: Domain testinde process calistirmadan lint sonucunu ve cagrilan belgeyi kaydeder.
    // sistemdeki gorevi: Derleyicinin IArazzoDocumentLinter portuna devrini gozlemlenebilir yapar.
    private sealed class LinterStub : IArazzoDocumentLinter
    {
        private readonly bool _isValid;
        private readonly string _diagnostics;

        public LinterStub(bool isValid, string diagnostics = "lint clean")
        {
            _isValid = isValid;
            _diagnostics = diagnostics;
        }

        public int CallCount { get; private set; }
        public string LastDocument { get; private set; } = string.Empty;

        // Derlenmis belgeyi kaydeder ve teste ozel lint kararini dondurur.
        public Task<ArazzoLintResult> LintAsync(
            string document,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastDocument = document;
            return Task.FromResult(new ArazzoLintResult
            {
                IsValid = _isValid,
                Diagnostics = _diagnostics
            });
        }
    }
}

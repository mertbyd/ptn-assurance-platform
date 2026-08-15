using System.Text.Json;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: HAR entry'lerinin adima StepKey ile baglanmasini ve veritabani adimlarinin isaretlenmesini dogrular.
// sistemdeki gorevi: Konumla eslestirme yolunun geri gelmesini ve header/echo celiskisinin gizlenmesini engeller (ADR-0021, ADR-0022).
public class HarInterpreterTests
{
    // Echo edilen kimlikler konum sirasindan farkliysa bile hukum dogru adima baglanmalidir.
    [Fact]
    public void Should_bind_entries_by_step_key_not_by_position()
    {
        var document = new HarInterpreter().Interpret(HarContent, CreateFacts());

        document.Entries.Count.ShouldBe(3);
        document.Entries[0].Ordinal.ShouldBe(1);
        document.Entries[0].StepKey.ShouldBe("verify-subject-row");
        document.Entries[1].StepKey.ShouldBe("create-order");
        document.Entries[2].StepKey.ShouldBeNull();
        document.HasUnboundEntries.ShouldBeTrue();
    }

    // Belgede bildirilmemis bir kimlik kabul edilmemeli; hukum baglanamaz kalmalidir.
    [Fact]
    public void Should_leave_entry_unbound_when_step_key_is_not_declared()
    {
        var facts = CreateFacts();
        facts.StepKeys = ["create-order"];

        var document = new HarInterpreter().Interpret(
            CreateSingleEntryHar("unknown-step", "{\"id\":1}"),
            facts);

        document.Entries[0].StepKey.ShouldBeNull();
        document.HasUnboundEntries.ShouldBeTrue();
    }

    // Siradan SUT yaniti echo etmediginde istek header'i entry'yi adima baglamalidir.
    [Fact]
    public void Should_bind_from_request_header_without_response_echo()
    {
        var document = new HarInterpreter().Interpret(
            CreateSingleEntryHar("create-order", "{\"id\":1}"),
            CreateFacts());

        document.Entries[0].StepKey.ShouldBe("create-order");
        document.HasUnboundEntries.ShouldBeFalse();
    }

    // Header bulunmadiginda checker response echo'su geriye uyumlu korelasyon kaynagi olmalidir.
    [Fact]
    public void Should_fall_back_to_response_echo_when_request_header_is_missing()
    {
        var document = new HarInterpreter().Interpret(
            CreateSingleEntryHar(null, "{\"correlation\":{\"stepKey\":\"verify-subject-row\"}}"),
            CreateFacts());

        document.Entries[0].StepKey.ShouldBe("verify-subject-row");
        document.HasUnboundEntries.ShouldBeFalse();
    }

    // Header ile checker echo'su celisirse sessizce birine guvenilmemelidir.
    [Fact]
    public void Should_leave_entry_unbound_when_correlation_sources_conflict()
    {
        var document = new HarInterpreter().Interpret(
            CreateSingleEntryHar(
                "create-order",
                "{\"correlation\":{\"stepKey\":\"verify-subject-row\"}}"),
            CreateFacts());

        document.Entries[0].StepKey.ShouldBeNull();
        document.HasUnboundEntries.ShouldBeTrue();
    }

    // Derlenmis veritabani adimlari isaretlenmeli; dagitici bunlari yeniden cagirmamalidir.
    [Fact]
    public void Should_mark_database_assertion_entries()
    {
        var document = new HarInterpreter().Interpret(HarContent, CreateFacts());

        document.Entries[0].IsDatabaseAssertion.ShouldBeTrue();
        document.Entries[1].IsDatabaseAssertion.ShouldBeFalse();
        document.Entries[0].StatusCode.ShouldBe(200);
        document.Entries[1].StatusCode.ShouldBe(201);
        document.CreatorName.ShouldBe("respect");
    }

    // Bozuk artefakt yargi asamasina gecmemeli, kararli koda cevrilmelidir.
    [Fact]
    public void Should_reject_unreadable_har_content()
    {
        var exception = Should.Throw<BusinessException>(() =>
            new HarInterpreter().Interpret("not-a-har", CreateFacts()));

        exception.Code.ShouldBe(TestModuleRunErrorCodes.HarNotProduced);
    }

    // Belgede bildirilen adim kimliklerini tasiyan olgu modelini kurar.
    private static WorkflowDocumentFacts CreateFacts()
    {
        return new WorkflowDocumentFacts
        {
            ArazzoVersion = "1.0.1",
            StepKeys = ["create-order", "verify-subject-row"]
        };
    }

    // Tek entry'li HAR'i istek ve yanit korelasyon kaynaklarini bagimsiz kuracak sekilde uretir.
    private static string CreateSingleEntryHar(string? requestStepKey, string responseBody)
    {
        var headers = requestStepKey is null
            ? "[]"
            : $$"""[{ "name": "{{WorkflowRunnerConsts.StepKeyHeaderName}}", "value": "{{requestStepKey}}" }]""";
        return $$"""
            {
              "log": {
                "version": "1.2",
                "creator": { "name": "respect", "version": "2.14.0" },
                "entries": [
                  {
                    "startedDateTime": "2026-08-14T10:00:00.000Z",
                    "time": 12,
                    "request": {
                      "method": "POST",
                      "url": "https://api.test/orders",
                      "headers": {{headers}}
                    },
                    "response": {
                      "status": 200,
                      "content": {
                        "mimeType": "application/json",
                        "text": {{JsonSerializer.Serialize(responseBody)}}
                      }
                    }
                  }
                ]
              }
            }
            """;
    }

    // Ilk entry veritabani adimi, ikincisi API adimi, ucuncusu korelasyonsuzdur.
    private const string HarContent = """
        {
          "log": {
            "version": "1.2",
            "creator": { "name": "respect", "version": "2.14.0" },
            "entries": [
              {
                "startedDateTime": "2026-08-14T10:00:00.000Z",
                "time": 12,
                "request": {
                  "method": "POST",
                  "url": "https://checker.test/api/comparison/assertions/row",
                  "headers": [
                    { "name": "X-CheckNexus-Step-Key", "value": "verify-subject-row" }
                  ]
                },
                "response": {
                  "status": 200,
                  "content": {
                    "mimeType": "application/json",
                    "text": "{\"outcomeCode\":\"Passed\",\"correlation\":{\"stepKey\":\"verify-subject-row\"}}"
                  }
                }
              },
              {
                "startedDateTime": "2026-08-14T10:00:01.000Z",
                "time": 30,
                "request": {
                  "method": "POST",
                  "url": "https://api.test/orders",
                  "headers": [
                    { "name": "X-CheckNexus-Step-Key", "value": "create-order" }
                  ]
                },
                "response": {
                  "status": 201,
                  "content": {
                    "mimeType": "application/json",
                    "text": "{\"id\":1}"
                  }
                }
              },
              {
                "startedDateTime": "2026-08-14T10:00:02.000Z",
                "time": 8,
                "request": {
                  "method": "GET",
                  "url": "https://api.test/orders/1",
                  "headers": []
                },
                "response": {
                  "status": 200,
                  "content": { "mimeType": "application/json", "text": "{\"id\":1}" }
                }
              }
            ]
          }
        }
        """;
}

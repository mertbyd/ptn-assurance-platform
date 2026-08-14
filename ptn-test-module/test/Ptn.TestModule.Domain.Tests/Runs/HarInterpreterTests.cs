using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Models.Runs;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Runs;

// islevi: HAR entry'lerinin adima StepKey ile baglanmasini ve veritabani adimlarinin isaretlenmesini dogrular.
// sistemdeki gorevi: AUDIT-0001 BULGU-01'deki konumla eslestirme yolunun geri gelmesini engeller (ADR-0021).
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

        var document = new HarInterpreter().Interpret(HarContent, facts);

        document.Entries[0].StepKey.ShouldBeNull();
        document.Entries[1].StepKey.ShouldBe("create-order");
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
                  "url": "https://checker.test/api/comparison/assertions/row"
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
                  "url": "https://api.test/orders"
                },
                "response": {
                  "status": 201,
                  "content": {
                    "mimeType": "application/json",
                    "text": "{\"correlation\":{\"stepKey\":\"create-order\"}}"
                  }
                }
              },
              {
                "startedDateTime": "2026-08-14T10:00:02.000Z",
                "time": 8,
                "request": {
                  "method": "GET",
                  "url": "https://api.test/orders/1"
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

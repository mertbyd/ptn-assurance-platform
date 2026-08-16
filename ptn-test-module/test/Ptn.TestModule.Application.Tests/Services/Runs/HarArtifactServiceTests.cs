using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Managers.Runs;
using Ptn.TestModule.Services.Runs;
using Shouldly;
using Volo.Abp.BlobStoring;
using Volo.Abp.Settings;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Runs;

// islevi: HAR artefakt sinirinin adlandirilmis container uzerinde yazma, okuma ve silme turunu dogrular.
// sistemdeki gorevi: Container adinin tek sahibinin Domain.Shared olmasini ve artefakt davranisinin degismemesini korur.
public class HarArtifactServiceTests
{
    private const string BlobName = "tenant/run/trace.har";

    // Container yalniz Domain.Shared'daki ad uzerinden cozulmelidir.
    [Fact]
    public void Should_resolve_the_container_by_its_domain_shared_name()
    {
        var container = CreateContainer([]);
        var factory = Substitute.For<IBlobContainerFactory>();
        factory.Create(Arg.Any<string>()).Returns(container);

        _ = new HarArtifactService(factory, CreatePlanner(), new HarInterpreter());

        factory.Received(1).Create(HarArtifactConsts.ContainerName);
    }

    // Yazilan artefakt ayni adla geri okunmali, silindikten sonra bulunmamalidir.
    [Fact]
    public async Task Should_save_read_and_delete_the_har_artifact()
    {
        var service = CreateService();

        var savedName = await service.SaveAsync(BlobName, HarContent);
        var readContent = await service.ReadAsync(BlobName);
        await service.DeleteAsync(BlobName);

        savedName.ShouldBe(BlobName);
        readContent.ShouldBe(HarContent);
        (await service.ReadAsync(BlobName)).ShouldBeNull();
    }

    // Kimlik basligi kalici depoya asla ham gitmemelidir (KBP-112 kosum kimligi kanali).
    [Fact]
    public async Task Should_not_store_credential_headers_in_the_har_artifact()
    {
        var service = CreateService();

        await service.SaveAsync(BlobName, CredentialHarContent);
        var stored = await service.ReadAsync(BlobName);

        stored.ShouldNotBeNull();
        stored.ShouldNotContain("Bearer secret-token");
        stored.ShouldNotContain("session=abc123");
        stored.ShouldNotContain("key-12345");
        stored.ShouldContain(HarArtifactConsts.RedactedValue);
        stored.ShouldContain(WorkflowRunnerConsts.StepKeyHeaderName);
        stored.ShouldContain("step-one");
    }

    // Artefakt sinirini bellekte tutulan adlandirilmis container'a baglar.
    private static HarArtifactService CreateService()
    {
        var container = CreateContainer([]);
        var factory = Substitute.For<IBlobContainerFactory>();
        factory.Create(HarArtifactConsts.ContainerName).Returns(container);
        return new HarArtifactService(factory, CreatePlanner(), new HarInterpreter());
    }

    // Artefakt guard'larini gercek Manager uzerinden calistiran varsayilan planner'i kurar.
    private static WorkflowRunPlanner CreatePlanner()
    {
        return new WorkflowRunPlanner(Substitute.For<ISettingProvider>());
    }

    // Container sozlesmesini bellekteki bir blob sozlugu uzerinde karsilar.
    private static IBlobContainer CreateContainer(Dictionary<string, byte[]> blobs)
    {
        var container = Substitute.For<IBlobContainer>();
        container.SaveAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                using var buffer = new MemoryStream();
                call.ArgAt<Stream>(1).CopyTo(buffer);
                blobs[call.ArgAt<string>(0)] = buffer.ToArray();
                return Task.CompletedTask;
            });
        container.GetOrNullAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult<Stream?>(
                blobs.TryGetValue(call.ArgAt<string>(0), out var bytes) ? new MemoryStream(bytes) : null));
        container.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(blobs.Remove(call.ArgAt<string>(0))));
        return container;
    }

    private const string HarContent = """
        {
          "log": {
            "version": "1.2",
            "creator": { "name": "respect", "version": "2.14.0" },
            "entries": []
          }
        }
        """;

    private const string CredentialHarContent = """
        {
          "log": {
            "version": "1.2",
            "creator": { "name": "respect", "version": "2.14.0" },
            "entries": [
              {
                "startedDateTime": "2026-08-16T10:00:00.000Z",
                "time": 12,
                "request": {
                  "method": "GET",
                  "url": "https://sut.example.test/orders",
                  "headers": [
                    { "name": "Authorization", "value": "Bearer secret-token" },
                    { "name": "X-Api-Key", "value": "key-12345" },
                    { "name": "X-CheckNexus-Step-Key", "value": "step-one" }
                  ],
                  "cookies": [ { "name": "session", "value": "session=abc123" } ]
                },
                "response": {
                  "status": 200,
                  "headers": [ { "name": "Set-Cookie", "value": "session=abc123; Path=/" } ],
                  "content": { "mimeType": "application/json", "text": "{}" }
                }
              }
            ]
          }
        }
        """;
}

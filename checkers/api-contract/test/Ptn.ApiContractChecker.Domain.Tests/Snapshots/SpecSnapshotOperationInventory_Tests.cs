using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Constants.Snapshots.Lookups;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Ptn.ApiContractChecker.Managers.Conformance;
using Ptn.ApiContractChecker.Managers.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Users;
using Xunit;

namespace Ptn.ApiContractChecker.Snapshots;

// islevi: Snapshot operasyon envanterinin sayfalama, kapali kume filtreleri ve byte butcesi sinirlarini kanitlar.
// sistemdeki gorevi: Envanterin belgeden hesaplandigini ve satirin hafif kaldigini regresyona karsi sabitler.
public class SpecSnapshotOperationInventory_Tests
{
    [Fact]
    public async Task Unknown_Snapshot_Should_Return_Not_Found_Outcome_Without_Rows()
    {
        var manager = BuildManager(Substitute.For<ISpecSchemaResolver>());

        var result = await manager.ListOperationsAsync(null, BuildRequest());

        result.OutcomeCode.ShouldBe(ConformanceOutcomeCodes.SnapshotNotFound);
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Inventory_Should_Page_In_Stable_Path_Then_Method_Order()
    {
        var fixture = BuildFixture(BuildInventorySnapshot());

        var result = await BuildManager(fixture.Resolver)
            .ListOperationsAsync(fixture.Entity, BuildRequest(skipCount: 1, maxResultCount: 2));

        result.OutcomeCode.ShouldBe(ConformanceOutcomeCodes.Passed);
        result.TotalCount.ShouldBe(4);
        result.Items.Select(row => $"{row.Method} {row.Path}")
            .ShouldBe(["GET /customers/{id}", "GET /orders"]);
    }

    [Fact]
    public async Task Closed_Set_Filters_Should_Narrow_By_Method_Prefix_And_Request_Body()
    {
        var fixture = BuildFixture(BuildInventorySnapshot());
        var manager = BuildManager(fixture.Resolver);

        var byMethod = await manager.ListOperationsAsync(
            fixture.Entity, BuildRequest(methodCode: SpecOperationMethodCodes.Get));
        var byPrefix = await manager.ListOperationsAsync(
            fixture.Entity, BuildRequest(pathPrefix: "/orders"));
        var withBody = await manager.ListOperationsAsync(
            fixture.Entity, BuildRequest(hasRequestBody: true));
        var withoutBody = await manager.ListOperationsAsync(
            fixture.Entity, BuildRequest(hasRequestBody: false));

        byMethod.Items.Select(row => row.Path).ShouldBe(["/customers/{id}", "/orders"]);
        byPrefix.Items.Select(row => row.Method).ShouldBe(["GET", "POST"]);
        withBody.Items.Single().OperationId.ShouldBe("createOrder");
        withoutBody.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task Row_Should_Carry_Only_Identity_And_Two_Schema_References()
    {
        var fixture = BuildFixture(BuildInventorySnapshot());

        var result = await BuildManager(fixture.Resolver)
            .ListOperationsAsync(fixture.Entity, BuildRequest(hasRequestBody: true));

        var row = result.Items.Single();
        row.OperationId.ShouldBe("createOrder");
        row.RequestSchemaRef.ShouldBe("CreateOrderDto");
        row.ResponseSchemaRef.ShouldBe("OrderDto");
        typeof(SpecOperationRow).GetProperties().Select(property => property.Name).ShouldBe(
            [
                nameof(SpecOperationRow.OperationId),
                nameof(SpecOperationRow.Method),
                nameof(SpecOperationRow.Path),
                nameof(SpecOperationRow.RequestSchemaRef),
                nameof(SpecOperationRow.ResponseSchemaRef)
            ],
            ignoreOrder: true);
    }

    [Fact]
    public async Task Requested_Page_Size_Above_Ceiling_Should_Report_The_Effective_Ceiling()
    {
        var fixture = BuildFixture(BuildInventorySnapshot());
        var requestedSize = SnapshotOperationInventoryConsts.MaxPageSize + 50;

        var result = await BuildManager(fixture.Resolver)
            .ListOperationsAsync(fixture.Entity, BuildRequest(maxResultCount: requestedSize));

        result.RequestedMaxResultCount.ShouldBe(requestedSize);
        result.EffectiveMaxResultCount.ShouldBe(SnapshotOperationInventoryConsts.MaxPageSize);
        result.IsTruncated.ShouldBeFalse();
    }

    [Fact]
    public async Task Response_Budget_Should_Trim_Rows_And_Flag_Truncation()
    {
        var fixture = BuildFixture(BuildOversizedSnapshot());

        var result = await BuildManager(fixture.Resolver).ListOperationsAsync(
            fixture.Entity,
            BuildRequest(maxResultCount: SnapshotOperationInventoryConsts.MaxPageSize));

        result.IsTruncated.ShouldBeTrue();
        result.Items.Count.ShouldBeLessThan(SnapshotOperationInventoryConsts.MaxPageSize);
        result.EffectiveMaxResultCount.ShouldBe(result.Items.Count);
        result.ResponseBytes.ShouldBeLessThanOrEqualTo(SnapshotOperationInventoryConsts.MaxResponseBytes);
        result.TotalCount.ShouldBe(SnapshotOperationInventoryConsts.MaxPageSize);
    }

    // Varsayilan tam sayfa istegini kurar; her test yalniz ilgilendigi filtreyi degistirir.
    private static SnapshotOperationInventoryRequest BuildRequest(
        string? methodCode = null,
        string? pathPrefix = null,
        bool? hasRequestBody = null,
        int skipCount = 0,
        int maxResultCount = SnapshotOperationInventoryConsts.DefaultPageSize)
        => new(methodCode, pathPrefix, hasRequestBody, skipCount, maxResultCount);

    // Envanter icin dort operasyonlu, iki path ve iki metot tasiyan kucuk belge kurar.
    private static SpecSnapshotModel BuildInventorySnapshot()
    {
        return new SpecSnapshotModel
        {
            Operations =
            [
                BuildOperation("listOrders", "GET", "/orders", null, "OrderListDto"),
                BuildOperation("createOrder", "POST", "/orders", "CreateOrderDto", "OrderDto"),
                BuildOperation("getCustomer", "GET", "/customers/{id}", null, "CustomerDto"),
                BuildOperation(null, "DELETE", "/customers/{id}", null, null)
            ]
        };
    }

    // Tek sayfanin byte tavanini asmasi icin uzun path sablonlu tam sayfa belgesi kurar.
    private static SpecSnapshotModel BuildOversizedSnapshot()
    {
        var model = new SpecSnapshotModel();
        for (var index = 0; index < SnapshotOperationInventoryConsts.MaxPageSize; index++)
        {
            model.Operations.Add(BuildOperation(
                $"operation{index:D3}",
                SpecOperationMethodCodes.Get,
                $"/{new string('a', 600)}/{index:D3}",
                null,
                "ResponseDto"));
        }

        return model;
    }

    private static SpecOperationModel BuildOperation(
        string? operationId,
        string method,
        string path,
        string? requestSchemaReferenceId,
        string? responseSchemaReferenceId)
    {
        var operation = new SpecOperationModel
        {
            OperationId = operationId,
            Method = method,
            Path = path
        };
        if (requestSchemaReferenceId is not null)
        {
            operation.RequestBodies.Add(new SpecRequestBodyModel
            {
                Required = true,
                MediaType = "application/json",
                SchemaReferenceId = requestSchemaReferenceId
            });
        }

        if (responseSchemaReferenceId is not null)
        {
            operation.Responses.Add(new SpecResponseModel
            {
                StatusCode = "200",
                MediaType = "application/json",
                SchemaReferenceId = responseSchemaReferenceId
            });
        }

        return operation;
    }

    private static SpecSnapshotAuthoringManager BuildManager(ISpecSchemaResolver resolver)
        => new(resolver, new OperationResolver(), BuildResultStore());

    private static Fixture BuildFixture(SpecSnapshotModel model)
    {
        var content = new SpecContent(Guid.NewGuid(), "raw", "canonical", "{}", 2, "application/json", null);
        var resolver = Substitute.For<ISpecSchemaResolver>();
        resolver.GetSnapshotAsync(content).Returns(model);
        var entity = new SpecSnapshot(
            Guid.NewGuid(), Guid.NewGuid(), content.Id, Guid.NewGuid(), "1", DateTime.UtcNow, null);
        typeof(SpecSnapshot).GetProperty(nameof(SpecSnapshot.SpecContent))!.SetValue(entity, content);
        return new Fixture(resolver, entity);
    }

    private static SnapshotAuthoringResultStore BuildResultStore()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        return new SnapshotAuthoringResultStore(
            new MemoryCache(new MemoryCacheOptions()),
            Substitute.For<ICurrentTenant>(),
            Substitute.For<ICurrentUser>(),
            Substitute.For<IGuidGenerator>(),
            settings);
    }

    private sealed record Fixture(ISpecSchemaResolver Resolver, SpecSnapshot Entity);
}

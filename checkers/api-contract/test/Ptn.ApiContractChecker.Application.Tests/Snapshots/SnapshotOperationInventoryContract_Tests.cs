using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptn.ApiContractChecker.Application.Mappers.Snapshots;
using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Constants.Conformance.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Constants.Snapshots.Lookups;
using Ptn.ApiContractChecker.Controllers.Snapshots;
using Ptn.ApiContractChecker.Dtos.Snapshots;
using Ptn.ApiContractChecker.ExceptionCodes.Snapshots;
using Ptn.ApiContractChecker.FluentValidation.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;
using Ptn.ApiContractChecker.Permissions;
using Ptn.ApiContractChecker.Services.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Snapshots;

// islevi: Envanter ucunun validator, Mapperly, hafif satir, interface ve rota sozlesmelerini kanitlar.
// sistemdeki gorevi: Ajan yuzeyinin serbest metin aramasina veya agir yazarlik alanlarina sessizce acilmasini engeller.
public class SnapshotOperationInventoryContract_Tests
{
    [Fact]
    public void Validator_Should_Reject_Unknown_Method_Long_Prefix_And_Empty_Page()
    {
        var validator = new ListSnapshotOperationsInputValidator();

        validator.Validate(new ListSnapshotOperationsInput { MethodCode = "FETCH" }).Errors
            .ShouldContain(error =>
                error.ErrorMessage == SpecSnapshotExceptionCodes.Validation.OperationMethodCodeInvalid);
        validator.Validate(new ListSnapshotOperationsInput
        {
            PathPrefix = new string('a', SnapshotOperationInventoryConsts.MaxPathPrefixLength + 1)
        }).Errors.ShouldContain(error =>
            error.ErrorMessage == SpecSnapshotExceptionCodes.Validation.OperationPathPrefixMaxLength);
        validator.Validate(new ListSnapshotOperationsInput { MaxResultCount = 0 }).IsValid.ShouldBeFalse();
        validator.Validate(new ListSnapshotOperationsInput { SkipCount = -1 }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validator_Should_Accept_Closed_Set_Method_And_Default_Page_Size()
    {
        var input = new ListSnapshotOperationsInput { MethodCode = SpecOperationMethodCodes.Delete };

        var result = new ListSnapshotOperationsInputValidator().Validate(input);

        result.IsValid.ShouldBeTrue();
        input.MaxResultCount.ShouldBe(SnapshotOperationInventoryConsts.DefaultPageSize);
    }

    [Fact]
    public void Mapperly_Should_Carry_The_Page_Budget_And_Every_Row_Field()
    {
        var mapper = new SpecSnapshotMapper();
        var request = mapper.MapToInventoryRequest(new ListSnapshotOperationsInput
        {
            MethodCode = SpecOperationMethodCodes.Post,
            PathPrefix = "/orders",
            HasRequestBody = true,
            SkipCount = 5,
            MaxResultCount = 30
        });
        var source = new SnapshotOperationInventoryResult
        {
            OutcomeCode = ConformanceOutcomeCodes.Passed,
            TotalCount = 7,
            RequestedMaxResultCount = 30,
            EffectiveMaxResultCount = 20,
            IsTruncated = true,
            ResponseBytes = 1234
        };
        source.Items.Add(new SpecOperationRow
        {
            OperationId = "createOrder",
            Method = SpecOperationMethodCodes.Post,
            Path = "/orders",
            RequestSchemaRef = "CreateOrderDto",
            ResponseSchemaRef = "OrderDto"
        });

        var dto = mapper.MapToInventoryDto(source);

        request.MethodCode.ShouldBe(SpecOperationMethodCodes.Post);
        request.PathPrefix.ShouldBe("/orders");
        request.HasRequestBody.ShouldBe(true);
        request.SkipCount.ShouldBe(5);
        request.MaxResultCount.ShouldBe(30);
        dto.OutcomeCode.ShouldBe(ConformanceOutcomeCodes.Passed);
        dto.TotalCount.ShouldBe(7);
        dto.RequestedMaxResultCount.ShouldBe(30);
        dto.EffectiveMaxResultCount.ShouldBe(20);
        dto.IsTruncated.ShouldBeTrue();
        dto.ResponseBytes.ShouldBe(1234);
        var row = dto.Items.Single();
        row.OperationId.ShouldBe("createOrder");
        row.Method.ShouldBe(SpecOperationMethodCodes.Post);
        row.Path.ShouldBe("/orders");
        row.RequestSchemaRef.ShouldBe("CreateOrderDto");
        row.ResponseSchemaRef.ShouldBe("OrderDto");
    }

    [Fact]
    public void Inventory_Row_Should_Not_Expose_Heavy_Authoring_Fields()
    {
        var rowFields = typeof(SnapshotOperationRowDto).GetProperties().Select(property => property.Name).ToList();

        rowFields.ShouldBe(
            [
                nameof(SnapshotOperationRowDto.OperationId),
                nameof(SnapshotOperationRowDto.Method),
                nameof(SnapshotOperationRowDto.Path),
                nameof(SnapshotOperationRowDto.RequestSchemaRef),
                nameof(SnapshotOperationRowDto.ResponseSchemaRef)
            ],
            ignoreOrder: true);
        rowFields.ShouldNotContain(nameof(OperationSummaryDto.RequiredParameters));
        rowFields.ShouldNotContain(nameof(OperationSummaryDto.ResponseFields));
        rowFields.ShouldNotContain(nameof(OperationSummaryDto.SecurityRequirements));
        rowFields.ShouldNotContain(nameof(OperationSummaryDto.ResultRef));
    }

    [Fact]
    public void Interface_And_Controller_Should_Expose_The_Authorized_Inventory_Route()
    {
        typeof(ISpecSnapshotAppService)
            .GetMethod(nameof(ISpecSnapshotAppService.ListOperationsAsync)).ShouldNotBeNull();

        var action = typeof(SpecSnapshotController).GetMethod(nameof(SpecSnapshotController.ListOperations))!;
        action.GetCustomAttribute<HttpGetAttribute>()!.Template
            .ShouldBe(ApiContractCheckerRoutes.SnapshotOperations);
        typeof(SpecSnapshotController).GetCustomAttribute<AuthorizeAttribute>()!.Policy
            .ShouldBe(ApiContractCheckerPermissions.Sources.View);
    }
}

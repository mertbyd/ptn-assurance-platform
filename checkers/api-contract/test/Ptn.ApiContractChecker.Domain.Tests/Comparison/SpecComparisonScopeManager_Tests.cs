using Ptn.ApiContractChecker.Constants.Runs;
using Ptn.ApiContractChecker.Managers.Comparison;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Comparison;

// islevi: Request-scoped OpenAPI kapsam kurallarinin include, exclude ve internal davranisini dogrular.
// sistemdeki gorevi: Kalici scope modeli olmadan iki snapshot'a uygulanacak deterministik filtre semantigini sabitler.
public class SpecComparisonScopeManager_Tests
{
    private readonly SpecComparisonScopeManager _manager = new();

    // Exclude kuralinin include eslesmesinden once kazandigini ve bagli dokumantasyonu da cikardigini kanitlar.
    [Fact]
    public void Exclude_Should_Win_Over_Include_And_Remove_Documentation()
    {
        var snapshot = CreateSnapshot();
        var rules = new List<ContractCheckScopeRuleModel>
        {
            Rule(ContractCheckScopeCodes.Kinds.Include, ContractCheckScopeCodes.Targets.Path, "/api/*"),
            Rule(ContractCheckScopeCodes.Kinds.Exclude, ContractCheckScopeCodes.Targets.Tag, "admin")
        };

        var scoped = _manager.Apply(snapshot, rules, ignoreInternal: false);

        scoped.Operations.Select(operation => operation.OperationId).ShouldBe(["GetOrders"]);
        scoped.Documentation.ShouldContain(item => item.Target == "GET /api/orders");
        scoped.Documentation.ShouldNotContain(item => item.Target == "DELETE /api/users|200");
    }

    // Internal yuzeyleri dislama bayraginin operasyon ve semalari birlikte cikardigini kanitlar.
    [Fact]
    public void IgnoreInternal_Should_Remove_Internal_Operations_And_Schemas()
    {
        var scoped = _manager.Apply(CreateSnapshot(), [], ignoreInternal: true);

        scoped.Operations.ShouldAllBe(operation => !operation.IsInternal);
        scoped.Schemas.ShouldAllBe(schema => !schema.IsInternal);
    }

    // Kapsam davranisini gosteren operasyon, sema ve dokumantasyon fotografini kurar.
    private static SpecSnapshotModel CreateSnapshot()
    {
        return new SpecSnapshotModel
        {
            Operations =
            [
                new SpecOperationModel
                {
                    Path = "/api/orders",
                    Method = "GET",
                    OperationId = "GetOrders",
                    Tags = ["orders"]
                },
                new SpecOperationModel
                {
                    Path = "/api/users",
                    Method = "DELETE",
                    OperationId = "DeleteUser",
                    Tags = ["admin"],
                    IsInternal = true
                }
            ],
            Schemas =
            [
                new SpecSchemaModel { Name = "Order" },
                new SpecSchemaModel { Name = "InternalUser", IsInternal = true }
            ],
            Documentation =
            [
                new SpecDocumentationModel { TargetKind = "operation", Target = "GET /api/orders" },
                new SpecDocumentationModel { TargetKind = "response", Target = "DELETE /api/users|200" }
            ]
        };
    }

    // Tek saf scope kuralini test girdisi olarak kurar.
    private static ContractCheckScopeRuleModel Rule(string kindCode, string targetCode, string pattern)
    {
        return new ContractCheckScopeRuleModel
        {
            KindCode = kindCode,
            TargetCode = targetCode,
            Pattern = pattern
        };
    }
}

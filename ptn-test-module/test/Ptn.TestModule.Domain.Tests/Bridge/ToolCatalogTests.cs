using System.Linq;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: Agent Bridge tool katalogunun aktif ve toplam butcesini mekanik olarak dogrular.
// sistemdeki gorevi: Yazarlik yuzeyleri zenginlesirken yeni tool acilmasini ve tekrarli kod sizmasini engeller.
public class ToolCatalogTests
{
    // Katalog mevcut yedi tool ile kalmali ve ADR-0018'in on iki tool tavanini asmamalidir.
    [Fact]
    public void Should_keep_the_existing_tool_catalog_within_budget()
    {
        var catalog = new ToolCatalogManager().GetCatalog(PtnResponseFormatCodes.Concise);

        catalog.ActiveToolCodes.Count.ShouldBeLessThanOrEqualTo(PtnToolCodes.ActiveMax);
        PtnToolCodes.All.Count.ShouldBeLessThanOrEqualTo(12);
        PtnToolCodes.All.Distinct().Count().ShouldBe(PtnToolCodes.All.Count);
        catalog.ActiveToolCodes.Concat(catalog.DiscoverableToolCodes)
            .ShouldBe(PtnToolCodes.All, ignoreOrder: true);
    }
}

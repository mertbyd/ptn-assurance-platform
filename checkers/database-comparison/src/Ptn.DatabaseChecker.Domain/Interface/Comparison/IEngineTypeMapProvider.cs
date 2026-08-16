using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Interface.Comparison;

// islevi: Bir motorun ham kolon tip adini kanonik tip ailesi ve fidelity koduna esler.
// sistemdeki gorevi: Discovery repository'lerinin motor bilgisini karsilastirma katmanina sizdirmadan normalize etmesini saglayan engine component sozlesmesidir.
public interface IEngineTypeMapProvider : IEngineComponent
{
    // islevi: Bilinen ham tip icin kanonik eslemeyi dondurur; bilinmeyen tipi uydurmadan false ile bildirir.
    bool TryMap(string rawTypeName, out CanonicalTypeMapping mapping);
}

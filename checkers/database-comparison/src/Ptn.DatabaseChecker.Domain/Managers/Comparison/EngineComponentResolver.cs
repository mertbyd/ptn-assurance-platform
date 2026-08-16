using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface.Comparison;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: DI'a kayitli tum TComponent bilesenleri arasindan motor koduna uygun olani secer (schema reader, connection tester... hepsi bunu paylasir).
// sistemdeki gorevi: Yeni motor eklemek = IEngineComponent implemente eden yeni sinif yazmak; bu sinifa ve cagiranlara dokunulmaz (acik/kapali, golden rule 1: pick-by-code tek yerde).
public class EngineComponentResolver<TComponent> : IEngineComponentResolver<TComponent>, ITransientDependency
    where TComponent : class, IEngineComponent
{
    // Konteyner, TComponent implemente eden TUM siniflari bu listeye kendisi toplar; elle kayit yok.
    private readonly IEnumerable<TComponent> _components;

    public EngineComponentResolver(IEnumerable<TComponent> components)
    {
        _components = components;
    }

    public TComponent Resolve(string engineCode)
    {
        // Kod eslesmesi buyuk/kucuk harften bagimsiz; lookup Code'u ile bilesen kimligi arasindaki olasi casing farkina karsi dayanikli.
        var component = _components.FirstOrDefault(
            c => string.Equals(c.EngineCode, engineCode, StringComparison.OrdinalIgnoreCase));

        return component
               ?? throw new BusinessException(ComparisonExceptionCodes.UnsupportedEngine);
    }
}

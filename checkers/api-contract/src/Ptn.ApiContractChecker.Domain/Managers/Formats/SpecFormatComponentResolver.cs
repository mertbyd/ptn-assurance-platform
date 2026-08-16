using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.Interface.Formats;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Formats;

// islevi: DI'a kayitli tum TComponent bilesenleri arasindan format koduna uygun olani secer (spec okuyucu gibi format-ozel bilesenler bunu paylasir).
// sistemdeki gorevi: Yeni format eklemek = ISpecFormatComponent implemente eden yeni sinif yazmak; bu sinifa ve cagiranlara dokunulmaz (acik/kapali, pick-by-code tek yerde).
public class SpecFormatComponentResolver<TComponent> : ISpecFormatComponentResolver<TComponent>, ITransientDependency
    where TComponent : class, ISpecFormatComponent
{
    // Konteyner, TComponent implemente eden TUM siniflari bu listeye kendisi toplar; elle kayit yok.
    private readonly IEnumerable<TComponent> _components;

    public SpecFormatComponentResolver(IEnumerable<TComponent> components)
    {
        _components = components;
    }

    // islevi: Format kodunu bilesene cozer; eslesme yoksa anlamli is-kurali hatasi firlatir.
    public TComponent Resolve(string formatCode)
    {
        // Kod eslesmesi buyuk/kucuk harften bagimsiz; lookup Code'u ile bilesen kimligi arasindaki olasi casing farkina karsi dayanikli.
        var component = _components.FirstOrDefault(
            c => string.Equals(c.FormatCode, formatCode, StringComparison.OrdinalIgnoreCase));

        return component
               ?? throw new BusinessException(SpecFormatExceptionCodes.UnsupportedFormat);
    }
}

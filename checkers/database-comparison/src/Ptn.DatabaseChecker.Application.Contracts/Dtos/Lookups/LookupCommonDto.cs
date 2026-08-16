using System;
using Volo.Abp.Application.Dtos;

namespace Ptn.DatabaseChecker.Dtos.Lookups;

public abstract class LookupCommonDto : EntityDto<Guid>
{
    /// <summary>
    /// Lookup satirinin kararli teknik kodu.
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Lookup satirinin ekranda gosterilecek insan-okur adi.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Lookup satirinin opsiyonel aciklamasi.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Lookup satirinin secilebilir/operasyonel olup olmadigi.
    /// </summary>
    public bool IsActive { get; set; }
}

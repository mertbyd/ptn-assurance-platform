using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Findings;

namespace Ptn.DatabaseChecker.Dtos.Reports;

// islevi: Bir run'in yapisal raporunun tam cevap modeli: run baslik baglami + ozet + tur/tablo gruplari + detayin kendisi (findings).
// sistemdeki gorevi: T8 "ozet + detay" JSON rapor API'sinin ciktisidir (GET {id}/report). Header/detay entity'den Mapperly ile dolar, ozet+gruplar ComparisonReportBuilder aggregation'indan set edilir; detay finding'leri ayrica modellemez, mevcut ComparisonFindingsDto'yu tasir.
public class ComparisonReportDto
{
    /// <summary>
    /// Raporun ait oldugu run kimligi.
    /// </summary>
    public Guid RunId { get; set; }

    /// <summary>
    /// Run'in dogdugu tarif kimligi; ad-hoc run'da null.
    /// </summary>
    public Guid? ComparisonDefinitionId { get; set; }

    /// <summary>
    /// Tarifin adi; ad-hoc run'da null.
    /// </summary>
    public string? ComparisonDefinitionName { get; set; }

    /// <summary>
    /// Kaynak (referans) baglantinin adi.
    /// </summary>
    public string SourceConnectionName { get; set; } = default!;

    /// <summary>
    /// Kaynak baglantinin motoru (DBMS turu) kodu.
    /// </summary>
    public string SourceEngineCode { get; set; } = default!;

    /// <summary>
    /// Kaynak baglantinin motoru (DBMS turu) adi.
    /// </summary>
    public string SourceEngineName { get; set; } = default!;

    /// <summary>
    /// Hedef (denetlenen) baglantinin adi.
    /// </summary>
    public string TargetConnectionName { get; set; } = default!;

    /// <summary>
    /// Hedef baglantinin motoru (DBMS turu) kodu.
    /// </summary>
    public string TargetEngineCode { get; set; } = default!;

    /// <summary>
    /// Hedef baglantinin motoru (DBMS turu) adi.
    /// </summary>
    public string TargetEngineName { get; set; } = default!;

    /// <summary>
    /// Karsilastirma modu (SchemaOnly/DataOnly/Both) kararli kodu.
    /// </summary>
    public string ComparisonTypeCode { get; set; } = default!;

    /// <summary>
    /// Karsilastirma modunun adi.
    /// </summary>
    public string ComparisonTypeName { get; set; } = default!;

    /// <summary>
    /// Run durumu kararli kodu.
    /// </summary>
    public string StatusCode { get; set; } = default!;

    /// <summary>
    /// Run durumunun adi.
    /// </summary>
    public string StatusName { get; set; } = default!;

    /// <summary>
    /// Motorun fiilen basladigi an.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Isin bittigi an.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Run kaydinin olusturulma zamani (tarihli gecmis ekseninin damgasi).
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Ozet blogu: toplam/kategori sayaclari + yon/nesne-turu dagilimi.
    /// </summary>
    public ComparisonReportSummaryDto Summary { get; set; } = new();

    /// <summary>
    /// Gruplama: farklarin nesne turu bazinda sayac dagilimi.
    /// </summary>
    public List<ComparisonReportGroupDto> ObjectTypeGroups { get; set; } = new();

    /// <summary>
    /// Gruplama: farklarin tablo (sema.nesne) bazinda sayac dagilimi.
    /// </summary>
    public List<ComparisonReportGroupDto> TableGroups { get; set; } = new();

    /// <summary>
    /// Detay: run'in tum bulgulari (sema/migration/veri).
    /// </summary>
    public ComparisonFindingsDto Findings { get; set; } = new();
}

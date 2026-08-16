using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Interface.Comparison;

// islevi: DBMS-ozel veri/migration okuma repository sozlesmesidir.
// sistemdeki gorevi: T7 motoru migration defteri, secili tablo yapisi/sayimi ve exact satir verisini engine component resolver ile provider'a delege eder; manager SQL veya surucu detayi bilmez.
public interface IDatabaseDataComparisonRepository : IEngineComponent
{
    // __EFMigrationsHistory defterini okur; tablo yoksa bos liste dondurur.
    Task<List<MigrationHistoryEntryModel>> ReadMigrationHistoryAsync(
        DatabaseConnectionInfo info,
        CancellationToken cancellationToken = default);

    // Secili tablolarin kesin row count degerlerini okur; tablo listesi snapshot/scope tarafindan guvenli secilmis olmalidir.
    Task<List<TableRowCountModel>> ReadRowCountsAsync(
        DatabaseConnectionInfo info,
        List<ComparisonTableIdentifierModel> tables,
        CancellationToken cancellationToken = default);

    // Secili tablo adreslerinden gercekte mevcut olanlarin kolon ve PK yapisini kataloglardan toplu okur.
    Task<List<TableDataStructureModel>> ReadTableStructuresAsync(
        DatabaseConnectionInfo info,
        List<ComparisonTableIdentifierModel> tables,
        CancellationToken cancellationToken = default);

    // Onceden dogrulanmis/limitlenmis tablo yapilarinin tum satirlarini tek batch JSON sorgusuyla okur.
    Task<List<TableDataSnapshotModel>> ReadTableDataAsync(
        DatabaseConnectionInfo info,
        List<TableDataStructureModel> tables,
        CancellationToken cancellationToken = default);

    // Katalogda dogrulanmis tablo ve anahtar kolonlariyla eslesen satirlari sinirli olarak okur.
    Task<List<TableDataRowModel>> ReadRowsByKeyAsync(
        DatabaseConnectionInfo info,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        int maxRows,
        CancellationToken cancellationToken = default)
        => Task.FromException<List<TableDataRowModel>>(new NotSupportedException());

    // Katalogda dogrulanmis tablo ve anahtar kolonlariyla eslesen kesin satir sayisini okur.
    Task<long> CountByKeyAsync(
        DatabaseConnectionInfo info,
        TableDataStructureModel structure,
        Dictionary<string, string?> keyValues,
        CancellationToken cancellationToken = default)
        => Task.FromException<long>(new NotSupportedException());
}

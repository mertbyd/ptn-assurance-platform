using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Interface.Comparison;

// islevi: DBMS-ozel kataloglardan tam sema fotografini (tablo + kolon detaylari) okuyan repository sozlesmesidir.
// sistemdeki gorevi: Manager motor secimini EngineCode ile yapar; her motor kendi katalog LINQ'unu implement eder ve motor-bagimsiz SchemaSnapshotModel dondurur. Karsilastirma motoru yalniz bu modeli gorur.
public interface IDatabaseSchemaDiscoveryRepository : IEngineComponent
{
    // Baglantidaki kullanici semalarini hafif listeler (sistem/rol semalari haric); T4 kesif akisinin ilk adimi.
    Task<List<DatabaseSchemaModel>> GetSchemasAsync(
        DatabaseConnectionInfo info,
        CancellationToken cancellationToken = default);

    // Verilen semadaki nesneleri (tablo/view/trigger/function/procedure) hafif listeler; T4 kesif akisinin ikinci adimi.
    Task<List<DatabaseSchemaObjectModel>> GetObjectsAsync(
        DatabaseConnectionInfo info,
        string schemaName,
        CancellationToken cancellationToken = default);

    // Verilen semalarin tam sema fotografini okur; her tablo kolonlariyla birlikte gelir.
    Task<SchemaSnapshotModel> ReadSnapshotAsync(
        DatabaseConnectionInfo info,
        List<string> schemaNames,
        CancellationToken cancellationToken = default);

    // Verilen semadaki tek tablonun katalog satirlarini ayni snapshot sorgu omurgasiyla hedefli okur.
    async Task<SchemaSnapshotModel> ReadSnapshotAsync(
        DatabaseConnectionInfo info,
        List<string> schemaNames,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadSnapshotAsync(info, schemaNames, cancellationToken);
        snapshot.Tables = snapshot.Tables
            .Where(table => string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return snapshot;
    }

    // Yapilandirilmis server setting katalogu destekleyen motorlarda tek izinli setting degerini okur; desteklemeyen motor null dondurur.
    Task<string?> ReadSettingAsync(
        DatabaseConnectionInfo info,
        string settingName,
        CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);
}

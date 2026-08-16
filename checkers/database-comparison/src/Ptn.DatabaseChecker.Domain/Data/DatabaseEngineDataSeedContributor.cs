using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Entities.Lookups;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Ptn.DatabaseChecker.Data;

// islevi: Sistemin destekledigi veritabani motorlarini (lookup) baslangicta idempotent olarak olusturur.
// sistemdeki gorevi: Host ilk acildiginda PostgreSql/SqlServer satirlari hazir olur; ortak akis LookupDataSeedContributor'dan gelir, burada yalnizca satirlar ve fabrika tanimli.
public class DatabaseEngineDataSeedContributor : LookupDataSeedContributor<DatabaseEngine>
{
    public DatabaseEngineDataSeedContributor(
        IRepository<DatabaseEngine, Guid> repository,
        IGuidGenerator guidGenerator)
        : base(repository, guidGenerator)
    {
    }

    // Tanimli motorlar: kod (kararli kimlik) -> insan-okur ad. Yeni motor destegi buraya bir satir.
    protected override IReadOnlyDictionary<string, string> DesiredRows => new Dictionary<string, string>
    {
        [DatabaseEngineCodes.PostgreSql] = "PostgreSQL",
        [DatabaseEngineCodes.SqlServer] = "SQL Server"
    };

    // Kod + ad ile DatabaseEngine satiri kurar; ortak alanlar LookupEntity ctor'una devredilir.
    protected override DatabaseEngine CreateEntity(Guid id, string code, string name)
        => new(id, code, name);
}

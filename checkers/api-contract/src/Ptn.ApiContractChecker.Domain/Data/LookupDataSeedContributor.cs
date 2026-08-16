using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace Ptn.ApiContractChecker.Data;

// islevi: Tum lookup tablolarinin baslangic satirlarini (Code -> Name) idempotent olarak olusturan ortak seed tabanidir.
// sistemdeki gorevi: Her yeni lookup icin ayni "mevcutlari tek sorguda cek, eksikleri tek InsertMany ile ekle" akisinin tekrar yazilmasini engeller (golden rule 1: is bir kez yapilir).
public abstract class LookupDataSeedContributor<TEntity> : IDataSeedContributor, ITransientDependency
    where TEntity : LookupEntity
{
    // Ana lookup deposu; mevcut kodlarin okunmasi ve eksiklerin yazilmasi bu depo uzerinden yurur.
    private readonly IRepository<TEntity, Guid> _repository;

    // ABP kimlik ureteci; yeni satirlarin Id'si statik uretim yerine ABP stratejisinden gelir.
    private readonly IGuidGenerator _guidGenerator;

    // Pasif seed satirlarini da idempotans hesabina katan ortak ABP veri filtresi.
    private readonly IDataFilter<IPassivable> _passivableFilter;

    protected LookupDataSeedContributor(
        IRepository<TEntity, Guid> repository,
        IGuidGenerator guidGenerator,
        IDataFilter<IPassivable> passivableFilter)
    {
        _repository = repository;
        _guidGenerator = guidGenerator;
        _passivableFilter = passivableFilter;
    }

    // Seed edilecek satirlar: kararli Code -> insan-okur Name. Concrete lookup yalnizca bu sozlugu doldurur.
    protected abstract IReadOnlyDictionary<string, string> DesiredRows { get; }

    // Concrete entity'nin public ctor'una compile-time guvenli kopru; reflection yok (golden rule 1: generic ama tip-guvenli).
    protected abstract TEntity CreateEntity(Guid id, string code, string name);

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        HashSet<string> existingCodes;
        using (_passivableFilter.Disable())
        {
            // Pasif satirlar dahil mevcut kodlari tek sorguda cek; rezerve/pasif seed ikinci calismada cogalmaz.
            existingCodes = (await _repository.GetListAsync())
                .Select(row => row.Code)
                .ToHashSet();
        }

        // Sadece eksik kodlar icin entity uret; mevcut satirlar ellenmez (admin duzenlemesi korunur).
        var rowsToInsert = DesiredRows
            .Where(row => !existingCodes.Contains(row.Key))
            .Select(row => CreateEntity(_guidGenerator.Create(), row.Key, row.Value))
            .ToList();

        // Idempotan: eksik yoksa hicbir sey yazilmaz; varsa tek InsertMany ile eklenir (dongude DB islemi yok).
        if (rowsToInsert.Count > 0)
        {
            await _repository.InsertManyAsync(rowsToInsert, autoSave: true);
        }
    }
}

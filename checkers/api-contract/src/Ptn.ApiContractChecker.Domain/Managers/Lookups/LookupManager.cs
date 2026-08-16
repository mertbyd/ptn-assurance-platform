using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.ExceptionCodes;
using Ptn.ApiContractChecker.Interface;
using Ptn.ApiContractChecker.Managers.Shared;
using Ptn.ApiContractChecker.Models.Lookups;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Lookups;

// islevi: Tum lookup tablolarinin Code benzersizligi ve pasiflestirme kurallarini tek generic manager'da isletir.
// sistemdeki gorevi: Her lookup icin create benzersizligi ve tarihceyi koruyan pasiflestirme davranisinin tekrar yazilmasini engeller.
public class LookupManager<TEntity> : BaseManager<TEntity>
    where TEntity : LookupEntity
{
    // Lookup benzersizlik ihlalinde ortak hata kodu; BaseManager benzersizlik yardimcilari bunu kullanir.
    protected override string AlreadyExistsErrorCode => LookupExceptionCodes.CodeAlreadyExists;

    public LookupManager(
        IBaseRepository<TEntity> repository,
        IAbpLazyServiceProvider abpLazyServiceProvider)
        : base(repository, abpLazyServiceProvider)
    {
    }

    // Yeni lookup satirinin Code'unun benzersiz oldugunu dogrular; kural manager'da, LINQ repository'de.
    public async Task<LookupCreateModel> ValidateCreateAsync(LookupCreateModel model)
    {
        NormalizeCreateModel(model);
        await EnsureUniqueAsync(x => x.Code == model.Code);
        return model;
    }

    // Toplu create'te once istek-ici tekrarlari, sonra tek sorguyla DB benzersizligini dogrular (dongude DB cagrisi yok).
    public async Task<List<LookupCreateModel>> ValidateCreateManyAsync(List<LookupCreateModel> models)
    {
        models.ForEach(NormalizeCreateModel);
        var codes = models.ConvertAll(m => m.Code);
        await EnsureUniqueBulkAsync(codes, x => x.Code);
        return models;
    }

    // Lookup satirini silmeden pasife ceker; Code ve ona bagli gecmis anlam degismez.
    public void Passivate(TEntity entity)
    {
        entity.IsActive = false;
    }

    // Lookup'un degisebilir alanlarini kararli Code'a dokunmadan entity davranisiyla yeniler.
    public void Update(TEntity entity, LookupUpdateModel model)
    {
        entity.Name = EntityCanonicalizer.NormalizeRequired(model.Name);
        entity.Description = EntityCanonicalizer.NormalizeOptional(model.Description);
        entity.IsActive = model.IsActive;
    }

    // Create modelini benzersizlik sorgusundan once kararli kod ve gorunen metin bicimine getirir.
    private static void NormalizeCreateModel(LookupCreateModel model)
    {
        model.Code = EntityCanonicalizer.NormalizeRequired(model.Code).ToLowerInvariant();
        model.Name = EntityCanonicalizer.NormalizeRequired(model.Name);
        model.Description = EntityCanonicalizer.NormalizeOptional(model.Description);
    }
}

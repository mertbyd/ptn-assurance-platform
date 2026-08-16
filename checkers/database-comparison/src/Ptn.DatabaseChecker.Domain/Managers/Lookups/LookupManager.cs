using System.Collections.Generic;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.ExceptionCodes;
using Ptn.DatabaseChecker.Interface;
using Ptn.DatabaseChecker.Models.Lookups;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Lookups;

// islevi: Tum lookup tablolarinin Code benzersizlik kurallarini tek generic manager'da isletir.
// sistemdeki gorevi: Her lookup icin ayni benzersizlik kodunun tekrar yazilmasini engeller; LookupCrudAppService'in create/update hook'lari bu manager'a devreder (golden rule 1: is bir kez yapilir).
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
        await EnsureUniqueAsync(x => x.Code == model.Code);
        return model;
    }

    // Toplu create'te once istek-ici tekrarlari, sonra tek sorguyla DB benzersizligini dogrular (dongude DB cagrisi yok).
    public async Task<List<LookupCreateModel>> ValidateCreateManyAsync(List<LookupCreateModel> models)
    {
        var codes = models.ConvertAll(m => m.Code);
        await EnsureUniqueBulkAsync(codes, x => x.Code);
        return models;
    }

    // Guncellemede Code degistiyse mevcut satir haric benzersizligi dogrular.
    public async Task<LookupUpdateModel> ValidateUpdateAsync(TEntity existing, LookupUpdateModel model)
    {
        if (existing.Code != model.Code)
        {
            await EnsureUniqueAsync(x => x.Code == model.Code, existing.Id);
        }

        return model;
    }
}

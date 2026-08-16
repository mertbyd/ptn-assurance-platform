using Microsoft.EntityFrameworkCore;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.EntityFrameworkCore;
using Ptn.ApiContractChecker.Interface.Snapshots;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.ApiContractChecker.Repository.Snapshots;

// islevi: Degismez spec icerigini ham hash uzerinden okur.
// sistemdeki gorevi: Icerik-adresli tekilligi tek LINQ merkezinde tutar; tenant siniri ABP filtresine birakilir.
public class SpecContentRepository : BaseRepository<SpecContent>, ISpecContentRepository
{
    public SpecContentRepository(IDbContextProvider<ApiContractCheckerDbContext> provider)
        : base(provider)
    {
    }

    // Ayni tenant icinde ayni ham hash'e sahip icerigi getirir.
    public async Task<SpecContent?> FindByRawHashAsync(string rawHash)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(content => content.RawHash == rawHash);
    }
}

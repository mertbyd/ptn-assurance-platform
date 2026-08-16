using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Entities.Connections;

namespace Ptn.DatabaseChecker.Interface.Connections;

// islevi: DatabaseConnection icin motor lookup'i dahil (Include) okuma sorgularinin kontratini tanimlar.
// sistemdeki gorevi: DTO'daki EngineCode/EngineName alanlari bu detayli okumadan beslenir (Id + Name standardi).
public interface IDatabaseConnectionRepository : IBaseRepository<DatabaseConnection>
{
    Task<DatabaseConnection?> FindWithDetailsAsync(Guid id);

    // Tek baglantiyi Engine navigation'iyla getirir; bulunamazsa null.
    Task<DatabaseConnection?> FindWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken)
        => FindWithDetailsAsync(id);

    Task<DatabaseConnection> GetWithDetailsAsync(Guid id)
        => GetAsync(id);

    // Baglantiyi Engine navigation'iyla getirir; bulunamazsa ABP standart not-found davranisini uygular.
    Task<DatabaseConnection> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken)
        => Task.FromException<DatabaseConnection>(new NotSupportedException());

    // islevi: Background execution icin baglantiyi kullanici claim'i aramadan, yalniz aktif tenant sinirinda Engine navigation'iyla getirir.
    // sistemdeki gorevi: HTTP ownership kontrolunu gevsetmeden tenant-aware worker'in snapshot FK'sini okuyabilmesini saglar.
    Task<DatabaseConnection?> FindForExecutionAsync(Guid id);

    // Baglantilari Engine navigation'iyla sayfali listeler.
    Task<List<DatabaseConnection>> GetPagedWithDetailsAsync(int skipCount, int maxResultCount);

    // Verilen baglanti kimliklerini Engine navigation'iyla tek sorguda getirir.
    Task<List<DatabaseConnection>> GetWithDetailsByIdsAsync(List<Guid> ids);

    Task<List<DatabaseConnection>> GetWithDetailsByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken)
        => GetWithDetailsByIdsAsync(ids);

    // islevi: Aktif kullanicinin tenant veya CreatorId kapsaminda erisebildigi baglantilari tek sorguda getirir.
    // sistemdeki gorevi: ComparisonDefinition FK dogrulamasinda baska kullanicinin veya tenant'in baglantisini kabul etmez.
    Task<List<DatabaseConnection>> GetAccessibleByIdsAsync(List<Guid> ids);

    // islevi: Aktif kullanicinin gorebildigi baglanti sayisini tenant/CreatorId kuraliyla hesaplar.
    // sistemdeki gorevi: Liste endpointindeki toplam sayiyi ayni gorunurluk sorgusuyla tutarli tutar.
    Task<long> GetAccessibleCountAsync();
}

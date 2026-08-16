using System;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Authoring;

namespace Ptn.TestModule.Interface.Authoring;

// islevi: Tenant-aware gecici authoring session okuma ve TTL'li yazma capability'sini tanimlar.
// sistemdeki gorevi: Domain session modelini ABP distributed cache uygulamasindan ayirir.
public interface IAuthoringSessionStore
{
    // Tenant cache anahtari altindaki session'i getirir; cache miss null'dir.
    Task<AuthoringSession?> GetAsync(Guid id, CancellationToken cancellationToken);

    // Session'i uygulamanin bildirdigi TTL secenekleriyle yazar.
    Task SetAsync(AuthoringSession session, CancellationToken cancellationToken);
}

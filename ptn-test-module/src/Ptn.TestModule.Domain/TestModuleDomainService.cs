using Volo.Abp.Domain.Services;

namespace Ptn.TestModule;

// islevi: Aggregate'i olmayan Test Module domain manager'lari icin ortak ABP tabanini tanimlar.
// sistemdeki gorevi: Somut Bridge manager'larinin dogrudan framework DomainService tabanina baglanmasini engeller.
public abstract class TestModuleDomainService : DomainService
{
}

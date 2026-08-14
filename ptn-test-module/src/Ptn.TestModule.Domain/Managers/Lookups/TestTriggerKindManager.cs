using System;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Lookups;

namespace Ptn.TestModule.Managers.Lookups;

// islevi: Tetikleme turu lookup'inin yasam dongusunu Foundation tabani uzerinden yurutur.
// sistemdeki gorevi: Kanoniklestirme, benzersizlik ve guncelleme tabandan gelir; bu tip yalniz somut entity uretimini ve hata kodunu sahiplenir.
public class TestTriggerKindManager : LookupManager<TestTriggerKind, Guid>
{
    // Cakisma hatasini lookup basina ayristirir; cagiran hangi sozlukte cakisma oldugunu mesaj metnine bakmadan anlar.
    protected override string AlreadyExistsErrorCode => TestModuleLookupErrorCodes.TriggerKindCodeAlreadyExists;

    public TestTriggerKindManager(ITestTriggerKindRepository repository)
        : base(repository)
    {
    }

    // Taban kanoniklestirmeden gecmis modeli somut entity'ye baglar; kimlik ABP ureticisinden gelir.
    protected override TestTriggerKind CreateEntity(LookupCreateModel model)
    {
        return new TestTriggerKind(GuidGenerator.Create(), model.Code, model.Name, model.Description);
    }
}

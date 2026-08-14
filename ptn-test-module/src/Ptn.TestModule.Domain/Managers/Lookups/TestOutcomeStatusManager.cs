using System;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Lookups;

namespace Ptn.TestModule.Managers.Lookups;

// islevi: Test hukmu lookup'inin yasam dongusunu Foundation tabani uzerinden yurutur.
// sistemdeki gorevi: Kanoniklestirme ve benzersizlik tabandan gelir; bu tip somut entity uretimini, build politikasi varsayimini ve hata kodunu sahiplenir.
public class TestOutcomeStatusManager : LookupManager<TestOutcomeStatus, Guid>
{
    // Cakisma hatasini lookup basina ayristirir; cagiran hangi sozlukte cakisma oldugunu mesaj metnine bakmadan anlar.
    protected override string AlreadyExistsErrorCode => TestModuleLookupErrorCodes.OutcomeStatusCodeAlreadyExists;

    public TestOutcomeStatusManager(ITestOutcomeStatusRepository repository)
        : base(repository)
    {
    }

    // Taban modelinde build politikasi alani yoktur; sonradan eklenen hukum varsayilan olarak build kirmaz (DBML default: false).
    protected override TestOutcomeStatus CreateEntity(LookupCreateModel model)
    {
        return new TestOutcomeStatus(GuidGenerator.Create(), model.Code, model.Name, breaksBuild: false, model.Description);
    }

    // Build kirma politikasini degistirir; mutasyon entity'de degil manager'da yasar.
    public virtual TestOutcomeStatus SetBuildPolicy(TestOutcomeStatus entity, bool breaksBuild)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.BreaksBuild = breaksBuild;
        return entity;
    }
}

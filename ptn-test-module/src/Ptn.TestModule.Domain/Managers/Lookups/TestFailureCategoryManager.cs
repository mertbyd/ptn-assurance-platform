using System;
using Nexum.Abp.Foundation.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Lookups;

namespace Ptn.TestModule.Managers.Lookups;

// islevi: Bulgu kategorisi lookup'inin yasam dongusunu Foundation tabani uzerinden yurutur.
// sistemdeki gorevi: Kanoniklestirme, benzersizlik ve guncelleme tabandan gelir; bu tip yalniz somut entity uretimini ve hata kodunu sahiplenir.
public class TestFailureCategoryManager : LookupManager<TestFailureCategory, Guid>
{
    // Cakisma hatasini lookup basina ayristirir; cagiran hangi sozlukte cakisma oldugunu mesaj metnine bakmadan anlar.
    protected override string AlreadyExistsErrorCode => TestModuleLookupErrorCodes.FailureCategoryCodeAlreadyExists;

    public TestFailureCategoryManager(ITestFailureCategoryRepository repository)
        : base(repository)
    {
    }

    // Taban kanoniklestirmeden gecmis modeli somut entity'ye baglar; kimlik ABP ureticisinden gelir.
    protected override TestFailureCategory CreateEntity(LookupCreateModel model)
    {
        return new TestFailureCategory(GuidGenerator.Create(), model.Code, model.Name, model.Description);
    }
}

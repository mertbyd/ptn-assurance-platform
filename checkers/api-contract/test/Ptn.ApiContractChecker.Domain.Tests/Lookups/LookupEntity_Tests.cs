using System;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Managers.Lookups;
using Ptn.ApiContractChecker.Models.Lookups;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Lookups;

// islevi: Lookup entity tabaninin kararli Code ve pasiflestirme degismezlerini dogrular.
// sistemdeki gorevi: Tum concrete lookup'lar uretilmeden once ortak tabandaki bir regresyonun bes tabloya yayilmasini engeller.
public class LookupEntity_Tests
{
    // Kararli Code'un API veya Mapperly tarafindan public setter ile degistirilememesini dogrular.
    [Fact]
    public void Code_Should_Have_An_Internal_Setter()
    {
        var codeProperty = typeof(LookupEntity).GetProperty(nameof(LookupEntity.Code));

        codeProperty.ShouldNotBeNull();
        codeProperty.SetMethod.ShouldNotBeNull();
        codeProperty.SetMethod.IsAssembly.ShouldBeTrue();
    }

    // Pasiflestirmenin satiri emekli ederken kararli kodu ve kimligi korudugunu dogrular.
    [Fact]
    public void Passivate_Should_Preserve_The_Stable_Code()
    {
        var entity = new TestLookup(Guid.NewGuid(), "stable-code", "Stable name");

        CreateManager().Passivate(entity);

        entity.IsActive.ShouldBeFalse();
        entity.Code.ShouldBe("stable-code");
    }

    // Manager'in gorunen lookup alanlarini tek kanoniklestirme davranisiyla yazdigini dogrular.
    [Fact]
    public void Update_Should_Canonicalize_Mutable_Fields()
    {
        var entity = new TestLookup(Guid.NewGuid(), "stable-code", "Old name");

        CreateManager().Update(entity, new LookupUpdateModel
        {
            Name = "  New name  ",
            Description = "   ",
            IsActive = true
        });

        entity.Name.ShouldBe("New name");
        entity.Description.ShouldBeNull();
    }

    // Lookup davranis testlerini veri erisimi kullanmadan manager uzerinden yurutur.
    private static LookupManager<TestLookup> CreateManager()
        => new(null!, null!);

    // islevi: Soyut LookupEntity tabanini saf domain testlerinde somutlastirir.
    // sistemdeki gorevi: Uretim lookup'i veya EF modeli eklemeden ortak entity davranisini test edilebilir yapar.
    private sealed class TestLookup : LookupEntity
    {
        // Test lookup'ini ortak ctor kurallariyla aktif olarak olusturur.
        public TestLookup(Guid id, string code, string name)
            : base(id, code, name)
        {
        }
    }
}

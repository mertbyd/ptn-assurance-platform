using System.Linq;
using Ptn.ApiContractChecker.Dtos.Lookups;
using Ptn.ApiContractChecker.FluentValidation.Lookups;
using Ptn.ApiContractChecker.Services.Lookups;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Lookups;

// islevi: Ortak lookup update ve servis kontratinin yalniz degisebilir alanlari ve pasiflestirmeyi acmasini dogrular.
// sistemdeki gorevi: Concrete lookup API'lari uretilirken kararli Code update'inin veya fiziksel delete'in geri gelmesini engeller.
public class LookupContract_Tests
{
    // Kaldirilan fiziksel silme metodunun reflection kontrolunde kullanilan eski kontrat adini sabitler.
    private const string RemovedDeleteMethodName = "DeleteAsync";

    // Update DTO'sunun kararli Code'u degil yalniz gorunen ve aktiflik alanlarini tasidigini dogrular.
    [Fact]
    public void Update_Dto_Should_Expose_Only_Mutable_Fields()
    {
        var propertyNames = typeof(TestLookupUpdateDto)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ToArray();

        propertyNames.ShouldBe(new[]
        {
            nameof(LookupUpdateDto.Description),
            nameof(LookupUpdateDto.IsActive),
            nameof(LookupUpdateDto.Name)
        });
    }

    // Ortak servis kontratinin fiziksel delete yerine DTO donduren pasiflestirme islemini actigini dogrular.
    [Fact]
    public void AppService_Contract_Should_Expose_Passivate_Instead_Of_Delete()
    {
        var contract = typeof(ILookupAppService<TestLookupDto, TestLookupCreateDto, TestLookupUpdateDto>);

        contract.GetMethod(nameof(ILookupAppService<TestLookupDto, TestLookupCreateDto, TestLookupUpdateDto>.PassivateAsync))
            .ShouldNotBeNull();
        contract.GetMethod(RemovedDeleteMethodName).ShouldBeNull();
    }

    // Update validator'un Code olmadan Name ve Description sinirlarini korudugunu dogrular.
    [Fact]
    public void Update_Validator_Should_Validate_The_Mutable_Fields()
    {
        var input = new TestLookupUpdateDto
        {
            Name = string.Empty,
            Description = new string('x', 513),
            IsActive = true
        };

        var result = new TestLookupUpdateDtoValidator().Validate(input);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(LookupUpdateDto.Name));
        result.Errors.ShouldContain(error => error.PropertyName == nameof(LookupUpdateDto.Description));
    }

    // islevi: Ortak lookup cevap DTO'sunu generic servis kontrati icin somutlastirir.
    // sistemdeki gorevi: Uretim lookup'i eklemeden servis metod yuzeyini reflection ile test etmeyi saglar.
    private sealed class TestLookupDto : LookupCommonDto
    {
    }

    // islevi: Ortak lookup create DTO'sunu generic servis kontrati icin somutlastirir.
    // sistemdeki gorevi: Testin uretim alanina yeni lookup kavrami eklemeden generic tipi kapatmasini saglar.
    private sealed class TestLookupCreateDto : LookupCreateDto
    {
    }

    // islevi: Kararli Code icermeyen ortak lookup update DTO'sunu test icin somutlastirir.
    // sistemdeki gorevi: Update alan seti ve validator davranisini concrete lookup olusturmadan sabitler.
    private sealed class TestLookupUpdateDto : LookupUpdateDto
    {
    }

    // islevi: Ortak lookup update validator kurallarini test DTO tipine baglar.
    // sistemdeki gorevi: Name/Description dogrulamasinin Code kaldirildiktan sonra da calistigini kanitlar.
    private sealed class TestLookupUpdateDtoValidator : LookupUpdateDtoValidator<TestLookupUpdateDto>
    {
    }
}

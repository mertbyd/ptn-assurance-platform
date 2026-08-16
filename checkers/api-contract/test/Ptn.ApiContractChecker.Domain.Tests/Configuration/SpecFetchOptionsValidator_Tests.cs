using Ptn.ApiContractChecker.Configuration;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Options;

// islevi: Spec cekim esiklerinin baslangic dogrulamasini gecerli ve gecersiz yapilandirmalarla sinar.
// sistemdeki gorevi: Yanlis appsettings degerinin ilk cekim denemesine kadar fark edilmeden kalmasini engeller.
public class SpecFetchOptionsValidator_Tests
{
    private readonly SpecFetchOptionsValidator _validator = new();

    // Varsayilan yapilandirmanin dogrulamayi gectigini kanitlar.
    [Fact]
    public void Default_Options_Should_Be_Valid()
    {
        var result = _validator.Validate(null, new SpecFetchOptions());

        result.Succeeded.ShouldBeTrue();
    }

    // Sifir veya negatif boyut sinirinin reddedildigini kanitlar.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_Positive_Size_Limit_Should_Fail(int maxContentSizeBytes)
    {
        var options = new SpecFetchOptions { MaxContentSizeBytes = maxContentSizeBytes };

        var result = _validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
    }

    // Bos liste ve bos girdi tasiyan izin listesinin reddedildigini kanitlar.
    [Fact]
    public void Unusable_Media_Type_List_Should_Fail()
    {
        _validator.Validate(null, new SpecFetchOptions { AllowedMediaTypes = [] })
            .Failed.ShouldBeTrue();

        _validator.Validate(null, new SpecFetchOptions { AllowedMediaTypes = [" "] })
            .Failed.ShouldBeTrue();
    }

    // Kalici MediaType kolonuna sigmayan medya tipinin reddedildigini kanitlar.
    [Fact]
    public void Media_Type_Longer_Than_Persisted_Column_Should_Fail()
    {
        var tooLongMediaType = new string('a', SpecContentConsts.MaxMediaTypeLength + 1);
        var options = new SpecFetchOptions { AllowedMediaTypes = [tooLongMediaType] };

        var result = _validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
    }
}

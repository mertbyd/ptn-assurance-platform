using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.FluentValidation.Sources;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Sources;

// islevi: SpecSource girdi sekli ve all-or-nothing credential cifti kurallarini dogrular.
// sistemdeki gorevi: Tek basina HeaderName veya HeaderValue'nun 200 donup sessizce kaybolmasini engelleyen kontrat testidir.
public class SpecSourceValidator_Tests
{
    // Yalniz HeaderName gelen update isteginin reddedildigini kanitlar.
    [Fact]
    public void Update_Should_Reject_HeaderName_Without_HeaderValue()
    {
        var input = BuildUpdateInput();
        input.HeaderName = "Authorization";

        var result = new UpdateSpecSourceDtoValidator().Validate(input);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateSpecSourceDto.HeaderValue));
    }

    // Yalniz HeaderValue gelen create isteginin reddedildigini kanitlar.
    [Fact]
    public void Create_Should_Reject_HeaderValue_Without_HeaderName()
    {
        var input = BuildCreateInput();
        input.HeaderValue = "Bearer test-value";

        var result = new CreateSpecSourceDtoValidator().Validate(input);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateSpecSourceDto.HeaderName));
    }

    // Iki credential alani da yoksa update'in mevcut secret'i korumak uzere kabul edildigini kanitlar.
    [Fact]
    public void Update_Should_Accept_Missing_Credential_Pair()
    {
        var result = new UpdateSpecSourceDtoValidator().Validate(BuildUpdateInput());

        result.IsValid.ShouldBeTrue();
    }

    // Dokuman adlarinin buyuk-kucuk harf farkiyla tekrar edemeyecegini kanitlar.
    [Fact]
    public void Create_Should_Reject_Duplicate_Document_Names()
    {
        var input = BuildCreateInput();
        input.Documents.Add(new SpecDocumentDto
        {
            DocumentName = "V1",
            Path = "/openapi/v2.json"
        });

        var result = new CreateSpecSourceDtoValidator().Validate(input);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateSpecSourceDto.Documents));
    }

    // Gecerli create istegini testler boyunca tek yerde kurar.
    private static CreateSpecSourceDto BuildCreateInput()
    {
        return new CreateSpecSourceDto
        {
            Name = "orders",
            BaseUrl = "https://orders.test",
            Documents =
            [
                new SpecDocumentDto
                {
                    DocumentName = "v1",
                    Path = "/openapi/v1.json"
                }
            ]
        };
    }

    // Gecerli update istegini create sozlesmesinden turetir.
    private static UpdateSpecSourceDto BuildUpdateInput()
    {
        return new UpdateSpecSourceDto
        {
            Name = "orders",
            BaseUrl = "https://orders.test",
            Documents =
            [
                new SpecDocumentDto
                {
                    Id = Guid.NewGuid(),
                    DocumentName = "v1",
                    Path = "/openapi/v1.json"
                }
            ]
        };
    }
}

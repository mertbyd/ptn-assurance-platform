using FluentValidation;
using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.Dtos.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;

namespace Ptn.ApiContractChecker.FluentValidation.Sources;

// islevi: SpecSource create/update isteklerinin ortak alan, dokuman ve credential-cifti kurallarini tanimlar.
// sistemdeki gorevi: Ayni sinir dogrulamasinin iki validator'da ayrisip HeaderName veya HeaderValue'yu sessizce yutmasini engeller.
public abstract class SpecSourceDtoValidatorBase<TDto> : AbstractValidator<TDto>
    where TDto : CreateSpecSourceDto
{
    protected SpecSourceDtoValidatorBase()
    {
        // Kaynak adi kalici kolon siniriyla ayni zorunluluk ve uzunlugu tasir.
        RuleFor(source => source.Name)
            .NotEmpty().WithErrorCode(SpecSourceExceptionCodes.Validation.NameRequired)
            .MaximumLength(SpecSourceConsts.MaxNameLength).WithErrorCode(SpecSourceExceptionCodes.Validation.NameMaxLength);

        // Servis koku mutlak HTTP(S) adresi olmak zorundadir.
        RuleFor(source => source.BaseUrl)
            .NotEmpty().WithErrorCode(SpecSourceExceptionCodes.Validation.BaseUrlRequired)
            .MaximumLength(SpecSourceConsts.MaxBaseUrlLength).WithErrorCode(SpecSourceExceptionCodes.Validation.BaseUrlMaxLength)
            .Must(BeAbsoluteHttpUrl).WithErrorCode(SpecSourceExceptionCodes.Validation.BaseUrlInvalid);

        // Bir kaynak en az bir dokuman tanimiyla anlamlidir ve adlar istek icinde tekildir.
        RuleFor(source => source.Documents)
            .NotEmpty().WithErrorCode(SpecSourceExceptionCodes.Validation.DocumentsRequired)
            .Must(HaveUniqueDocumentNames).WithErrorCode(SpecSourceExceptionCodes.Validation.DocumentNameDuplicate);

        // Aggregate cocuklarinin alan sekli her kaynak mutasyonunda ayni kurallara uyar.
        RuleForEach(source => source.Documents).ChildRules(document =>
        {
            document.RuleFor(item => item.DocumentName)
                .NotEmpty().WithErrorCode(SpecSourceExceptionCodes.Validation.DocumentNameRequired)
                .MaximumLength(SpecDocumentConsts.MaxDocumentNameLength).WithErrorCode(SpecSourceExceptionCodes.Validation.DocumentNameMaxLength);
            document.RuleFor(item => item.Path)
                .NotEmpty().WithErrorCode(SpecSourceExceptionCodes.Validation.DocumentPathRequired)
                .MaximumLength(SpecDocumentConsts.MaxPathLength).WithErrorCode(SpecSourceExceptionCodes.Validation.DocumentPathMaxLength)
                .Must(BeRelativePath).WithErrorCode(SpecSourceExceptionCodes.Validation.DocumentPathInvalid);
        });

        // HeaderName yalniz HeaderValue ile birlikte kabul edilir ve HTTP token biciminde olmalidir.
        RuleFor(source => source.HeaderName)
            .NotEmpty().WithErrorCode(SpecSourceExceptionCodes.Validation.HeaderNameRequired)
            .When(source => HasValue(source.HeaderValue));
        RuleFor(source => source.HeaderName)
            .MaximumLength(SpecSourceConsts.MaxHeaderNameLength).WithErrorCode(SpecSourceExceptionCodes.Validation.HeaderNameMaxLength)
            .Matches(SpecSourceConsts.HeaderNamePattern).WithErrorCode(SpecSourceExceptionCodes.Validation.HeaderNameInvalid)
            .When(source => HasValue(source.HeaderName));

        // HeaderValue yalniz HeaderName ile birlikte kabul edilir ve satir-bolme karakteri tasiyamaz.
        RuleFor(source => source.HeaderValue)
            .NotEmpty().WithErrorCode(SpecSourceExceptionCodes.Validation.HeaderValueRequired)
            .When(source => HasValue(source.HeaderName));
        RuleFor(source => source.HeaderValue)
            .MaximumLength(SpecSourceConsts.MaxHeaderValueLength).WithErrorCode(SpecSourceExceptionCodes.Validation.HeaderValueMaxLength)
            .Must(NotContainLineBreak).WithErrorCode(SpecSourceExceptionCodes.Validation.HeaderValueInvalid)
            .When(source => HasValue(source.HeaderValue));
    }

    // Verilen metnin bosluk disinda icerik tasiyip tasimadigini belirler.
    private static bool HasValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    // Servis kokunun mutlak HTTP veya HTTPS adresi oldugunu dogrular.
    private static bool BeAbsoluteHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    // Dokuman yolunun kaynak taban adresinden bagimsiz mutlak HTTP(S) URL olmamasini saglar.
    private static bool BeRelativePath(string value)
    {
        return !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
               (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps);
    }

    // Kaynak icindeki dokuman adlarini buyuk-kucuk harf duyarsiz tekil tutar.
    private static bool HaveUniqueDocumentNames(List<SpecDocumentDto> documents)
    {
        return documents
            .Select(document => document.DocumentName?.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() == documents.Count;
    }

    // Kimlik baslik degerinin HTTP header enjeksiyonuna yol acan satir sonlarini tasimamasini saglar.
    private static bool NotContainLineBreak(string? value)
    {
        return value == null || (!value.Contains('\r') && !value.Contains('\n'));
    }
}

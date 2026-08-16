using Microsoft.Extensions.Options;
using Ptn.ApiContractChecker.Constants.Snapshots;

namespace Ptn.ApiContractChecker.Configuration;

// islevi: Spec cekim esiklerinin uygulama ayaga kalkarken anlamli ve kalici modele sigar oldugunu dogrular.
// sistemdeki gorevi: Yanlis yapilandirmayi ilk istegi beklemeden durdurur; options sinifini veri tasiyici olarak birakir.
public class SpecFetchOptionsValidator : IValidateOptions<SpecFetchOptions>
{
    private const string InvalidMaxContentSize = "SpecFetch:MaxContentSizeBytes must be greater than zero.";
    private const string InvalidAllowedMediaTypes =
        "SpecFetch:AllowedMediaTypes must contain at least one non-empty value that fits the persisted media type length.";

    // Iki esigi de ayni calistirmada denetler ve bulunan tum ihlalleri birlikte bildirir.
    public ValidateOptionsResult Validate(string? name, SpecFetchOptions options)
    {
        var failures = new List<string>();

        if (options.MaxContentSizeBytes <= 0)
        {
            failures.Add(InvalidMaxContentSize);
        }

        if (!HasUsableMediaTypes(options))
        {
            failures.Add(InvalidAllowedMediaTypes);
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    // Izin listesinin bos olmadigini ve her girdinin SpecContent.MediaType kolonuna sigdigini bildirir.
    private static bool HasUsableMediaTypes(SpecFetchOptions options)
    {
        return options.AllowedMediaTypes is { Length: > 0 } &&
               options.AllowedMediaTypes.All(mediaType =>
                   !string.IsNullOrWhiteSpace(mediaType) &&
                   mediaType.Length <= SpecContentConsts.MaxMediaTypeLength);
    }
}

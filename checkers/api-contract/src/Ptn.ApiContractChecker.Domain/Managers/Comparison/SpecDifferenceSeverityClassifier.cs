using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Models.Comparison;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Ptn.ApiContractChecker.Managers.Comparison;

// islevi: Siddetlendirilmemis spec farkini tur ve yon asimetrisine gore kapali katalogdan siniflandirir.
// sistemdeki gorevi: RULE-0007 kiricilik kararlarini diff ureticilerinden ve run orkestrasyonundan ayiran tek karar noktadir.
public class SpecDifferenceSeverityClassifier : ITransientDependency
{
    // Fark turunun izin verdigi yonu dogrulayip kararli siddet kodunu dondurur.
    public string Classify(SpecDifferenceModel difference)
    {
        Check.NotNull(difference, nameof(difference));

        return difference.KindCode switch
        {
            DifferenceKindCodes.NewRequiredRequestProperty or
                DifferenceKindCodes.RequestPropertyBecameRequired or
                DifferenceKindCodes.RequestParameterEnumValueRemoved or
                DifferenceKindCodes.RequestBodyBecameRequired => ClassifyRequestNarrowing(difference),
            DifferenceKindCodes.RequestPropertyTypeChanged => ClassifyPropertyTypeChange(difference),
            DifferenceKindCodes.ResponsePropertyBecameOptional or
                DifferenceKindCodes.ResponsePropertyBecameNullable or
                DifferenceKindCodes.ResponseSuccessStatusRemoved or
                DifferenceKindCodes.ResponseMediaTypeRemoved or
                DifferenceKindCodes.RequiredResponseHeaderRemoved => ClassifyResponseLoosening(difference),
            DifferenceKindCodes.EndpointAdded or
                DifferenceKindCodes.EndpointRemoved => ClassifyEndpointChange(difference),
            DifferenceKindCodes.SchemaAdded or
                DifferenceKindCodes.SchemaRemoved or
                DifferenceKindCodes.SchemaRenamed => ClassifySchemaChange(difference),
            DifferenceKindCodes.DescriptionChanged => ClassifyDocumentationChange(difference),
            _ => throw new ArgumentOutOfRangeException(nameof(difference))
        };
    }

    // Istemcinin gondermesi gereken sozlesmeyi daraltan request farklarini breaking yapar.
    private static string ClassifyRequestNarrowing(SpecDifferenceModel difference)
    {
        EnsureDirection(difference.DirectionCode, DifferenceDirectionCodes.Request);
        return DifferenceSeverityCodes.Breaking;
    }

    // Tip degisikliginin request ve response taraflarinda da breaking oldugunu uygular.
    private static string ClassifyPropertyTypeChange(SpecDifferenceModel difference)
    {
        EnsureDirection(
            difference.DirectionCode,
            DifferenceDirectionCodes.Request,
            DifferenceDirectionCodes.Response);
        return DifferenceSeverityCodes.Breaking;
    }

    // Sunucunun donus garantisini gevseten response farklarini breaking yapar.
    private static string ClassifyResponseLoosening(SpecDifferenceModel difference)
    {
        EnsureDirection(difference.DirectionCode, DifferenceDirectionCodes.Response);
        return DifferenceSeverityCodes.Breaking;
    }

    // Endpoint ekleme ve silme siddetlerini endpoint yonu altinda merkezilestirir.
    private static string ClassifyEndpointChange(SpecDifferenceModel difference)
    {
        EnsureDirection(difference.DirectionCode, DifferenceDirectionCodes.Endpoint);
        return difference.KindCode switch
        {
            DifferenceKindCodes.EndpointAdded => DifferenceSeverityCodes.NonBreaking,
            DifferenceKindCodes.EndpointRemoved => DifferenceSeverityCodes.Breaking,
            _ => throw new ArgumentOutOfRangeException(nameof(difference))
        };
    }

    // Erisilebilir sema ekleme, silme ve yeniden adlandirma kararlarini iki kullanma yonu icin uygular.
    private static string ClassifySchemaChange(SpecDifferenceModel difference)
    {
        EnsureDirection(
            difference.DirectionCode,
            DifferenceDirectionCodes.Request,
            DifferenceDirectionCodes.Response);
        return difference.KindCode switch
        {
            DifferenceKindCodes.SchemaAdded => DifferenceSeverityCodes.NonBreaking,
            DifferenceKindCodes.SchemaRemoved or
                DifferenceKindCodes.SchemaRenamed => DifferenceSeverityCodes.Breaking,
            _ => throw new ArgumentOutOfRangeException(nameof(difference))
        };
    }

    // Davranissal sozlesmeye dokunmayan metin farklarini yalniz dokumantasyon olarak isaretler.
    private static string ClassifyDocumentationChange(SpecDifferenceModel difference)
    {
        EnsureDirection(difference.DirectionCode, DifferenceDirectionCodes.Documentation);
        return DifferenceSeverityCodes.DocsOnly;
    }

    // Ture ait kapali yon kumesi disindaki ara model kombinasyonlarini reddeder.
    private static void EnsureDirection(string directionCode, params string[] allowedDirectionCodes)
    {
        if (!allowedDirectionCodes.Contains(directionCode, StringComparer.Ordinal))
        {
            throw new ArgumentOutOfRangeException(nameof(directionCode));
        }
    }
}

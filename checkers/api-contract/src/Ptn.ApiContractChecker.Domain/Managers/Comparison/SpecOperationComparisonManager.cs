using Ptn.ApiContractChecker.Constants.Differences;
using Ptn.ApiContractChecker.Constants.Differences.Lookups;
using Ptn.ApiContractChecker.Constants.Snapshots;
using Ptn.ApiContractChecker.Models.Comparison;
using Ptn.ApiContractChecker.Models.Runs;
using Ptn.ApiContractChecker.Models.Snapshots;
using Volo.Abp.Domain.Services;

namespace Ptn.ApiContractChecker.Managers.Comparison;

// islevi: Iki normalize snapshot'in endpoint, istek, yanit ve dokumantasyon yuzeyini karsilastirir.
// sistemdeki gorevi: KBP-611 operasyon kurallarini I/O ve siddet siniflandirmasi olmadan deterministik fark listesine indirger.
public class SpecOperationComparisonManager : DomainService
{
    // Koleksiyon kimligi ve esleme yonlerini tum operasyon alt yuzeylerinde tek merkezde tutan comparer.
    private readonly SpecCollectionComparer _collectionComparer;

    // Operasyon manager'ini ortak koleksiyon esleyicisiyle kurar.
    public SpecOperationComparisonManager(SpecCollectionComparer collectionComparer)
    {
        _collectionComparer = collectionComparer;
    }

    // Normalize base ve target snapshot'larin operasyon farklarini kararli sirada uretir.
    public List<SpecDifferenceModel> Compare(
        SpecSnapshotModel baseSnapshot,
        SpecSnapshotModel targetSnapshot)
    {
        var differences = CompareEndpoints(baseSnapshot.Operations, targetSnapshot.Operations);
        differences.AddRange(CompareSharedOperations(baseSnapshot.Operations, targetSnapshot.Operations));
        differences.AddRange(CompareDocumentation(baseSnapshot.Documentation, targetSnapshot.Documentation));
        return _collectionComparer.SortDifferences(differences);
    }

    // Operasyon kimligine gore eklenen ve kaldirilan endpoint'leri karsilastirir.
    private List<SpecDifferenceModel> CompareEndpoints(
        IEnumerable<SpecOperationModel> sourceOperations,
        IEnumerable<SpecOperationModel> targetOperations)
        => _collectionComparer.Compare(
            sourceOperations,
            targetOperations,
            BuildOperationKey,
            BuildOperationAddress,
            BuildOperationValue,
            BuildOperationValue,
            (source, target) => (BuildOperationValue(source), BuildOperationValue(target)),
            DifferenceKindCodes.EndpointRemoved,
            DifferenceKindCodes.EndpointAdded,
            null,
            DifferenceDirectionCodes.Endpoint);

    // Ayni operasyon kimligindeki istek ve yanit alt yuzeylerini karsilastirir.
    private List<SpecDifferenceModel> CompareSharedOperations(
        IEnumerable<SpecOperationModel> sourceOperations,
        IEnumerable<SpecOperationModel> targetOperations)
    {
        var differences = new List<SpecDifferenceModel>();
        foreach (var (sourceOperation, targetOperation) in _collectionComparer.EnumerateMatched(
                     sourceOperations,
                     targetOperations,
                     BuildOperationKey))
        {
            differences.AddRange(CompareParameterEnums(sourceOperation, targetOperation));
            differences.AddRange(CompareRequestBodies(sourceOperation, targetOperation));
            differences.AddRange(CompareResponseStatuses(sourceOperation, targetOperation));
            differences.AddRange(CompareResponseMediaTypes(sourceOperation, targetOperation));
            differences.AddRange(CompareResponseHeaders(sourceOperation, targetOperation));
        }

        return differences;
    }

    // Ayni ad ve konumdaki parametrelerden hedefte kaldirilan enum degerlerini uretir.
    private List<SpecDifferenceModel> CompareParameterEnums(
        SpecOperationModel sourceOperation,
        SpecOperationModel targetOperation)
    {
        var differences = new List<SpecDifferenceModel>();
        foreach (var (sourceParameter, targetParameter) in _collectionComparer.EnumerateMatched(
                     sourceOperation.Parameters,
                     targetOperation.Parameters,
                     BuildParameterKey))
        {
            differences.AddRange(_collectionComparer.Compare(
                sourceParameter.EnumValues,
                targetParameter.EnumValues,
                enumValue => enumValue,
                _ => BuildParameterAddress(sourceOperation, sourceParameter),
                enumValue => enumValue,
                enumValue => enumValue,
                (source, target) => (source, target),
                DifferenceKindCodes.RequestParameterEnumValueRemoved,
                null,
                null,
                DifferenceDirectionCodes.Request));
        }

        return differences;
    }

    // Ayni medya tipindeki request body'nin optional durumdan required duruma gecisini uretir.
    private List<SpecDifferenceModel> CompareRequestBodies(
        SpecOperationModel sourceOperation,
        SpecOperationModel targetOperation)
    {
        var differences = new List<SpecDifferenceModel>();
        foreach (var (sourceBody, targetBody) in _collectionComparer.EnumerateMatched(
                     sourceOperation.RequestBodies,
                     targetOperation.RequestBodies,
                     body => body.MediaType))
        {
            if (sourceBody.Required || !targetBody.Required)
            {
                continue;
            }

            differences.Add(SpecDifferenceFactory.Modified(
                DifferenceKindCodes.RequestBodyBecameRequired,
                DifferenceDirectionCodes.Request,
                BuildRequestBodyAddress(sourceOperation, sourceBody),
                SpecComparisonTextConstants.Optional,
                SpecComparisonTextConstants.Required));
        }

        return differences;
    }

    // Base'te olup target'ta bulunmayan basarili yanit durum kodlarini uretir.
    private List<SpecDifferenceModel> CompareResponseStatuses(
        SpecOperationModel sourceOperation,
        SpecOperationModel targetOperation)
    {
        var sourceStatuses = sourceOperation.Responses
            .Select(response => response.StatusCode)
            .Where(IsSuccessStatus)
            .Distinct(StringComparer.Ordinal);
        var targetStatuses = targetOperation.Responses
            .Select(response => response.StatusCode)
            .Where(IsSuccessStatus)
            .Distinct(StringComparer.Ordinal);

        return _collectionComparer.Compare(
            sourceStatuses,
            targetStatuses,
            status => status,
            status => BuildResponseAddress(sourceOperation, status),
            status => status,
            status => status,
            (source, target) => (source, target),
            DifferenceKindCodes.ResponseSuccessStatusRemoved,
            null,
            null,
            DifferenceDirectionCodes.Response);
    }

    // Iki tarafta kalan durum kodlarindan hedefte kaldirilan response medya tiplerini uretir.
    private List<SpecDifferenceModel> CompareResponseMediaTypes(
        SpecOperationModel sourceOperation,
        SpecOperationModel targetOperation)
    {
        var differences = new List<SpecDifferenceModel>();
        var sourceStatuses = sourceOperation.Responses
            .Select(response => response.StatusCode)
            .Distinct(StringComparer.Ordinal);
        var targetStatuses = targetOperation.Responses
            .Select(response => response.StatusCode)
            .Distinct(StringComparer.Ordinal);

        foreach (var status in sourceStatuses.Intersect(targetStatuses, StringComparer.Ordinal))
        {
            var sourceMediaTypes = sourceOperation.Responses
                .Where(response => response.StatusCode == status && response.MediaType.Length > 0)
                .Select(response => response.MediaType);
            var targetMediaTypes = targetOperation.Responses
                .Where(response => response.StatusCode == status && response.MediaType.Length > 0)
                .Select(response => response.MediaType);

            differences.AddRange(_collectionComparer.Compare(
                sourceMediaTypes,
                targetMediaTypes,
                mediaType => mediaType,
                mediaType => BuildResponseAddress(sourceOperation, status, mediaType),
                mediaType => mediaType,
                mediaType => mediaType,
                (source, target) => (source, target),
                DifferenceKindCodes.ResponseMediaTypeRemoved,
                null,
                null,
                DifferenceDirectionCodes.Response));
        }

        return differences;
    }

    // Ayni status ve medya tipindeki target yanitindan kaldirilan zorunlu header'lari uretir.
    private List<SpecDifferenceModel> CompareResponseHeaders(
        SpecOperationModel sourceOperation,
        SpecOperationModel targetOperation)
    {
        var differences = new List<SpecDifferenceModel>();
        foreach (var (sourceResponse, targetResponse) in _collectionComparer.EnumerateMatched(
                     sourceOperation.Responses,
                     targetOperation.Responses,
                     BuildResponseKey))
        {
            differences.AddRange(_collectionComparer.Compare(
                sourceResponse.Headers.Where(header => header.Required),
                targetResponse.Headers,
                header => header.Name,
                _ => BuildResponseAddress(
                    sourceOperation,
                    sourceResponse.StatusCode,
                    sourceResponse.MediaType),
                header => header.Name,
                header => header.Name,
                (source, target) => (source.Name, target.Name),
                DifferenceKindCodes.RequiredResponseHeaderRemoved,
                null,
                null,
                DifferenceDirectionCodes.Response));
        }

        return differences;
    }

    // Iki tarafta da duran dokumantasyon hedeflerinin summary, description ve example farklarini uretir.
    // Yalniz tek tarafta olan hedef icin bulgu URETILMEZ: o hedefin yoklugu zaten endpoint-removed,
    // response-success-status-removed veya response-media-type-removed olarak raporlanir; ikinci bir
    // docs-only satiri raporu ve degisiklik gunlugu mailini var olmayan adreslerle sisirirdi.
    private List<SpecDifferenceModel> CompareDocumentation(
        IEnumerable<SpecDocumentationModel> sourceDocumentation,
        IEnumerable<SpecDocumentationModel> targetDocumentation)
        => _collectionComparer.Compare(
            sourceDocumentation.Where(IsOperationDocumentation),
            targetDocumentation.Where(IsOperationDocumentation),
            BuildDocumentationKey,
            BuildDocumentationAddress,
            BuildDocumentationDefinition,
            BuildDocumentationDefinition,
            BuildDocumentationChangeSummary,
            null,
            null,
            DifferenceKindCodes.DescriptionChanged,
            DifferenceDirectionCodes.Documentation);

    // OperationId varsa onu, yoksa HTTP metodu ve normalize path'i operasyon kimligi yapar.
    private static string BuildOperationKey(SpecOperationModel operation)
    {
        var keyPrefix = operation.OperationId is null
            ? SpecComparisonTextConstants.MethodPathKeyPrefix
            : SpecComparisonTextConstants.OperationIdKeyPrefix;
        var identity = operation.OperationId ?? BuildOperationValue(operation);
        return string.Join(
            SpecNormalizationTextConstants.Normalization.TypeSeparator,
            keyPrefix,
            identity);
    }

    // Parametreyi ad ve konum ciftiyle eslestirir.
    private static string BuildParameterKey(SpecParameterModel parameter)
        => string.Join(
            SpecNormalizationTextConstants.Normalization.TypeSeparator,
            parameter.Name,
            parameter.In);

    // Yaniti durum kodu ve medya tipi ciftiyle eslestirir.
    private static string BuildResponseKey(SpecResponseModel response)
        => string.Join(
            SpecNormalizationTextConstants.Normalization.TypeSeparator,
            response.StatusCode,
            response.MediaType);

    // Dokumantasyon kaydini hedef turu ve normalize hedef adresiyle eslestirir.
    private static string BuildDocumentationKey(SpecDocumentationModel documentation)
        => string.Join(
            SpecNormalizationTextConstants.Normalization.TypeSeparator,
            documentation.TargetKind,
            documentation.Target);

    // Endpoint farki icin insan-okur HTTP metodu ve path degerini kurar.
    private static string BuildOperationValue(SpecOperationModel operation)
        => string.Concat(
            operation.Method,
            SpecNormalizationTextConstants.Normalization.SingleSpace,
            operation.Path);

    // Endpoint fark adresini operasyon kimligi, metod ve path ile kurar.
    private static FindingAddress BuildOperationAddress(SpecOperationModel operation)
        => new(
            operationId: operation.OperationId,
            httpMethod: operation.Method,
            path: operation.Path);

    // Parametre fark adresine operasyon ve parametre kimligini yerlestirir.
    private static FindingAddress BuildParameterAddress(
        SpecOperationModel operation,
        SpecParameterModel parameter)
        => new(
            operationId: operation.OperationId,
            httpMethod: operation.Method,
            path: operation.Path,
            parameterName: parameter.Name);

    // Request body fark adresine operasyon ve medya tipi kimligini yerlestirir.
    private static FindingAddress BuildRequestBodyAddress(
        SpecOperationModel operation,
        SpecRequestBodyModel body)
        => new(
            operationId: operation.OperationId,
            httpMethod: operation.Method,
            path: operation.Path,
            schemaName: body.SchemaReferenceId,
            mediaType: body.MediaType);

    // Response fark adresine operasyon, durum ve varsa medya tipi kimligini yerlestirir.
    private static FindingAddress BuildResponseAddress(
        SpecOperationModel operation,
        string status,
        string? mediaType = null)
        => new(
            operationId: operation.OperationId,
            httpMethod: operation.Method,
            path: operation.Path,
            responseStatus: status,
            mediaType: mediaType);

    // Dokumantasyon hedef metnini FindingAddress alanlarina geri ayirir.
    private static FindingAddress BuildDocumentationAddress(SpecDocumentationModel documentation)
    {
        var targetParts = documentation.Target.Split(
            SpecNormalizationTextConstants.Normalization.TypeSeparator,
            StringSplitOptions.None);
        var operationParts = SplitOperationTarget(targetParts[0]);
        var parameterName = documentation.TargetKind == SpecNormalizationTextConstants.DocumentationTargets.Parameter &&
                            targetParts.Length > 2
            ? targetParts[2]
            : null;
        var responseStatus = documentation.TargetKind is
                                 SpecNormalizationTextConstants.DocumentationTargets.Response or
                                 SpecNormalizationTextConstants.DocumentationTargets.Header &&
                             targetParts.Length > 1
            ? targetParts[1]
            : null;
        var mediaType = documentation.TargetKind switch
        {
            SpecNormalizationTextConstants.DocumentationTargets.RequestBody when targetParts.Length > 1 => targetParts[1],
            SpecNormalizationTextConstants.DocumentationTargets.Response when targetParts.Length > 2 => targetParts[2],
            SpecNormalizationTextConstants.DocumentationTargets.Header when targetParts.Length > 2 => targetParts[2],
            _ => null
        };

        return new FindingAddress(
            httpMethod: operationParts.Method,
            path: operationParts.Path,
            parameterName: parameterName,
            responseStatus: responseStatus,
            mediaType: mediaType);
    }

    // Operasyon hedefinin ilk boslugundan HTTP metodu ve path parcalarini ayirir.
    private static (string Method, string Path) SplitOperationTarget(string target)
    {
        var separatorIndex = target.IndexOf(' ');
        return separatorIndex < 0
            ? (target, string.Empty)
            : (target[..separatorIndex], target[(separatorIndex + 1)..]);
    }

    // Dokumantasyonun tum metin alanlarini sirali tek karsilastirma tanimina cevirir.
    private static string BuildDocumentationDefinition(SpecDocumentationModel documentation)
        => string.Join(
            SpecNormalizationTextConstants.Normalization.TypeSeparator,
            documentation.Summary,
            documentation.Description,
            documentation.Example);

    // Degisen ilk dokumantasyon alaninin once ve sonra degerini rapor ozeti yapar.
    private static (string? OldValue, string? NewValue) BuildDocumentationChangeSummary(
        SpecDocumentationModel source,
        SpecDocumentationModel target)
    {
        if (!string.Equals(source.Description, target.Description, StringComparison.Ordinal))
        {
            return (source.Description, target.Description);
        }

        if (!string.Equals(source.Summary, target.Summary, StringComparison.Ordinal))
        {
            return (source.Summary, target.Summary);
        }

        return (source.Example, target.Example);
    }

    // Dokumantasyon kaydinin sema yerine operasyon yuzeyine ait olup olmadigini belirler.
    private static bool IsOperationDocumentation(SpecDocumentationModel documentation)
        => documentation.TargetKind is
            SpecNormalizationTextConstants.DocumentationTargets.Operation or
            SpecNormalizationTextConstants.DocumentationTargets.Parameter or
            SpecNormalizationTextConstants.DocumentationTargets.RequestBody or
            SpecNormalizationTextConstants.DocumentationTargets.Response or
            SpecNormalizationTextConstants.DocumentationTargets.Header;

    // HTTP kodunun sayisal 2xx veya OpenAPI 2XX wildcard basari kodu olup olmadigini belirler.
    private static bool IsSuccessStatus(string status)
        => int.TryParse(status, out var numericStatus)
            ? numericStatus is >= 200 and <= 299
            : string.Equals(
                status,
                SpecComparisonTextConstants.SuccessStatusWildcard,
                StringComparison.OrdinalIgnoreCase);

}

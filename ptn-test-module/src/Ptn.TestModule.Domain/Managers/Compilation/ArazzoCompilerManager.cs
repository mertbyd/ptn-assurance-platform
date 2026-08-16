using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Constants;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Comparison.Assertions;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.Constants.Compilation;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.ExceptionCodes.Compilation;
using Ptn.TestModule.Interface.Compilation;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Database;
using Ptn.TestModule.Models.Authoring;
using Ptn.TestModule.Models.Compilation;
using Volo.Abp;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Ptn.TestModule.Managers.Compilation;

// islevi: x-checknexus-db adimlarini profil baglariyla gercek Database Checker HTTP adimlarina derler.
// sistemdeki gorevi: Arazzo 1.0.1, XPath yasagi, deterministik cikti ve lint kapisinin tek domain sahibidir.
public class ArazzoCompilerManager : TestModuleDomainService
{
    private readonly ProfilePackManager _profilePackManager;
    private readonly IArazzoDocumentLinter _documentLinter;

    // Profil cozumu ile resmi lint capability'sini derleme use-case'ine baglar.
    public ArazzoCompilerManager(
        ProfilePackManager profilePackManager,
        IArazzoDocumentLinter documentLinter)
    {
        _profilePackManager = profilePackManager;
        _documentLinter = documentLinter;
    }

    // Kaynak belgeyi derler, hash'ler ve pinli Redocly surecinde sema kanitini toplar.
    public async Task<ArazzoCompilationResult> CompileAsync(
        string sourceDocument,
        ProfilePack profilePack,
        Guid specSnapshotId,
        CancellationToken cancellationToken = default)
    {
        var result = Compile(sourceDocument, profilePack, specSnapshotId);
        var lintResult = await _documentLinter.LintAsync(result.CompiledDocument, cancellationToken);
        result.IsSchemaValid = lintResult.IsValid;
        result.LintDiagnostics = lintResult.Diagnostics;
        return result;
    }

    // Cache oturumundaki tipli adimlardan tam Arazzo 1.0.1 kaynak belgesini mekanik olarak uretir.
    public string BuildAuthoringDocument(AuthoringSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var workflow = new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.WorkflowId, session.WorkflowId },
            { ArazzoCompilationConsts.Fields.Summary, session.WorkflowSummary },
            { ArazzoCompilationConsts.Fields.Steps, new YamlSequenceNode(
                session.Steps.Select(BuildAuthoringStep).Concat(
                    session.DatabaseSteps.Select(BuildAuthoringDatabaseStep))) }
        };
        var root = new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.Arazzo, ArazzoCompilationConsts.TargetVersion },
            { ArazzoCompilationConsts.Fields.Info, new YamlMappingNode
                {
                    { ArazzoCompilationConsts.Fields.Title, session.WorkflowSummary },
                    { ArazzoCompilationConsts.Fields.Version, ArazzoCompilationConsts.TargetVersion }
                }
            },
            { ArazzoCompilationConsts.Fields.SourceDescriptions, BuildAuthoringSources(session) },
            { ArazzoCompilationConsts.Fields.Workflows, new YamlSequenceNode(workflow) }
        };
        return Serialize(root);
    }

    // Derleme ciktisini ve iki turetilebilirlik yuzeyinin cevabini tek yayin kanitina indirger.
    public ScenarioCompilationEvidence CreateEvidence(
        ArazzoCompilationResult compilation,
        DerivabilityResult? apiDerivability,
        DatabaseDerivabilityResult? databaseDerivability,
        IReadOnlyList<SchemaLintWarning>? schemaLintWarnings = null)
    {
        ArgumentNullException.ThrowIfNull(compilation);
        return new ScenarioCompilationEvidence
        {
            CompiledDocument = compilation.CompiledDocument,
            CompiledHash = compilation.CompiledHash,
            AssertionCount = compilation.CompiledAssertionCount,
            IsSchemaValid = compilation.IsSchemaValid,
            AreAssertionsDerivable = IsFullyDerivable(compilation, apiDerivability, databaseDerivability),
            ApiDerivability = apiDerivability,
            DatabaseDerivability = databaseDerivability,
            SourceDescriptionSpecSnapshotIds = compilation.SourceDescriptionSpecSnapshotIds,
            LintDiagnostics = compilation.LintDiagnostics,
            SchemaLintWarnings = schemaLintWarnings?.ToList() ?? []
        };
    }

    // Derleme ciktisindan gercekten sorgulanacak checker isteklerini fail-closed planlar.
    public ScenarioDerivabilityPlan CreateDerivabilityPlan(
        ScenarioPublicationCandidate candidate,
        ArazzoCompilationResult compilation)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(compilation);
        return new ScenarioDerivabilityPlan
        {
            ApiRequests = compilation.ApiAssertions,
            DatabaseRequests = CreateDatabaseRequests(candidate, compilation)
        };
    }

    // Checker cevap listelerini birlestirip mevcut yayin kaniti kuralina uygular.
    public ScenarioCompilationEvidence CreateEvidence(
        ArazzoCompilationResult compilation,
        IReadOnlyList<DerivabilityResult> apiResults,
        IReadOnlyList<DatabaseDerivabilityResult> databaseResults,
        IReadOnlyList<SchemaLintWarning>? schemaLintWarnings = null)
    {
        return CreateEvidence(
            compilation,
            MergeApiResults(apiResults),
            databaseResults.FirstOrDefault(),
            schemaLintWarnings);
    }

    // Bagli veritabani ve derlenmis assertion birlikte varsa tek checker istegi kurar.
    private static IReadOnlyList<DatabaseDerivabilityRequest> CreateDatabaseRequests(
        ScenarioPublicationCandidate candidate,
        ArazzoCompilationResult compilation)
    {
        if (compilation.DatabaseAssertions.Count == 0 || !candidate.DbConnectionId.HasValue)
        {
            return [];
        }

        return
        [
            new DatabaseDerivabilityRequest
            {
                ConnectionId = candidate.DbConnectionId.Value,
                Assertions = compilation.DatabaseAssertions
            }
        ];
    }

    // Oturumun API ve Database Checker sourceDescription kayitlarini sabit sirada kurar.
    private static YamlSequenceNode BuildAuthoringSources(AuthoringSession session) => new(
        new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.Name, ArazzoCompilationConsts.ApiSourceDescriptionName },
            { ArazzoCompilationConsts.Fields.Url, session.ApiSourceUrl },
            { ArazzoCompilationConsts.Fields.Type, ArazzoCompilationConsts.OpenApiSourceType }
        },
        new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.Name, ArazzoCompilationConsts.DatabaseSourceDescriptionName },
            { ArazzoCompilationConsts.Fields.Url, session.DatabaseSourceUrl },
            { ArazzoCompilationConsts.Fields.Type, ArazzoCompilationConsts.OpenApiSourceType }
        });

    // Grounding ile cozulmus tek adimi operationPath, body ve assertion kriterlerine cevirir.
    private static YamlMappingNode BuildAuthoringStep(AuthoringStep step)
    {
        var result = new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.StepId, step.StepId },
            { ArazzoCompilationConsts.Fields.OperationPath, BuildApiOperationPath(step.Method, step.Path) },
            { ArazzoCompilationConsts.Fields.SuccessCriteria, new YamlSequenceNode(
                step.AssertionPaths.Select(BuildAuthoringCriterion)) }
        };
        if (!string.IsNullOrWhiteSpace(step.RequestBodyJson))
        {
            result.Add(ArazzoCompilationConsts.Fields.RequestBody, new YamlMappingNode
            {
                { ArazzoCompilationConsts.Fields.ContentType, ArazzoCompilationConsts.JsonContentType },
                { ArazzoCompilationConsts.Fields.Payload, ParseAuthoringPayload(step.RequestBodyJson) }
            });
        }
        return result;
    }

    // Cache oturumundaki DB adimlarini mekanik x-checknexus-db uzantisina cevirir.
    private static YamlMappingNode BuildAuthoringDatabaseStep(AuthoringDatabaseStep step)
    {
        var dbExtension = new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.Operation, step.OperationCode },
            { ArazzoCompilationConsts.Fields.Concept, step.TableReferenceId.ToString() }
        };

        if (step.KeyBindings.Count > 0)
        {
            var keyNode = new YamlMappingNode();
            foreach (var kvp in step.KeyBindings)
            {
                keyNode.Add(kvp.Key, kvp.Value ?? string.Empty);
            }
            dbExtension.Add(ArazzoCompilationConsts.Fields.Key, keyNode);
        }

        if (step.Expectations.Count > 0)
        {
            var expectNode = new YamlSequenceNode();
            foreach (var exp in step.Expectations)
            {
                var expNode = new YamlMappingNode
                {
                    { ArazzoCompilationConsts.Fields.ColumnName, exp.ColumnName },
                    { ArazzoCompilationConsts.Fields.Matcher, exp.MatcherCode }
                };
                if (exp.Value != null)
                {
                    expNode.Add(ArazzoCompilationConsts.Fields.Value, exp.Value);
                }
                expectNode.Add(expNode);
            }
            dbExtension.Add(ArazzoCompilationConsts.Fields.Expect, expectNode);
        }

        if (step.TimeoutMs > 0)
        {
            dbExtension.Add(ArazzoCompilationConsts.Fields.TimeoutMs, step.TimeoutMs.ToString(CultureInfo.InvariantCulture));
        }

        if (step.PollIntervalMs > 0)
        {
            dbExtension.Add(ArazzoCompilationConsts.Fields.PollIntervalMs, step.PollIntervalMs.ToString(CultureInfo.InvariantCulture));
        }

        return new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.StepId, step.StepId },
            { ArazzoCompilationConsts.DatabaseExtension, dbExtension }
        };
    }

    // API yolunu Arazzo sourceDescription JSON Pointer operationPath adresine cevirir.
    private static string BuildApiOperationPath(string method, string path)
    {
        var pointer = path.Replace("~", "~0", StringComparison.Ordinal)
            .Replace("/", "~1", StringComparison.Ordinal);
        return $"{{$sourceDescriptions.{ArazzoCompilationConsts.ApiSourceDescriptionName}.url}}#/paths/{pointer}/{method.ToLowerInvariant()}";
    }

    // Tek response JSON pointer'ini varlik kontrolu yapan basit Arazzo criterion'una cevirir.
    private static YamlMappingNode BuildAuthoringCriterion(string path) => new()
    {
        { ArazzoCompilationConsts.Fields.Condition,
            $"{ArazzoCompilationConsts.ResponseBodyPointerMarker}{path} != null" }
    };

    // Validator'dan gecmis JSON body'yi YAML payload dugumune kayipsiz aktarir.
    private static YamlNode ParseAuthoringPayload(string payload)
    {
        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(payload));
            return stream.Documents.Count == 1
                ? stream.Documents[0].RootNode
                : throw InvalidDocument(ArazzoCompilationConsts.Fields.Payload);
        }
        catch (YamlException exception)
        {
            throw new BusinessException(
                TestModuleCompilationErrorCodes.InvalidDocument,
                innerException: exception);
        }
    }

    // API checker cevaplarini assertion sirasini ve truncation olgusunu koruyarak birlestirir.
    private static DerivabilityResult? MergeApiResults(IReadOnlyList<DerivabilityResult> results)
    {
        if (results.Count == 0)
        {
            return null;
        }

        var merged = new DerivabilityResult();
        foreach (var result in results)
        {
            merged.Assertions.AddRange(result.Assertions);
            merged.IsTruncated |= result.IsTruncated;
        }

        return merged;
    }

    // Saf derleme adiminda uzantilari standart HTTP step nesnelerine cevirir.
    private ArazzoCompilationResult Compile(
        string sourceDocument,
        ProfilePack profilePack,
        Guid specSnapshotId)
    {
        ArgumentNullException.ThrowIfNull(profilePack);
        var root = LoadRoot(sourceDocument);
        EnsureDocumentContract(root);
        var result = new ArazzoCompilationResult
        {
            SourceDescriptionSpecSnapshotIds = ResolveSourceDescriptionSnapshotIds(root)
        };
        CollectApiAssertions(root, specSnapshotId, result);
        result.DatabaseAssertions = CompileDatabaseSteps(root, profilePack);
        InjectStepCorrelationHeaders(root);
        result.CompiledDocument = Serialize(root);
        result.CompiledHash = ComputeHash(result.CompiledDocument);
        result.CompiledAssertionCount = CountAssertions(result);
        return result;
    }

    // Iki yuzeyin sonucunu fail-closed birlestirir; cevaplanmamis yuzey turetilebilir sayilmaz (RULE-0006).
    private static bool IsFullyDerivable(
        ArazzoCompilationResult compilation,
        DerivabilityResult? apiDerivability,
        DatabaseDerivabilityResult? databaseDerivability)
    {
        if (compilation.CompiledAssertionCount <= 0 || compilation.UnresolvedApiAssertionCount > 0)
        {
            return false;
        }
        return IsDatabaseSurfaceDerivable(compilation, databaseDerivability) &&
               IsApiSurfaceDerivable(compilation, apiDerivability);
    }

    // DB assertion tasiyan belge icin checker'in butun adresleri turetilebilir bulmasini sart kosar.
    private static bool IsDatabaseSurfaceDerivable(
        ArazzoCompilationResult compilation,
        DatabaseDerivabilityResult? databaseDerivability)
    {
        return compilation.DatabaseAssertions.Count == 0 ||
               (databaseDerivability?.AllDerivable ?? false);
    }

    // API assertion tasiyan belge icin her pointer'in kesilmemis Derivable hukmu almasini sart kosar.
    private static bool IsApiSurfaceDerivable(
        ArazzoCompilationResult compilation,
        DerivabilityResult? apiDerivability)
    {
        if (compilation.ApiAssertions.Count == 0)
        {
            return true;
        }
        return apiDerivability is { IsTruncated: false } &&
               apiDerivability.Assertions.Count > 0 &&
               apiDerivability.Assertions.All(item => item.OutcomeCode == PtnOutcomeCodes.Derivable);
    }

    // Derlenmis belgedeki DB ve API assertion'larini tek RULE-0006 sayacinda birlestirir.
    private static int CountAssertions(ArazzoCompilationResult result)
    {
        return result.DatabaseAssertions.Count +
               result.ApiAssertions.Sum(request => request.AssertionPaths.Count) +
               result.UnresolvedApiAssertionCount;
    }

    // Kapsam raporu icin belgenin dokundugu API operasyon adreslerini kararli sirada okur.
    // Belge okuma bu modulde tek sahiplidir; kapsam Manager'i YAML tipine hic dokunmaz (KBP-110 Dilim 5).
    /// <summary>Derlenmis belgenin dokundugu operasyon adreslerini tekil ve sirali getirir.</summary>
    public static IReadOnlyList<string> ReadTouchedOperations(string? compiledDocument)
    {
        var root = TryLoadDocumentRoot(compiledDocument);
        if (root is null)
        {
            return [];
        }

        var operations = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var workflow in ReadOptionalSequence(root, ArazzoCompilationConsts.Fields.Workflows))
        {
            foreach (var step in ReadOptionalSequence(workflow, ArazzoCompilationConsts.Fields.Steps))
            {
                AddTouchedOperation(step, operations);
            }
        }

        return [.. operations];
    }

    // Database Checker adimlari API operasyonu saymaz; adres olarak yalniz sozlesme adimlari alinir.
    /// <summary>Tek adimin operasyon adresini kumeye ekler.</summary>
    private static void AddTouchedOperation(YamlMappingNode step, SortedSet<string> operations)
    {
        if (TryGetScalar(step, ArazzoCompilationConsts.Fields.OperationId, out var operationId) &&
            !string.IsNullOrWhiteSpace(operationId))
        {
            operations.Add(operationId);
            return;
        }

        if (TryGetScalar(step, ArazzoCompilationConsts.Fields.OperationPath, out var operationPath) &&
            !string.IsNullOrWhiteSpace(operationPath) &&
            !operationPath.Contains(ArazzoCompilationConsts.DatabaseSourceDescriptionName, StringComparison.Ordinal))
        {
            operations.Add(operationPath);
        }
    }

    // Rapor akislarinda eksik alan hata degildir; dizi yoksa bos kume dondurulur.
    /// <summary>Mapping alanini varsa nesne dizisi olarak okur.</summary>
    private static IEnumerable<YamlMappingNode> ReadOptionalSequence(YamlMappingNode mapping, string field)
    {
        return TryGetSequence(mapping, field, out var sequence)
            ? sequence.Children.OfType<YamlMappingNode>()
            : [];
    }

    // Derlenmis belgeyi okuyan raporlama akislari icin hata firlatmayan tek yukleme yolunu acar.
    /// <summary>Arazzo belgesini kok nesneye cevirmeyi dener; bozuk belge icin null doner.</summary>
    private static YamlMappingNode? TryLoadDocumentRoot(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return null;
        }

        try
        {
            return LoadRoot(document);
        }
        catch (BusinessException)
        {
            return null;
        }
    }

    // YAML kutuphanesiyle tek ve butceli Arazzo kok nesnesini okur.
    private static YamlMappingNode LoadRoot(string sourceDocument)
    {
        if (string.IsNullOrWhiteSpace(sourceDocument) ||
            Encoding.UTF8.GetByteCount(sourceDocument) > ArazzoCompilationConsts.MaxDocumentBytes)
        {
            throw InvalidDocument(ArazzoCompilationConsts.Fields.Arazzo);
        }

        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(sourceDocument));
            if (stream.Documents.Count != 1 || stream.Documents[0].RootNode is not YamlMappingNode root)
            {
                throw InvalidDocument(ArazzoCompilationConsts.Fields.Arazzo);
            }
            return root;
        }
        catch (YamlException exception)
        {
            throw new BusinessException(
                TestModuleCompilationErrorCodes.InvalidDocument,
                innerException: exception);
        }
    }

    // Surum, DB sourceDescription ve desteklenmeyen XPath kriterini derlemeden once dogrular.
    private static void EnsureDocumentContract(YamlMappingNode root)
    {
        var version = GetRequiredScalar(root, ArazzoCompilationConsts.Fields.Arazzo);
        if (version != ArazzoCompilationConsts.TargetVersion)
        {
            throw new BusinessException(TestModuleCompilationErrorCodes.UnsupportedVersion)
                .WithData(nameof(version), version);
        }

        EnsureDatabaseSourceDescription(root);
        if (ContainsXPathCriterion(root))
        {
            throw new BusinessException(TestModuleCompilationErrorCodes.XPathCriteriaUnsupported);
        }
    }

    // DB operationPath'in baglanacagi kararli sourceDescription kaydini zorunlu tutar.
    private static void EnsureDatabaseSourceDescription(YamlMappingNode root)
    {
        var sources = GetRequiredSequence(root, ArazzoCompilationConsts.Fields.SourceDescriptions);
        if (!sources.Children.OfType<YamlMappingNode>().Any(IsDatabaseSourceDescription))
        {
            throw new BusinessException(TestModuleCompilationErrorCodes.DatabaseSourceDescriptionMissing);
        }
    }

    // Modulun kendi checker kaydini API kaynak tanimlarindan ayirir.
    private static bool IsDatabaseSourceDescription(YamlMappingNode source)
    {
        return TryGetScalar(source, ArazzoCompilationConsts.Fields.Name, out var name) &&
               name == ArazzoCompilationConsts.DatabaseSourceDescriptionName;
    }

    // API kaynak tanimlarini satirdaki spec snapshot kimligine cozer (ADR-0020 §B/5).
    private static List<Guid> ResolveSourceDescriptionSnapshotIds(YamlMappingNode root)
    {
        var sources = GetRequiredSequence(root, ArazzoCompilationConsts.Fields.SourceDescriptions);
        return
        [
            .. sources.Children.OfType<YamlMappingNode>()
                .Where(source => !IsDatabaseSourceDescription(source))
                .Select(ReadSourceDescriptionSnapshotId)
                .Where(snapshotId => snapshotId != Guid.Empty)
        ];
    }

    // Kaynak url'indeki snapshot kimligini kararli adres ayraclariyla ayiklar; kimlik uydurmaz.
    private static Guid ReadSourceDescriptionSnapshotId(YamlMappingNode source)
    {
        if (!TryGetScalar(source, ArazzoCompilationConsts.Fields.Url, out var url))
        {
            return Guid.Empty;
        }

        var tokens = url.Split(
            ArazzoCompilationConsts.SourceUrlSeparators,
            StringSplitOptions.RemoveEmptyEntries);
        return tokens
            .Select(token => Guid.TryParseExact(token, "D", out var parsed) ? parsed : Guid.Empty)
            .FirstOrDefault(parsed => parsed != Guid.Empty);
    }

    // Tum workflow adimlarindaki CheckNexus DB uzantilarini kaynak sirasini koruyarak derler.
    private List<DatabaseDerivabilityAddress> CompileDatabaseSteps(YamlMappingNode root, ProfilePack profilePack)
    {
        var workflows = GetRequiredSequence(root, ArazzoCompilationConsts.Fields.Workflows);
        var addresses = new List<DatabaseDerivabilityAddress>();
        foreach (var workflow in workflows.Children.OfType<YamlMappingNode>())
        {
            var steps = GetRequiredSequence(workflow, ArazzoCompilationConsts.Fields.Steps);
            addresses.AddRange(CompileSteps(steps, profilePack));
        }
        return addresses;
    }

    // Her workflow adimina stepId'den turetilen standart Arazzo header parametresini enjekte eder.
    private static void InjectStepCorrelationHeaders(YamlMappingNode root)
    {
        var workflows = GetRequiredSequence(root, ArazzoCompilationConsts.Fields.Workflows);
        foreach (var workflow in workflows.Children.OfType<YamlMappingNode>())
        {
            var steps = GetRequiredSequence(workflow, ArazzoCompilationConsts.Fields.Steps);
            foreach (var step in steps.Children.OfType<YamlMappingNode>())
            {
                InjectStepCorrelationHeader(step);
            }
        }
    }

    // Kaynakta ayni header varsa belirsizligi kaldirir ve derleyicinin tek kararli degerini yazar.
    private static void InjectStepCorrelationHeader(YamlMappingNode step)
    {
        var stepKey = GetRequiredScalar(step, ArazzoCompilationConsts.Fields.StepId);
        if (stepKey.Length > PtnCorrelationConsts.MaxStepKeyLength)
        {
            throw InvalidDocument(ArazzoCompilationConsts.Fields.StepId);
        }

        var parameters = GetOrCreateParameters(step);
        RemoveExistingStepCorrelationHeaders(parameters);
        parameters.Add(new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.Name, WorkflowRunnerConsts.StepKeyHeaderName },
            { ArazzoCompilationConsts.Fields.In, ArazzoCompilationConsts.HeaderParameterLocation },
            { ArazzoCompilationConsts.Fields.Value, stepKey }
        });
    }

    // Kaynaktaki ayni header tanimlarini derleyicinin tek sahipli degeri icin kaldirir.
    private static void RemoveExistingStepCorrelationHeaders(YamlSequenceNode parameters)
    {
        var existing = parameters.Children
            .OfType<YamlMappingNode>()
            .Where(IsStepKeyHeaderParameter)
            .ToList();
        foreach (var parameter in existing)
        {
            parameters.Children.Remove(parameter);
        }
    }

    // Adimin mevcut parameter dizisini kullanir veya standart dizi sahibini olusturur.
    private static YamlSequenceNode GetOrCreateParameters(YamlMappingNode step)
    {
        if (!TryGet(step, ArazzoCompilationConsts.Fields.Parameters, out var node))
        {
            var created = new YamlSequenceNode();
            Set(step, ArazzoCompilationConsts.Fields.Parameters, created);
            return created;
        }

        return node as YamlSequenceNode ??
               throw InvalidDocument(ArazzoCompilationConsts.Fields.Parameters);
    }

    // Yalniz ADR-0022'nin header konumundaki kararli parametresini eslestirir.
    private static bool IsStepKeyHeaderParameter(YamlMappingNode parameter)
    {
        return TryGetScalar(parameter, ArazzoCompilationConsts.Fields.Name, out var name) &&
               TryGetScalar(parameter, ArazzoCompilationConsts.Fields.In, out var location) &&
               string.Equals(name, WorkflowRunnerConsts.StepKeyHeaderName, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(location, ArazzoCompilationConsts.HeaderParameterLocation, StringComparison.OrdinalIgnoreCase);
    }

    // Bir workflow icindeki uzantili step'leri standart Arazzo alanlariyla degistirir.
    private List<DatabaseDerivabilityAddress> CompileSteps(YamlSequenceNode steps, ProfilePack profilePack)
    {
        var addresses = new List<DatabaseDerivabilityAddress>();
        foreach (var step in steps.Children.OfType<YamlMappingNode>())
        {
            if (!TryGetMapping(step, ArazzoCompilationConsts.DatabaseExtension, out var extension))
            {
                continue;
            }

            addresses.Add(CompileStep(step, extension, profilePack));
        }
        return addresses;
    }

    // Tek uzantiyi profil bagi, endpoint yolu, request govdesi ve Passed kriterine indirger.
    private DatabaseDerivabilityAddress CompileStep(
        YamlMappingNode step,
        YamlMappingNode extension,
        ProfilePack profilePack)
    {
        EnsureNoOperationReference(step);
        var operation = GetRequiredScalar(extension, ArazzoCompilationConsts.Fields.Operation);
        EnsureOperationIsSupported(operation);
        var concept = GetRequiredScalar(extension, ArazzoCompilationConsts.Fields.Concept);
        var binding = _profilePackManager.ResolveConcept(profilePack, concept);
        var requestBody = BuildRequestBody(step, extension, binding, operation);

        Remove(step, ArazzoCompilationConsts.DatabaseExtension);
        Remove(step, ArazzoCompilationConsts.Fields.Timeout);
        Set(step, ArazzoCompilationConsts.Fields.OperationPath, Scalar(BuildOperationPath(operation)));
        Set(step, ArazzoCompilationConsts.Fields.RequestBody, requestBody);
        EnsurePassedCriterion(step);
        EnsureObservedRowCountOutput(step);
        return BuildDerivabilityAddress(binding, requestBody);
    }

    // Derlenmis checker payload'ini turetilebilirlik yuzeyinin okudugu somut adrese cevirir.
    private static DatabaseDerivabilityAddress BuildDerivabilityAddress(
        ConceptBinding binding,
        YamlMappingNode requestBody)
    {
        var payload = GetRequiredMapping(requestBody, ArazzoCompilationConsts.Fields.Payload);
        var keys = GetRequiredMapping(payload, ArazzoCompilationConsts.Fields.KeyValues);
        var expectations = GetRequiredSequence(payload, ArazzoCompilationConsts.Fields.Expectations);
        var cardinality = GetRequiredMapping(payload, ArazzoCompilationConsts.Fields.Cardinality);
        return new DatabaseDerivabilityAddress
        {
            SchemaName = binding.DbSchemaName,
            TableName = binding.TableName,
            KeyColumns = [.. keys.Children.Keys.Select(key => GetScalarValue(key, ArazzoCompilationConsts.Fields.KeyValues))],
            ExpectedColumns = [.. ReadExpectationColumns(expectations)],
            MatcherCode = ResolveMatcherCode(expectations),
            CardinalityKindCode = GetRequiredScalar(cardinality, ArazzoCompilationConsts.Fields.KindCode)
        };
    }

    // Derlenmis beklenti listesindeki somut kolon adlarini kararli sirada okur.
    private static IEnumerable<string> ReadExpectationColumns(YamlSequenceNode expectations)
    {
        return expectations.Children
            .OfType<YamlMappingNode>()
            .Select(item => GetRequiredScalar(item, ArazzoCompilationConsts.Fields.ColumnName));
    }

    // Adres basina tek matcher tasiyan checker sozlesmesine ilk beklentinin matcher kodunu verir.
    private static string ResolveMatcherCode(YamlSequenceNode expectations)
    {
        var first = expectations.Children.OfType<YamlMappingNode>().FirstOrDefault();
        return first is null
            ? MatcherKindCodes.Equals
            : GetRequiredScalar(first, ArazzoCompilationConsts.Fields.MatcherKindCode);
    }

    // Uzantisiz adimlarin response assertion'larini API turetilebilirlik istegine cevirir.
    private static void CollectApiAssertions(
        YamlMappingNode root,
        Guid specSnapshotId,
        ArazzoCompilationResult result)
    {
        var workflows = GetRequiredSequence(root, ArazzoCompilationConsts.Fields.Workflows);
        foreach (var workflow in workflows.Children.OfType<YamlMappingNode>())
        {
            var steps = GetRequiredSequence(workflow, ArazzoCompilationConsts.Fields.Steps);
            CollectWorkflowApiAssertions(steps, specSnapshotId, result);
        }
    }

    // Tek workflow icindeki DB disi adimlari assertion pointer'lariyla toplar.
    private static void CollectWorkflowApiAssertions(
        YamlSequenceNode steps,
        Guid specSnapshotId,
        ArazzoCompilationResult result)
    {
        foreach (var step in steps.Children.OfType<YamlMappingNode>())
        {
            if (!Has(step, ArazzoCompilationConsts.DatabaseExtension))
            {
                AddApiAssertion(step, specSnapshotId, result);
            }
        }
    }

    // Assertion tasiyan API adimini cozulebilir operasyon adresiyle eslestirir, cozulemezse fail-closed sayar.
    private static void AddApiAssertion(
        YamlMappingNode step,
        Guid specSnapshotId,
        ArazzoCompilationResult result)
    {
        var assertionPaths = ReadResponseAssertionPaths(step);
        if (assertionPaths.Count == 0)
        {
            return;
        }

        if (!TryReadOperationAddress(step, out var method, out var path))
        {
            result.UnresolvedApiAssertionCount += assertionPaths.Count;
            return;
        }

        result.ApiAssertions.Add(CreateApiRequest(step, specSnapshotId, method, path, assertionPaths));
    }

    // Cozulmus operasyon adresini ve pointer listesini checker turetilebilirlik istegine cevirir.
    private static DerivabilityRequest CreateApiRequest(
        YamlMappingNode step,
        Guid specSnapshotId,
        string method,
        string path,
        List<string> assertionPaths)
    {
        return new DerivabilityRequest
        {
            SnapshotId = specSnapshotId,
            OperationId = TryGetScalar(step, ArazzoCompilationConsts.Fields.OperationId, out var operationId)
                ? operationId
                : null,
            Method = method,
            Path = path,
            AssertionPaths = assertionPaths
        };
    }

    // successCriteria icindeki response govdesi pointer'larini kararli sirada assertion yoluna cevirir.
    private static List<string> ReadResponseAssertionPaths(YamlMappingNode step)
    {
        if (!TryGetSequence(step, ArazzoCompilationConsts.Fields.SuccessCriteria, out var criteria))
        {
            return [];
        }

        return
        [
            .. criteria.Children.OfType<YamlMappingNode>()
                .Select(ReadAssertionPointer)
                .Where(pointer => pointer.Length > 0)
        ];
    }

    // Tek criterion'un $response.body pointer bolumunu ilk bosluga kadar ayiklar.
    private static string ReadAssertionPointer(YamlMappingNode criterion)
    {
        if (!TryGetScalar(criterion, ArazzoCompilationConsts.Fields.Condition, out var condition))
        {
            return string.Empty;
        }

        var marker = condition.IndexOf(
            ArazzoCompilationConsts.ResponseBodyPointerMarker,
            StringComparison.Ordinal);
        if (marker < 0)
        {
            return string.Empty;
        }

        var pointer = condition[(marker + ArazzoCompilationConsts.ResponseBodyPointerMarker.Length)..];
        var end = pointer.IndexOf(' ', StringComparison.Ordinal);
        return end < 0 ? pointer : pointer[..end];
    }

    // operationPath JSON Pointer'ini OpenAPI yoluna ve HTTP metoduna geri cozer.
    private static bool TryReadOperationAddress(YamlMappingNode step, out string method, out string path)
    {
        method = string.Empty;
        path = string.Empty;
        if (!TryGetScalar(step, ArazzoCompilationConsts.Fields.OperationPath, out var operationPath))
        {
            return false;
        }

        var marker = operationPath.IndexOf(ArazzoCompilationConsts.PathsPointerMarker, StringComparison.Ordinal);
        if (marker < 0)
        {
            return false;
        }

        var segments = operationPath[(marker + ArazzoCompilationConsts.PathsPointerMarker.Length)..].Split('/');
        if (segments.Length != 2)
        {
            return false;
        }

        path = segments[0].Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
        method = segments[1];
        return path.Length > 0 && method.Length > 0;
    }

    // Extension step'inin baska bir operation/workflow hedefiyle belirsizlesmesini engeller.
    private static void EnsureNoOperationReference(YamlMappingNode step)
    {
        var hasReference = Has(step, ArazzoCompilationConsts.Fields.OperationId) ||
                           Has(step, ArazzoCompilationConsts.Fields.OperationPath) ||
                           Has(step, ArazzoCompilationConsts.Fields.WorkflowId);
        if (hasReference)
        {
            throw InvalidDocument(ArazzoCompilationConsts.Fields.OperationPath);
        }
    }

    // Uzanti operation kodunu desteklenen uc Database Checker endpoint'iyle sinirlar.
    private static void EnsureOperationIsSupported(string operation)
    {
        if (!ArazzoCompilationConsts.Operations.All.Contains(operation))
        {
            throw new BusinessException(TestModuleCompilationErrorCodes.UnsupportedDatabaseOperation)
                .WithData(nameof(operation), operation);
        }
    }

    // Profil bagindan somut adres, anahtar, beklenti, cardinality ve polling payload'ini kurar.
    private static YamlMappingNode BuildRequestBody(
        YamlMappingNode step,
        YamlMappingNode extension,
        ConceptBinding binding,
        string operation)
    {
        var payload = new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.ConnectionId, ArazzoCompilationConsts.DatabaseConnectionRuntimeExpression },
            { ArazzoCompilationConsts.Fields.SchemaName, binding.DbSchemaName },
            { ArazzoCompilationConsts.Fields.TableName, binding.TableName },
            { ArazzoCompilationConsts.Fields.KeyValues, BuildKeyValues(extension, binding) },
            { ArazzoCompilationConsts.Fields.Expectations, BuildExpectations(extension, binding) },
            { ArazzoCompilationConsts.Fields.Cardinality, BuildCardinality(extension, operation) },
            { ArazzoCompilationConsts.Fields.TimeoutMs, ReadTimeoutMs(step, extension).ToString(CultureInfo.InvariantCulture) },
            { ArazzoCompilationConsts.Fields.PollIntervalMs, ReadPositiveInt(extension, ArazzoCompilationConsts.Fields.PollIntervalMs, ArazzoCompilationConsts.DefaultPollIntervalMs).ToString(CultureInfo.InvariantCulture) },
            { ArazzoCompilationConsts.Fields.IncludeRowOnFailure, ReadBoolean(extension, ArazzoCompilationConsts.Fields.IncludeRowOnFailure, true).ToString().ToLowerInvariant() }
        };
        return new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.ContentType, "application/json" },
            { ArazzoCompilationConsts.Fields.Payload, payload }
        };
    }

    // Kavramsal key rollerini profil paketindeki somut kolon adlarina cevirir.
    private static YamlMappingNode BuildKeyValues(
        YamlMappingNode extension,
        ConceptBinding binding)
    {
        var keys = GetRequiredMapping(extension, ArazzoCompilationConsts.Fields.Key);
        if (keys.Children.Count == 0)
        {
            throw InvalidAssertion(ArazzoCompilationConsts.Fields.Key);
        }

        var result = new YamlMappingNode();
        foreach (var item in keys.Children)
        {
            var role = GetScalarValue(item.Key, ArazzoCompilationConsts.Fields.Key);
            var value = GetScalarValue(item.Value, role);
            result.Add(ResolveColumn(binding, role), value);
        }
        return result;
    }

    // Kavramsal expectation rollerini checker'in tipli kolon beklentisi listesine cevirir.
    private static YamlSequenceNode BuildExpectations(
        YamlMappingNode extension,
        ConceptBinding binding)
    {
        var result = new YamlSequenceNode();
        if (!TryGetMapping(extension, ArazzoCompilationConsts.Fields.Expect, out var expectations))
        {
            return result;
        }

        foreach (var item in expectations.Children)
        {
            var role = GetScalarValue(item.Key, ArazzoCompilationConsts.Fields.Expect);
            result.Add(BuildExpectation(ResolveColumn(binding, role), item.Value));
        }
        return result;
    }

    // Scalar kisayolunu ve ayrintili expectation tanimini tek checker beklenti sekline indirger.
    private static YamlMappingNode BuildExpectation(string columnName, YamlNode source)
    {
        if (source is YamlMappingNode mapping)
        {
            return BuildDetailedExpectation(columnName, mapping);
        }

        if (source is not YamlScalarNode scalar)
        {
            throw InvalidAssertion(ArazzoCompilationConsts.Fields.Expect);
        }

        return new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.ColumnName, columnName },
            { ArazzoCompilationConsts.Fields.MatcherKindCode, MatcherKindCodes.Equals },
            { ArazzoCompilationConsts.Fields.ExpectedValue, scalar.Value ?? string.Empty }
        };
    }

    // Matcher, deger, deger listesi ve tolerans tasiyan ayrintili beklentiyi cevirir.
    private static YamlMappingNode BuildDetailedExpectation(string columnName, YamlMappingNode mapping)
    {
        var matcher = TryGetScalar(mapping, ArazzoCompilationConsts.Fields.Matcher, out var value)
            ? NormalizeMatcher(value)
            : MatcherKindCodes.Equals;
        var result = new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.ColumnName, columnName },
            { ArazzoCompilationConsts.Fields.MatcherKindCode, matcher }
        };
        CopyOptionalScalar(mapping, ArazzoCompilationConsts.Fields.Value, result, ArazzoCompilationConsts.Fields.ExpectedValue);
        CopyOptionalSequence(mapping, ArazzoCompilationConsts.Fields.Values, result, ArazzoCompilationConsts.Fields.ExpectedValues);
        CopyOptionalScalar(mapping, ArazzoCompilationConsts.Fields.Tolerance, result, ArazzoCompilationConsts.Fields.Tolerance);
        return result;
    }

    // Uzanti matcher adini Database Checker'in kapali kod sozlugune cevirir.
    private static string NormalizeMatcher(string matcher)
    {
        var normalized = matcher.ToLowerInvariant();
        return normalized switch
        {
            "equals" => MatcherKindCodes.Equals,
            "notequals" => MatcherKindCodes.NotEquals,
            "isnull" => MatcherKindCodes.IsNull,
            "isnotnull" => MatcherKindCodes.IsNotNull,
            "greaterthan" => MatcherKindCodes.GreaterThan,
            "greaterthanorequal" => MatcherKindCodes.GreaterThanOrEqual,
            "lessthan" => MatcherKindCodes.LessThan,
            "lessthanorequal" => MatcherKindCodes.LessThanOrEqual,
            "matchesregex" => MatcherKindCodes.MatchesRegex,
            "oneof" => MatcherKindCodes.OneOf,
            "withintolerance" => MatcherKindCodes.WithinTolerance,
            _ => throw InvalidAssertion(ArazzoCompilationConsts.Fields.Matcher)
        };
    }

    // Operation semantigine gore sabit veya kullanici tanimli cardinality nesnesi kurar.
    private static YamlMappingNode BuildCardinality(YamlMappingNode extension, string operation)
    {
        if (operation == ArazzoCompilationConsts.Operations.AssertRow)
        {
            return Cardinality(CardinalityKindCodes.Exactly, 1);
        }
        if (operation == ArazzoCompilationConsts.Operations.AssertAbsent)
        {
            return Cardinality(CardinalityKindCodes.None, 0);
        }

        return ParseCountCardinality(extension);
    }

    // Count uzantisinin mapping veya "exactly 1" kisaltmasini kapali cardinality'ye cevirir.
    private static YamlMappingNode ParseCountCardinality(YamlMappingNode extension)
    {
        if (!TryGet(extension, ArazzoCompilationConsts.Fields.Cardinality, out var node))
        {
            throw InvalidAssertion(ArazzoCompilationConsts.Fields.Cardinality);
        }

        return node is YamlMappingNode mapping
            ? ParseCardinalityMapping(mapping)
            : ParseCardinalityShorthand(GetScalarValue(node, ArazzoCompilationConsts.Fields.Cardinality));
    }

    // Ayrintili cardinality nesnesini kapali kod ve beklenen sayiya cevirir.
    private static YamlMappingNode ParseCardinalityMapping(YamlMappingNode mapping)
    {
        var kind = NormalizeCardinality(GetRequiredScalar(mapping, ArazzoCompilationConsts.Fields.Kind));
        var count = ReadPositiveLong(mapping, ArazzoCompilationConsts.Fields.Count, kind == CardinalityKindCodes.None ? 0 : null);
        return Cardinality(kind, count);
    }

    // "atLeast 1" bicimindeki kisayolu kapali cardinality koduna ve beklenen sayiya cevirir.
    private static YamlMappingNode ParseCardinalityShorthand(string shorthand)
    {
        var parts = shorthand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is < 1 or > 2)
        {
            throw InvalidAssertion(ArazzoCompilationConsts.Fields.Cardinality);
        }

        var kind = NormalizeCardinality(parts[0]);
        if (parts.Length == 2)
        {
            return Cardinality(kind, ParseNonNegativeLong(parts[1], ArazzoCompilationConsts.Fields.Count));
        }
        return kind == CardinalityKindCodes.None
            ? Cardinality(kind, 0)
            : throw InvalidAssertion(ArazzoCompilationConsts.Fields.Count);
    }

    // Uzanti cardinality adini Database Checker'in kapali kod sozlugune cevirir.
    private static string NormalizeCardinality(string kind)
    {
        return kind.ToLowerInvariant() switch
        {
            "exactly" => CardinalityKindCodes.Exactly,
            "atleast" => CardinalityKindCodes.AtLeast,
            "none" => CardinalityKindCodes.None,
            _ => throw InvalidAssertion(ArazzoCompilationConsts.Fields.Cardinality)
        };
    }

    // Kapali cardinality kodu ile beklenen sayiyi checker payload nesnesine yerlestirir.
    private static YamlMappingNode Cardinality(string kindCode, long expectedCount)
        => new()
        {
            { ArazzoCompilationConsts.Fields.KindCode, kindCode },
            { ArazzoCompilationConsts.Fields.ExpectedCount, expectedCount.ToString(CultureInfo.InvariantCulture) }
        };

    // DB step timeout'unu uzanti alani, eski step alani ve paket varsayilani sirasiyla cozer.
    private static int ReadTimeoutMs(YamlMappingNode step, YamlMappingNode extension)
    {
        if (TryReadPositiveInt(extension, ArazzoCompilationConsts.Fields.TimeoutMs, out var timeoutMs))
        {
            return timeoutMs;
        }
        return TryReadPositiveInt(step, ArazzoCompilationConsts.Fields.Timeout, out var timeout)
            ? timeout
            : ArazzoCompilationConsts.DefaultTimeoutMs;
    }

    // DB operation kodunu resmi checker rotasinin Arazzo JSON Pointer adresine cevirir.
    private static string BuildOperationPath(string operation)
    {
        var segment = operation switch
        {
            ArazzoCompilationConsts.Operations.AssertRow => DatabaseCheckerHttpApiConstants.Segments.RowAssertion,
            ArazzoCompilationConsts.Operations.AssertCount => DatabaseCheckerHttpApiConstants.Segments.CountAssertion,
            ArazzoCompilationConsts.Operations.AssertAbsent => DatabaseCheckerHttpApiConstants.Segments.AbsentAssertion,
            _ => throw new BusinessException(TestModuleCompilationErrorCodes.UnsupportedDatabaseOperation)
        };
        var path = $"/{DatabaseCheckerHttpApiConstants.Routes.Assertions}/{segment}";
        var pointer = path.Replace("~", "~0", StringComparison.Ordinal)
            .Replace("/", "~1", StringComparison.Ordinal);
        return $"{{$sourceDescriptions.{ArazzoCompilationConsts.DatabaseSourceDescriptionName}.url}}#/paths/{pointer}/post";
    }

    // Checker Passed alanini her derlenmis DB adiminin ilk zorunlu basari kriteri yapar.
    private static void EnsurePassedCriterion(YamlMappingNode step)
    {
        var generated = new YamlMappingNode
        {
            { ArazzoCompilationConsts.Fields.Condition, ArazzoCompilationConsts.PassedCriterion }
        };
        if (TryGetSequence(step, ArazzoCompilationConsts.Fields.SuccessCriteria, out var criteria))
        {
            criteria.Children.Insert(0, generated);
            return;
        }
        Set(step, ArazzoCompilationConsts.Fields.SuccessCriteria, new YamlSequenceNode(generated));
    }

    // Gozlenen satir sayisini sonraki Arazzo adimlarina kararli output olarak acar.
    private static void EnsureObservedRowCountOutput(YamlMappingNode step)
    {
        if (!TryGetMapping(step, ArazzoCompilationConsts.Fields.Outputs, out var outputs))
        {
            outputs = new YamlMappingNode();
            Set(step, ArazzoCompilationConsts.Fields.Outputs, outputs);
        }
        if (!Has(outputs, ArazzoCompilationConsts.Fields.ObservedRowCount))
        {
            outputs.Add(
                ArazzoCompilationConsts.Fields.ObservedRowCount,
                ArazzoCompilationConsts.ObservedRowCountExpression);
        }
    }

    // Profil columnMap'inde bulunmayan kavramsal kolon icin somut ad tahmin etmez.
    private static string ResolveColumn(ConceptBinding binding, string role)
    {
        if (binding.ColumnMap.TryGetValue(role, out var column) && !string.IsNullOrWhiteSpace(column))
        {
            return column;
        }
        throw new BusinessException(TestModuleCompilationErrorCodes.ConceptColumnNotBound)
            .WithData(nameof(role), role);
    }

    // Herhangi bir criterion nesnesindeki type=xpath degerini tum belge agacinda bulur.
    private static bool ContainsXPathCriterion(YamlNode node)
    {
        if (node is YamlMappingNode mapping)
        {
            if (TryGetScalar(mapping, ArazzoCompilationConsts.Fields.Type, out var type) &&
                string.Equals(type, "xpath", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return mapping.Children.Any(item => ContainsXPathCriterion(item.Value));
        }
        return node is YamlSequenceNode sequence && sequence.Children.Any(ContainsXPathCriterion);
    }

    // Mapping alanini zorunlu nesne olarak okur.
    private static YamlMappingNode GetRequiredMapping(YamlMappingNode mapping, string field)
    {
        return TryGetMapping(mapping, field, out var result)
            ? result
            : throw InvalidAssertion(field);
    }

    // Mapping alanini zorunlu dizi olarak okur.
    private static YamlSequenceNode GetRequiredSequence(YamlMappingNode mapping, string field)
    {
        return TryGetSequence(mapping, field, out var result)
            ? result
            : throw InvalidDocument(field);
    }

    // Mapping alanini zorunlu ve bos olmayan scalar olarak okur.
    private static string GetRequiredScalar(YamlMappingNode mapping, string field)
    {
        return TryGetScalar(mapping, field, out var result) && !string.IsNullOrWhiteSpace(result)
            ? result
            : throw InvalidDocument(field);
    }

    // Dugumu bos olmayan scalar metnine cevirir.
    private static string GetScalarValue(YamlNode node, string field)
    {
        return node is YamlScalarNode scalar && scalar.Value is not null
            ? scalar.Value
            : throw InvalidAssertion(field);
    }

    // Opsiyonel scalar alani baska bir adla hedef mapping'e kopyalar.
    private static void CopyOptionalScalar(
        YamlMappingNode source,
        string sourceField,
        YamlMappingNode target,
        string targetField)
    {
        if (TryGetScalar(source, sourceField, out var value))
        {
            target.Add(targetField, value);
        }
    }

    // Opsiyonel scalar diziyi checker hedef alanina yeni node'larla kopyalar.
    private static void CopyOptionalSequence(
        YamlMappingNode source,
        string sourceField,
        YamlMappingNode target,
        string targetField)
    {
        if (!TryGetSequence(source, sourceField, out var values))
        {
            return;
        }
        var copy = new YamlSequenceNode(values.Children.Select(item => Scalar(GetScalarValue(item, sourceField))));
        target.Add(targetField, copy);
    }

    // Pozitif int alani yoksa paket varsayilanini dondurur.
    private static int ReadPositiveInt(YamlMappingNode mapping, string field, int defaultValue)
        => TryReadPositiveInt(mapping, field, out var value) ? value : defaultValue;

    // Opsiyonel int alani pozitifse parse eder; gecersiz degeri sessizce varsayilana indirmez.
    private static bool TryReadPositiveInt(YamlMappingNode mapping, string field, out int value)
    {
        value = default;
        if (!TryGet(mapping, field, out var node))
        {
            return false;
        }
        var scalar = GetScalarValue(node, field);
        if (!int.TryParse(scalar, NumberStyles.None, CultureInfo.InvariantCulture, out value) || value <= 0)
        {
            throw InvalidAssertion(field);
        }
        return true;
    }

    // Cardinality sayisini non-negative long olarak okur veya verilen varsayilani uygular.
    private static long ReadPositiveLong(YamlMappingNode mapping, string field, long? defaultValue)
    {
        if (!TryGet(mapping, field, out var node))
        {
            return defaultValue ?? throw InvalidAssertion(field);
        }
        return ParseNonNegativeLong(GetScalarValue(node, field), field);
    }

    // Cardinality metnini sifir veya pozitif long degerine cevirir.
    private static long ParseNonNegativeLong(string value, string field)
    {
        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) || parsed < 0)
        {
            throw InvalidAssertion(field);
        }
        return parsed;
    }

    // Opsiyonel boolean alani parse eder veya paket varsayilanini dondurur.
    private static bool ReadBoolean(YamlMappingNode mapping, string field, bool defaultValue)
    {
        if (!TryGet(mapping, field, out var node))
        {
            return defaultValue;
        }
        return bool.TryParse(GetScalarValue(node, field), out var parsed)
            ? parsed
            : throw InvalidAssertion(field);
    }

    // Mapping'de verilen alanin varligini bildirir.
    private static bool Has(YamlMappingNode mapping, string field)
        => mapping.Children.ContainsKey(Scalar(field));

    // Mapping alanini turden bagimsiz okumaya calisir.
    private static bool TryGet(YamlMappingNode mapping, string field, out YamlNode node)
    {
        if (mapping.Children.TryGetValue(Scalar(field), out var found))
        {
            node = found;
            return true;
        }
        node = null!;
        return false;
    }

    // Mapping alanini nesne olarak okumaya calisir.
    private static bool TryGetMapping(YamlMappingNode mapping, string field, out YamlMappingNode result)
    {
        if (TryGet(mapping, field, out var node) && node is YamlMappingNode found)
        {
            result = found;
            return true;
        }
        result = null!;
        return false;
    }

    // Mapping alanini dizi olarak okumaya calisir.
    private static bool TryGetSequence(YamlMappingNode mapping, string field, out YamlSequenceNode result)
    {
        if (TryGet(mapping, field, out var node) && node is YamlSequenceNode found)
        {
            result = found;
            return true;
        }
        result = null!;
        return false;
    }

    // Mapping alanini scalar olarak okumaya calisir.
    private static bool TryGetScalar(YamlMappingNode mapping, string field, out string value)
    {
        if (TryGet(mapping, field, out var node) && node is YamlScalarNode scalar && scalar.Value is not null)
        {
            value = scalar.Value;
            return true;
        }
        value = string.Empty;
        return false;
    }

    // Mapping alanini tek sahipli yeni degerle ekler veya degistirir.
    private static void Set(YamlMappingNode mapping, string field, YamlNode value)
        => mapping.Children[Scalar(field)] = value;

    // Derlenince gecersiz olacak kaynak uzanti alanini mapping'den kaldirir.
    private static void Remove(YamlMappingNode mapping, string field)
        => mapping.Children.Remove(Scalar(field));

    // Kararli scalar key/value dugumu olusturur.
    private static YamlScalarNode Scalar(string value) => new(value);

    // Tek dokumani satir sonu dahil kararli YAML metnine cevirir.
    private static string Serialize(YamlMappingNode root)
    {
        var stream = new YamlStream(new YamlDocument(root));
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        stream.Save(writer, assignAnchors: false);
        return writer.ToString().ReplaceLineEndings("\n");
    }

    // Derlenmis belge baytlarinin lowercase SHA-256 kimligini hesaplar.
    private static string ComputeHash(string document)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(document))).ToLowerInvariant();

    // Belge sekli hatasini alan adresiyle kararli BusinessException'a cevirir.
    private static BusinessException InvalidDocument(string field)
        => new BusinessException(TestModuleCompilationErrorCodes.InvalidDocument)
            .WithData(nameof(field), field);

    // DB uzanti sekli hatasini alan adresiyle kararli BusinessException'a cevirir.
    private static BusinessException InvalidAssertion(string field)
        => new BusinessException(TestModuleCompilationErrorCodes.InvalidDatabaseAssertion)
            .WithData(nameof(field), field);
}

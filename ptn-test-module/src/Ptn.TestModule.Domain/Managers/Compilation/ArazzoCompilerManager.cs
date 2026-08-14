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
using Ptn.TestModule.Constants.Compilation;
using Ptn.TestModule.ExceptionCodes.Compilation;
using Ptn.TestModule.Interface.Compilation;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Models.Bridge;
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
        CancellationToken cancellationToken = default)
    {
        var result = Compile(sourceDocument, profilePack);
        var lintResult = await _documentLinter.LintAsync(result.CompiledDocument, cancellationToken);
        result.IsSchemaValid = lintResult.IsValid;
        result.LintDiagnostics = lintResult.Diagnostics;
        return result;
    }

    // Saf derleme adiminda uzantilari standart HTTP step nesnelerine cevirir.
    private ArazzoCompilationResult Compile(string sourceDocument, ProfilePack profilePack)
    {
        ArgumentNullException.ThrowIfNull(profilePack);
        var root = LoadRoot(sourceDocument);
        EnsureDocumentContract(root);
        var assertionCount = CompileDatabaseSteps(root, profilePack);
        var compiledDocument = Serialize(root);
        return new ArazzoCompilationResult
        {
            CompiledDocument = compiledDocument,
            CompiledHash = ComputeHash(compiledDocument),
            CompiledAssertionCount = assertionCount
        };
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
        var exists = sources.Children.OfType<YamlMappingNode>().Any(source =>
            TryGetScalar(source, ArazzoCompilationConsts.Fields.Name, out var name) &&
            name == ArazzoCompilationConsts.DatabaseSourceDescriptionName);
        if (!exists)
        {
            throw new BusinessException(TestModuleCompilationErrorCodes.DatabaseSourceDescriptionMissing);
        }
    }

    // Tum workflow adimlarindaki CheckNexus DB uzantilarini kaynak sirasini koruyarak derler.
    private int CompileDatabaseSteps(YamlMappingNode root, ProfilePack profilePack)
    {
        var workflows = GetRequiredSequence(root, ArazzoCompilationConsts.Fields.Workflows);
        var assertionCount = 0;
        foreach (var workflow in workflows.Children.OfType<YamlMappingNode>())
        {
            var steps = GetRequiredSequence(workflow, ArazzoCompilationConsts.Fields.Steps);
            assertionCount += CompileSteps(steps, profilePack);
        }
        return assertionCount;
    }

    // Bir workflow icindeki uzantili step'leri standart Arazzo alanlariyla degistirir.
    private int CompileSteps(YamlSequenceNode steps, ProfilePack profilePack)
    {
        var compiledCount = 0;
        foreach (var step in steps.Children.OfType<YamlMappingNode>())
        {
            if (!TryGetMapping(step, ArazzoCompilationConsts.DatabaseExtension, out var extension))
            {
                continue;
            }

            CompileStep(step, extension, profilePack);
            compiledCount++;
        }
        return compiledCount;
    }

    // Tek uzantiyi profil bagi, endpoint yolu, request govdesi ve Passed kriterine indirger.
    private void CompileStep(
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

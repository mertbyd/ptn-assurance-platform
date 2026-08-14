using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Bridge.Vocabulary;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Bridge;

// islevi: Profil paketini dogrular, sema muhru kaymasini uygular ve kavram kapsam karari verir.
// sistemdeki gorevi: Baglama, kapsam ve sessiz sema drift'i kurallarinin tek domain sahibidir.
public class ProfilePackManager : TestModuleDomainService
{
    // Yuklenmis profili kapali sozluklerle dogrular ve sema kaymasinda onaylari geri alir.
    public ProfilePack GetValidated(
        ProfilePack pack,
        string profileKey,
        string currentFingerprint)
    {
        ValidatePack(pack, profileKey);
        DowngradeDriftedBindings(pack, currentFingerprint);
        return pack;
    }
    // Onayli kavram bagini dondurur; baglanmamis kavrami tahmin etmez.
    public ConceptBinding ResolveConcept(ProfilePack pack, string conceptCode)
    {
        var binding = pack.Bindings.FirstOrDefault(item =>
            item.ConceptCode == conceptCode && item.StateCode == PtnBindingStateCodes.Approved);
        if (binding is not null)
        {
            return binding;
        }

        throw new BusinessException(TestModuleBridgeErrorCodes.ConceptNotBound)
            .WithData(nameof(conceptCode), conceptCode);
    }
    // Gerekli kavramlari onayli baglamalarla karsilastirip bound/required oranini uretir.
    public CoverageReport BuildCoverage(
        ProfilePack pack,
        IReadOnlyCollection<string> requiredConcepts)
    {
        var required = requiredConcepts.Distinct(StringComparer.Ordinal).Order().ToList();
        var approved = pack.Bindings
            .Where(item => item.StateCode == PtnBindingStateCodes.Approved)
            .Select(item => item.ConceptCode)
            .ToHashSet(StringComparer.Ordinal);
        var bound = required.Where(approved.Contains).ToList();
        var unbound = required.Where(item => !approved.Contains(item)).ToList();
        return CreateCoverage(required, bound, unbound);
    }
    // Dogrulanmis profil paketinden istenen kapali kavramlar icin ozet-once knowledge sonucunu kurar.
    public KnowledgeResult GetKnowledge(
        KnowledgeRequest request,
        ProfilePack pack,
        string currentFingerprint)
    {
        GetValidated(pack, request.ProfileKey, currentFingerprint);
        var concepts = request.ConceptCodes.Distinct(StringComparer.Ordinal).Order().ToList();
        return new KnowledgeResult
        {
            ResponseFormat = request.ResponseFormat,
            Coverage = BuildCoverage(pack, concepts),
            ConceptCodes = concepts,
            ResourceLink = request.ResponseFormat == PtnResponseFormatCodes.Concise
                ? PtnBridgeRoutes.Resource(PtnToolCodes.Knowledge)
                : null
        };
    }
    // Profil kimligini, kapali kodlarini ve sinirli hukum dilini dogrular.
    private static void ValidatePack(ProfilePack pack, string expectedProfileKey)
    {
        if (pack.ProfileKey != expectedProfileKey || string.IsNullOrWhiteSpace(pack.Revision))
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }

        EnsureBindingsAreKnown(pack.Bindings);
        EnsurePathsAreKnown(pack.Paths);
    }
    // Kavram, desen ve durum kodlarinin kapali sozluklerde bulunmasini zorunlu kilar.
    private static void EnsureBindingsAreKnown(IEnumerable<ConceptBinding> bindings)
    {
        var invalid = bindings.Any(item =>
            !PtnConceptCodes.All.Contains(item.ConceptCode) ||
            !PtnBindingPatternCodes.All.Contains(item.PatternCode) ||
            !PtnBindingStateCodes.All.Contains(item.StateCode));
        if (invalid)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }
    }
    // Yol adimlarini kapali dugum/kaynak sozlugu ve sinirli ifade diliyle dogrular.
    private static void EnsurePathsAreKnown(IEnumerable<EvidencePathDefinition> paths)
    {
        var invalid = paths.Any(path =>
            path.Steps.Any(step =>
                !PtnNodeKindCodes.All.Contains(step.NodeKindCode) ||
                !PtnEvidenceSourceCodes.All.Contains(step.SourceCode)) ||
            !IsAllowedConfirmedExpression(path.ConfirmedWhen) ||
            !Regex.IsMatch(path.InconclusiveWhen, PtnEvidenceExpressionPatterns.Unavailable));
        if (invalid)
        {
            throw new BusinessException(TestModuleBridgeErrorCodes.ProfilePackInvalid);
        }
    }

    // Confirmed ifadesini yalniz observed ve containsAny atomlarinin AND birlesimine sinirlar.
    private static bool IsAllowedConfirmedExpression(string expression)
    {
        var atoms = expression.Split(
            PtnEvidenceExpressionPatterns.AndSeparator,
            StringSplitOptions.RemoveEmptyEntries);
        return atoms.Length > 0 && atoms.All(atom =>
            Regex.IsMatch(atom, PtnEvidenceExpressionPatterns.Observed) ||
            Regex.IsMatch(atom, PtnEvidenceExpressionPatterns.ContainsAny));
    }

    // Sema muhru degismisse daha once onaylanan tum baglamalari yeniden onaya dusurur.
    private static void DowngradeDriftedBindings(ProfilePack pack, string currentFingerprint)
    {
        if (pack.DbSchemaFingerprint == currentFingerprint)
        {
            return;
        }

        foreach (var binding in pack.Bindings.Where(item => item.StateCode == PtnBindingStateCodes.Approved))
        {
            binding.StateCode = PtnBindingStateCodes.Proposed;
            binding.ApprovedBy = null;
        }
    }

    // Sirali kapsam listelerini sayi ve oran alanlariyla veri modeline yerlestirir.
    private static CoverageReport CreateCoverage(
        List<string> required,
        List<string> bound,
        List<string> unbound)
    {
        return new CoverageReport
        {
            RequiredConcepts = required,
            BoundConcepts = bound,
            UnboundConcepts = unbound,
            BoundCount = bound.Count,
            RequiredCount = required.Count,
            BoundRatio = required.Count == 0 ? 1m : (decimal)bound.Count / required.Count
        };
    }
}

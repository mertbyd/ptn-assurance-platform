using Ptn.TestModule.Documents.Bridge.Profiles;
using Ptn.TestModule.Models.Bridge;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Bridge.Profiles;

// islevi: YAML transport belgelerini Bridge profil modellerine compile-time esler.
// sistemdeki gorevi: Provider icindeki elle property kopyalamayi kaldirir ve hedef alan tamligini derlemede denetler.
[Mapper]
internal partial class ProfilePackMapper
{
    [MapperIgnoreTarget(nameof(PtnProfilePack.ContentFingerprint))]
    public partial PtnProfilePack Map(ProfilePackDocument document);

    private partial PtnConceptBinding MapBinding(ConceptBindingDocument document);

    private partial PtnEvidencePathDefinition MapPath(EvidencePathDocument document);

    private partial PtnEvidencePathTrigger MapTrigger(EvidenceTriggerDocument document);

    [MapProperty(nameof(EvidenceStepDocument.NodeKind), nameof(PtnEvidencePathStep.NodeKindCode))]
    [MapProperty(nameof(EvidenceStepDocument.Source), nameof(PtnEvidencePathStep.SourceCode))]
    [MapProperty(nameof(EvidenceStepDocument.Concept), nameof(PtnEvidencePathStep.ConceptCode))]
    [MapProperty(nameof(EvidenceStepDocument.JoinFrom), nameof(PtnEvidencePathStep.JoinFromNodeKindCode))]
    private partial PtnEvidencePathStep MapStep(EvidenceStepDocument document);
}

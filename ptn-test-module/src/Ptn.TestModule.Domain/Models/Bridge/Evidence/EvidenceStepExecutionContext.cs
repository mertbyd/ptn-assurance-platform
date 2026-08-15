using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Tek kanit adiminin profil, yol, tuple ve onceki dugum girdilerini tasir.
// sistemdeki gorevi: EvidenceChainManager yurutme durumunu nested tip olmadan adlandirilmis veri kabugunda toplar.
internal sealed class EvidenceStepExecutionContext
{
    public ProfilePack Pack { get; }
    public EvidencePathDefinition Path { get; }
    public EvidencePathStep Step { get; }
    public AccessTuple Tuple { get; }
    public IReadOnlyList<ExplanationNode> Nodes { get; }

    public EvidenceStepExecutionContext(
        ProfilePack pack,
        EvidencePathDefinition path,
        EvidencePathStep step,
        AccessTuple tuple,
        IReadOnlyList<ExplanationNode> nodes)
    {
        Pack = pack;
        Path = path;
        Step = step;
        Tuple = tuple;
        Nodes = nodes;
    }
}

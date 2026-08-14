using System.Collections.Generic;

namespace Ptn.TestModule.Models.Bridge;

// islevi: Tek kanit adiminin profil, yol, tuple ve onceki dugum girdilerini tasir.
// sistemdeki gorevi: EvidenceChainManager yurutme durumunu nested tip olmadan adlandirilmis veri kabugunda toplar.
internal sealed class PtnEvidenceStepExecutionContext
{
    public PtnProfilePack Pack { get; }
    public PtnEvidencePathDefinition Path { get; }
    public PtnEvidencePathStep Step { get; }
    public PtnAccessTuple Tuple { get; }
    public IReadOnlyList<PtnExplanationNode> Nodes { get; }

    public PtnEvidenceStepExecutionContext(
        PtnProfilePack pack,
        PtnEvidencePathDefinition path,
        PtnEvidencePathStep step,
        PtnAccessTuple tuple,
        IReadOnlyList<PtnExplanationNode> nodes)
    {
        Pack = pack;
        Path = path;
        Step = step;
        Tuple = tuple;
        Nodes = nodes;
    }
}

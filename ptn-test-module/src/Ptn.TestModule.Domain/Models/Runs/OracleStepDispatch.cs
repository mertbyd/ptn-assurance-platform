using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Runs;

// islevi: Bir HAR adiminin dogrudan hukum veya API checker gozlemi olarak nasil islenecegini tasir.
// sistemdeki gorevi: Hakem secimini Application servisinden OracleDispatchManager'a tasir.
public sealed class OracleStepDispatch
{
    public HarEntryModel Entry { get; set; } = new();

    public ResponseObservation? Observation { get; set; }

    public StepJudgement? DirectJudgement { get; set; }
}

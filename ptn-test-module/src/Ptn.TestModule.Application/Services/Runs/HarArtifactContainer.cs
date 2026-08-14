using Ptn.TestModule.Constants.Runs;
using Volo.Abp.BlobStoring;

namespace Ptn.TestModule.Services.Runs;

// islevi: HAR artefaktlarinin yazildigi ABP BLOB Storing container'ini tipli olarak adlandirir.
// sistemdeki gorevi: Saglayici secimini host kompozisyonuna birakirken container adini modulde sabitler (ADR-0016 §H).
[BlobContainerName(HarArtifactConsts.ContainerName)]
public sealed class HarArtifactContainer
{
}

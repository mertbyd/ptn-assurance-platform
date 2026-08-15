using Volo.Abp.Application.Dtos;

namespace Ptn.TestModule.Dtos.Runs;

// islevi: Kosum listesi sayfalama girdisini tanimlar.
// sistemdeki gorevi: Liste ucunun tipli ve sinirli paging sozlesmesidir; agir rapor kolonlarini tasimaz.
public sealed class TestRunListInput : PagedResultRequestDto
{
}

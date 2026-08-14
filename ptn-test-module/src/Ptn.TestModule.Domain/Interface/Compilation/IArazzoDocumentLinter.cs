using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Compilation;

namespace Ptn.TestModule.Interface.Compilation;

// islevi: Arazzo belgesini resmi Redocly lint surec sinirinda dogrulayan capability'yi tanimlar.
// sistemdeki gorevi: Domain derleme kararini Docker ve process uygulamasindan ayirir.
public interface IArazzoDocumentLinter
{
    // Derlenmis belgeyi pinli resmi aracla dogrular ve kapali sema sonucunu dondurur.
    Task<ArazzoLintResult> LintAsync(string document, CancellationToken cancellationToken);
}

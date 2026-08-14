using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Interface.Compilation;
using Ptn.TestModule.Interface.Shared;
using Ptn.TestModule.Managers.Compilation;
using Ptn.TestModule.Models.Compilation;
using Volo.Abp.DependencyInjection;

namespace Ptn.TestModule.Services.Compilation;

// islevi: Manager'in kurdugu lint planini ortak surec sinirinda kosar ve yorumu yine Manager'a birakir.
// sistemdeki gorevi: Derleme kapisinin pinli araca ulasmasini saglayan ince Application orkestrasyonudur.
public sealed class RedoclyArazzoDocumentLinter : IArazzoDocumentLinter, ITransientDependency
{
    // Lint cagri planini ve ham surec cikisinin yorumunu sahiplenen Manager'dir.
    private readonly ArazzoLintManager _lintManager;

    // Ortak process mekanigini tasiyan surec siniridir.
    private readonly IProcessBoundaryPort _processBoundary;

    // Lint use-case'ini plan sahibi Manager ile ortak surec sinirina baglar.
    public RedoclyArazzoDocumentLinter(ArazzoLintManager lintManager, IProcessBoundaryPort processBoundary)
    {
        _lintManager = lintManager;
        _processBoundary = processBoundary;
    }

    // Plani alir, kosar ve sema hukmunu Manager'dan dondurur.
    public async Task<ArazzoLintResult> LintAsync(
        string document,
        CancellationToken cancellationToken)
    {
        return _lintManager.Interpret(await _processBoundary.ExecuteAsync(
            _lintManager.CreatePlan(document),
            cancellationToken));
    }
}

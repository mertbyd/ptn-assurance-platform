using System.Threading;
using System.Threading.Tasks;
using Ptn.DatabaseChecker.Application.Mappers.Diagnosis;
using Ptn.DatabaseChecker.Dtos.Diagnosis;
using Ptn.DatabaseChecker.Interface.Connections;
using Ptn.DatabaseChecker.Managers.Diagnosis;
using Ptn.DatabaseChecker.Services.Diagnosis;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Ptn.DatabaseChecker.Application.Services.Diagnosis;

// islevi: Tek dinamik failure diagnosis use-case'ini kayitli baglanti uzerinden orkestre eder.
// sistemdeki gorevi: Yetki -> baglanti oku -> Mapperly signal -> DiagnosisManager -> Mapperly RFC DTO zinciridir; hedef I/O acik UOW tutmaz.
[RemoteService(IsEnabled = false)]
[UnitOfWork(IsDisabled = true)]
public class DiagnosisAppService : DatabaseCheckerAppService, IDiagnosisAppService
{
    private static readonly DiagnosisMapper Mapper = new();

    private DiagnosisManager Manager
        => LazyServiceProvider.LazyGetRequiredService<DiagnosisManager>();

    private IDatabaseConnectionRepository ConnectionRepository
        => LazyServiceProvider.LazyGetRequiredService<IDatabaseConnectionRepository>();

    // islevi: DTO'yu sinyale map eder, katalog-resolved teshisi calistirir ve RFC cevap DTO'suna map eder.
    public virtual async Task<DiagnosisReportDto> DiagnoseAsync(
        DiagnoseRequestDto input,
        CancellationToken cancellationToken = default)
    {
        var connection = await ConnectionRepository.GetWithDetailsAsync(input.ConnectionId, cancellationToken);
        var signal = Mapper.MapToSignal(input);
        var report = await Manager.DiagnoseAsync(connection, signal, cancellationToken);
        return Mapper.MapToDto(report);
    }
}

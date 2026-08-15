using System;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Querying;
using Ptn.TestModule.Constants.Runs;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.Entities.Runs;
using Ptn.TestModule.ExceptionCodes.Runs;
using Ptn.TestModule.Interface.Lookups;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;

namespace Ptn.TestModule.Managers.Runs;

// islevi: Running kosumlarin kooperatif iptal talebi kuralini sahiplenir.
// sistemdeki gorevi: HTTP talebi ile job kontrol noktalarini ExtraProperties kaydi uzerinden baglar.
public sealed class RunCancellationManager : TestModuleDomainService
{
    private readonly ITestRunStatusRepository _statusRepository;

    public RunCancellationManager(ITestRunStatusRepository statusRepository)
    {
        _statusRepository = statusRepository;
    }

    public async Task RequestAsync(TestRun entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var running = await _statusRepository.FindAsync(
            new RepositoryQuery<TestRunStatus>().Where(item => item.Code == TestRunStatusCodes.Running),
            cancellationToken);
        if (running is null)
        {
            throw new EntityNotFoundException(typeof(TestRunStatus));
        }

        if (entity.RunStatusId != running.Id)
        {
            throw new BusinessException(TestModuleRunErrorCodes.RunCancellationRejected);
        }

        entity.SetProperty(RunArtifactConsts.CancellationRequestedProperty, true);
    }

    public bool IsRequested(TestRun entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.GetProperty<bool>(RunArtifactConsts.CancellationRequestedProperty);
    }
}

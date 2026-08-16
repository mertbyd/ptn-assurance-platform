using Microsoft.EntityFrameworkCore;
using Ptn.ApiContractChecker.Entities.Lookups;
using Ptn.ApiContractChecker.Entities.Runs;
using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Entities.Sources;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Ptn.ApiContractChecker.EntityFrameworkCore;

// islevi: Uygulama DbContext'inin repository ve seed tarafina acilan sozlesmesini tanimlar.
// sistemdeki gorevi: Katmanlarin somut DbContext tipine degil bu arayuze bagli kalmasini saglar.
[ConnectionStringName(ApiContractCheckerDbProperties.ConnectionStringName)]
public interface IApiContractCheckerDbContext : IEfCoreDbContext
{
    DbSet<SpecSource> SpecSources { get; }
    DbSet<SpecDocument> SpecDocuments { get; }
    DbSet<SpecContent> SpecContents { get; }
    DbSet<SpecSnapshot> SpecSnapshots { get; }
    DbSet<ContractCheckRun> ContractCheckRuns { get; }
    DbSet<SpecFormat> SpecFormats { get; }
    DbSet<CheckRunStatus> CheckRunStatuses { get; }
    DbSet<DifferenceSeverity> DifferenceSeverities { get; }
    DbSet<DifferenceDirection> DifferenceDirections { get; }
    DbSet<DifferenceKind> DifferenceKinds { get; }
}

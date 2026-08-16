using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Entities.Lookups;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: CheckRunStatus lookup'unu kararli tablo sabitine baglar.
// sistemdeki gorevi: Ortak lookup EF eslemesini run durumlari tablosuna uygular.
public class CheckRunStatusConfiguration : LookupEntityConfiguration<CheckRunStatus>
{
    protected override string TableName => ApiContractCheckerTableNames.CheckRunStatuses;
}

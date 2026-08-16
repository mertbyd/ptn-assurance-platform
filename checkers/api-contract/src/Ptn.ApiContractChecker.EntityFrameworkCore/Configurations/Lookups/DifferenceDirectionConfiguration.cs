using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Entities.Lookups;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: DifferenceDirection lookup'unu kararli tablo sabitine baglar.
// sistemdeki gorevi: Ortak lookup EF eslemesini fark yonleri tablosuna uygular.
public class DifferenceDirectionConfiguration : LookupEntityConfiguration<DifferenceDirection>
{
    protected override string TableName => ApiContractCheckerTableNames.DifferenceDirections;
}

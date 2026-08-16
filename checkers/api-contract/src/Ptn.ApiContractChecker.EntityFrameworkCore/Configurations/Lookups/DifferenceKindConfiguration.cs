using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Entities.Lookups;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: DifferenceKind lookup'unu kararli tablo sabitine baglar.
// sistemdeki gorevi: Ortak lookup EF eslemesini kapali fark turleri tablosuna uygular.
public class DifferenceKindConfiguration : LookupEntityConfiguration<DifferenceKind>
{
    protected override string TableName => ApiContractCheckerTableNames.DifferenceKinds;
}

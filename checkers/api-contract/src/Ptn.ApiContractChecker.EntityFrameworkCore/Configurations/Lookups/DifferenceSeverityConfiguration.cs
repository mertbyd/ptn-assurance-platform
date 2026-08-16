using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Entities.Lookups;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: DifferenceSeverity lookup'unu kararli tablo sabitine baglar.
// sistemdeki gorevi: Ortak lookup EF eslemesini fark siddetleri tablosuna uygular.
public class DifferenceSeverityConfiguration : LookupEntityConfiguration<DifferenceSeverity>
{
    protected override string TableName => ApiContractCheckerTableNames.DifferenceSeverities;
}

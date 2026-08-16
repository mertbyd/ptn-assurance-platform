using Ptn.ApiContractChecker.Constants;
using Ptn.ApiContractChecker.Entities.Lookups;

namespace Ptn.ApiContractChecker.EntityFrameworkCore.Configurations.Lookups;

// islevi: SpecFormat lookup'unu kararli tablo sabitine baglar.
// sistemdeki gorevi: Ortak lookup EF eslemesini spec formatlari tablosuna uygular.
public class SpecFormatConfiguration : LookupEntityConfiguration<SpecFormat>
{
    protected override string TableName => ApiContractCheckerTableNames.SpecFormats;
}

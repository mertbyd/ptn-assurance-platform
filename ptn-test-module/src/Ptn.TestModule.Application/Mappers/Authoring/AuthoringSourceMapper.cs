using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.Models.Bridge;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Authoring;

// islevi: Yazarlik malzemesi ozet modellerinin public DTO eslemelerini tanimlar.
// sistemdeki gorevi: Listeleme ucundaki alan kopyalamayi AppService yerine Mapperly'ye birakir.
[Mapper]
public partial class AuthoringSourceMapper
{
    public partial AuthoringSourceDto Map(AuthoringSourceSeal source);
    public partial ProfilePackSummaryDto Map(ProfilePackSummary source);
}

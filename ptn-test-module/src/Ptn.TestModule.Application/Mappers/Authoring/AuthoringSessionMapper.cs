using Ptn.TestModule.Dtos.Authoring;
using Ptn.TestModule.Models.Authoring;
using Riok.Mapperly.Abstractions;

namespace Ptn.TestModule.Mappers.Authoring;

// islevi: Authoring DTO, domain girdi ve cache session gorunumu eslemelerini tanimlar.
// sistemdeki gorevi: AppService'teki tum katmanlar-arasi alan kopyalamayi Mapperly'ye birakir.
[Mapper]
public partial class AuthoringSessionMapper
{
    public partial AuthoringSessionCreateModel Map(CreateAuthoringSessionDto source);
    public partial AuthoringAnswerModel Map(AnswerAuthoringSessionDto source);
    public partial AuthoringStepModel Map(AddAuthoringStepDto source);
    public partial AuthoringDatabaseStep Map(AddDatabaseAuthoringStepDto source);
    public partial AuthoringSessionDto Map(AuthoringSession source);
}

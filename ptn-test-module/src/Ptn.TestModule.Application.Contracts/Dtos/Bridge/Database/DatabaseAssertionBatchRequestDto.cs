namespace Ptn.TestModule.Dtos.Bridge.Database;

// islevi: Birlikte denetlenecek database assertion isteklerini tasir.
// sistemdeki gorevi: Batch girdisine FluentValidation ve kararli navigation siniri verir.
public sealed class DatabaseAssertionBatchRequestDto
{
    public List<DatabaseAssertionRequestDto> Requests { get; set; } = [];
}

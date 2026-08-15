using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Bir kanitin geldigi checker veya kopru kaynagini kapali kodlarla tanimlar.
// sistemdeki gorevi: Fingerprint kimligini kaynak checker ile ayrilmaz bir cift olarak tasir.
public static class PtnSourceCheckerCodes
{
    public const string ApiContract = nameof(ApiContract);
    public const string DatabaseComparison = nameof(DatabaseComparison);
    public const string Runner = nameof(Runner);
    public const string Bridge = nameof(Bridge);

    public static IReadOnlyCollection<string> All { get; } =
        [ApiContract, DatabaseComparison, Runner, Bridge];
}

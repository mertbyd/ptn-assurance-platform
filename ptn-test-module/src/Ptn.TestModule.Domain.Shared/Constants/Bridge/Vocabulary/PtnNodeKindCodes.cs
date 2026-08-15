using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Veri gudumlu kanit yolunda kaydedilebilen dugum turlerini tanimlar.
// sistemdeki gorevi: Aciklama agacini serbest metinden ve vaka-ozel siniflardan bagimsiz tutar.
public static class PtnNodeKindCodes
{
    public const string ScopeRequired = nameof(ScopeRequired);
    public const string SubjectResolved = nameof(SubjectResolved);
    public const string RoleHeld = nameof(RoleHeld);
    public const string GrantMatched = nameof(GrantMatched);
    public const string OperationBound = nameof(OperationBound);
    public const string RequestExampleBuilt = nameof(RequestExampleBuilt);
    public const string TableDescribed = nameof(TableDescribed);
    public const string KeyUnique = nameof(KeyUnique);
    public const string AssertionDerivable = nameof(AssertionDerivable);
    public const string FootprintObserved = nameof(FootprintObserved);

    public static IReadOnlyCollection<string> All { get; } =
    [
        ScopeRequired, SubjectResolved, RoleHeld, GrantMatched, OperationBound,
        RequestExampleBuilt, TableDescribed, KeyUnique, AssertionDerivable, FootprintObserved
    ];
}

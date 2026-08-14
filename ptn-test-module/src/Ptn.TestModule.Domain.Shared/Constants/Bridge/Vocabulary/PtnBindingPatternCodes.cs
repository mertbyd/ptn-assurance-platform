using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Vocabulary;

// islevi: Profil paketinin destekledigi kapali semantik baglama desenlerini tanimlar.
// sistemdeki gorevi: Kavram-tablo bagini serbest metin yerine onaylanabilir desen koduna baglar.
public static class PtnBindingPatternCodes
{
    public const string SemanticEntity = "SE";
    public const string SemanticRelation = "SR";
    public const string SemanticRoleAssignment = "SRa";
    public const string SemanticRoleRelation = "SRR";
    public const string SemanticHierarchy = "SH";

    public static IReadOnlyCollection<string> All { get; } =
        [SemanticEntity, SemanticRelation, SemanticRoleAssignment, SemanticRoleRelation, SemanticHierarchy];
}

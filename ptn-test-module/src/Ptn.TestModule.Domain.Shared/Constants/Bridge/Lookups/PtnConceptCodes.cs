using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Bridge.Lookups;

// islevi: Profil paketinin somut semaya baglayabildigi kapali is kavramlarini tanimlar.
// sistemdeki gorevi: Ajanin tablo veya kolon adi tahmin etmesi yerine onayli kavram secmesini saglar.
public static class PtnConceptCodes
{
    public const string Subject = nameof(Subject);
    public const string RoleAssignment = nameof(RoleAssignment);
    public const string PermissionGrant = nameof(PermissionGrant);
    public const string Resource = nameof(Resource);
    public const string ResourceOwnership = nameof(ResourceOwnership);
    public const string TimeAnchor = nameof(TimeAnchor);
    public const string Quota = nameof(Quota);

    public static IReadOnlyCollection<string> All { get; } =
        [Subject, RoleAssignment, PermissionGrant, Resource, ResourceOwnership, TimeAnchor, Quota];
}

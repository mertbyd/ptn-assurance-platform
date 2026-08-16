namespace Ptn.ApiContractChecker.Constants;

// islevi: Uygulama JSON sozlesmesindeki kararli metadata alanlarini tanimlar.
// sistemdeki gorevi: Bildirim polymorphism sozlesmesinin serializer kullanimlarinda dagilmasini engeller.
public static class ApiContractCheckerSerializationConstants
{
    public const string TypeDiscriminatorPropertyName = "$type";
}

namespace Ptn.ApiContractChecker.Constants.Diagnosis;

// islevi: KBP-622 v1 API teshis hipotezlerinin kapali kod katalogunu tanimlar.
// sistemdeki gorevi: Rule siniflari, localization ve composition host arasinda metinden bagimsiz kimlik saglar.
public static class HypothesisKindCodes
{
    public const string ResponseSchemaChanged = "H-CD-01";
    public const string RequiredRequestFieldCreated = "H-CD-02";
    public const string EnumValueRemoved = "H-CD-03";
    public const string EndpointRemovedOrMoved = "H-CD-04";
    public const string SuccessStatusChanged = "H-CD-05";
    public const string MediaTypeRemoved = "H-CD-06";
    public const string PropertyBecameOptional = "H-CD-07";
    public const string ResourceNeverCreated = "H-ST-01";
    public const string ResourceCreatedLate = "H-ST-02";
    public const string AuthenticationMissing = "H-AU-01";
    public const string TokenExpired = "H-AU-02";
    public const string InsufficientScope = "H-AU-03";
    public const string PathNotDeployed = "H-EN-01";
    public const string MethodNotSupported = "H-EN-02";
    public const string SnapshotVersionMismatch = "H-EN-04";
    public const string AssertionValueDiffers = "H-AS-01";
    public const string AssertionRequiredFieldMissing = "H-AS-02";
    public const string AssertionOutsideContract = "H-AS-03";
    public const string VolatileLiteralAssertion = "H-AS-04";
}

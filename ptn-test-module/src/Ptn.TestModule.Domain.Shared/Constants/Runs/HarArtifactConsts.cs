using System;
using System.Collections.Generic;

namespace Ptn.TestModule.Constants.Runs;

// islevi: HAR artefaktinin BLOB container adini, blob adi bicimini ve boyut butcelerini tanimlar.
// sistemdeki gorevi: Satirda yalniz har_blob_name birakan artefakt sozlesmesini tek Domain.Shared sahibinde tutar (ADR-0016 §H).
/// <summary>
/// HAR artefakt deposunun kararli container, ad ve boyut sabitlerini tasir.
/// </summary>
public static class HarArtifactConsts
{
    /// <summary>HAR artefaktlarinin yazildigi ABP BLOB Storing container adidir.</summary>
    public const string ContainerName = "test-module-har";

    /// <summary>Blob adinin tenant, kosum ve trace kimliginden turetildigi bicimdir.</summary>
    public const string BlobNameFormat = "{0}/{1}/{2}.har";

    /// <summary>Tenant kimligi bulunmayan kosumlarin blob adi bolumudur.</summary>
    public const string HostTenantSegment = "host";

    /// <summary>Bir kosumun uretebilecegi azami HAR artefakt boyutudur.</summary>
    public const int MaxHarBytes = 33_554_432;

    /// <summary>Kanitin satir icinde tutulabilecegi azami bayt sayisidir; ustu bloba gider.</summary>
    public const int MaxInlineEvidenceBytes = 4_096;

    /// <summary>Maskelenen basliklarin yerine artefakta yazilan sabit degerdir.</summary>
    public const string RedactedValue = "[redacted]";

    /// <summary>Degeri kalici artefakta hicbir zaman yazilmayan kimlik basliklaridir.</summary>
    public static IReadOnlyCollection<string> SensitiveHeaderNames { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization",
            "Proxy-Authorization",
            "Cookie",
            "Set-Cookie",
            "X-Api-Key",
            "Api-Key",
            "X-Auth-Token"
        };

    // islevi: Okuyucunun HAR 1.2 govdesinde adresledigi alan adlarini merkezilestirir.
    /// <summary>HAR 1.2 belgesinin kararli JSON alan adlarini tasir.</summary>
    public static class Fields
    {
        public const string Log = "log";
        public const string Creator = "creator";
        public const string Name = "name";
        public const string Version = "version";
        public const string Entries = "entries";
        public const string StartedDateTime = "startedDateTime";
        public const string Time = "time";
        public const string Request = "request";
        public const string Response = "response";
        public const string Headers = "headers";
        public const string Cookies = "cookies";
        public const string Method = "method";
        public const string Url = "url";
        public const string Value = "value";
        public const string Status = "status";
        public const string PostData = "postData";
        public const string Content = "content";
        public const string MimeType = "mimeType";
        public const string Text = "text";
    }
}

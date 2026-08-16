using Ptn.ApiContractChecker.Managers.Snapshots;
using Shouldly;
using Xunit;

namespace Ptn.ApiContractChecker.Snapshots;

// islevi: SpecIngestionManager'in SpecContent hash, medya tipi ve boyut kurallarini dogrular.
// sistemdeki gorevi: Icerik-adresli dedup anahtarinin farkli yazimlarla ayrismasini engeller.
public class SpecContent_Tests
{
    // Hashleri ve medya tipini tenant unique indexiyle uyumlu kanonik bicime indirger.
    [Fact]
    public void Constructor_Should_Normalize_Content_Identity()
    {
        var uppercaseHash = new string('A', 64);

        var content = CreateManager().CreateContent(
            Guid.NewGuid(),
            uppercaseHash,
            uppercaseHash,
            "{}",
            2,
            " Application/JSON ",
            Guid.NewGuid());

        content.RawHash.ShouldBe(new string('a', 64));
        content.CanonicalHash.ShouldBe(new string('a', 64));
        content.MediaType.ShouldBe("application/json");
    }

    // SHA-256 olmayan ham hash degerini kalici modele girmeden reddeder.
    [Fact]
    public void Constructor_Should_Reject_An_Invalid_Hash()
    {
        Should.Throw<ArgumentException>(() => CreateManager().CreateContent(
            Guid.NewGuid(),
            "not-a-sha256",
            new string('a', 64),
            "{}",
            2,
            "application/json",
            null));
    }

    // Saf kurulus davranislarini veri erisimi olmadan test edecek manager ornegini kurar.
    private static SpecIngestionManager CreateManager()
        => new(null!);
}

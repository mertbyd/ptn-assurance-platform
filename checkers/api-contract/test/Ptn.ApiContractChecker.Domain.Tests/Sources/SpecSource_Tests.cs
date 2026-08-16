using Ptn.ApiContractChecker.Constants.Sources;
using Ptn.ApiContractChecker.ExceptionCodes.Sources;
using Ptn.ApiContractChecker.Managers.Sources;
using Ptn.ApiContractChecker.Models.Sources;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.ApiContractChecker.Sources;

// islevi: SpecSourceManager aggregate kurulum, dokuman sahipligi ve pasiflestirme davranislarini dogrular.
// sistemdeki gorevi: Ince entity'lerde kaybolabilecek kaynak ve dokuman davranislarini manager seviyesinde sabitler.
public class SpecSource_Tests
{
    // Kaynak adresini tekillestirir ve dokuman cocuguna ayni tenant sahipligini aktarir.
    [Fact]
    public void AddDocument_Should_Keep_The_Aggregate_Tenant_Boundary()
    {
        var manager = CreateManager();
        var tenantId = Guid.NewGuid();
        var source = CreateSource(manager, baseUrl: "https://orders.test/", tenantId: tenantId);

        var document = manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");

        source.BaseUrl.ShouldBe("https://orders.test");
        document.SpecSourceId.ShouldBe(source.Id);
        document.TenantId.ShouldBe(tenantId);
        source.Documents.ShouldContain(document);
    }

    // Dokuman adi kucuk-buyuk harf farkiyla tekrarlandiginda aggregate kuralini reddeder.
    [Fact]
    public void AddDocument_Should_Reject_A_Duplicate_Name()
    {
        var manager = CreateManager();
        var source = CreateSource(manager);
        manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");

        var exception = Should.Throw<BusinessException>(() =>
            manager.AddDocument(source, Guid.NewGuid(), "V1", "/openapi/v2.json"));

        exception.Code.ShouldBe(SpecSourceExceptionCodes.DocumentNameAlreadyExists);
    }

    // Kaynak pasiflestiginde kimligi ve dokuman gecmisi korunur.
    [Fact]
    public void Passivate_Should_Not_Remove_Documents()
    {
        var manager = CreateManager();
        var source = CreateSource(manager);
        manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");

        manager.Passivate(source);

        source.IsActive.ShouldBeFalse();
        source.Documents.Count.ShouldBe(1);
    }

    // Yeni dokumanin zamanlanmis taramaya kendiliginden girmedigini kanitlar.
    [Fact]
    public void AddDocument_Should_Not_Enable_Monitoring()
    {
        var manager = CreateManager();
        var source = CreateSource(manager);

        var document = manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");

        document.IsMonitored.ShouldBeFalse();
        document.CheckIntervalMinutes.ShouldBeNull();
        document.NextCheckAt.ShouldBeNull();
    }

    // Izleme acildiginda ilk taramanin vadesinin hemen geldigini kanitlar.
    [Fact]
    public void ConfigureDocumentMonitoring_Should_Open_The_First_Due_Date()
    {
        var manager = CreateManager();
        var now = new DateTime(2026, 8, 6, 9, 0, 0, DateTimeKind.Utc);
        var source = CreateSource(manager);
        var document = manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");

        manager.ConfigureDocumentMonitoring(source, document.Id, true, 15, now);

        document.IsMonitored.ShouldBeTrue();
        document.CheckIntervalMinutes.ShouldBe(15);
        document.NextCheckAt.ShouldBe(now);
    }

    // Sinir disi araligin manager savunmasinda reddedildigini kanitlar.
    [Theory]
    [InlineData(null)]
    [InlineData(SpecDocumentConsts.MinCheckIntervalMinutes - 1)]
    [InlineData(SpecDocumentConsts.MaxCheckIntervalMinutes + 1)]
    public void ConfigureDocumentMonitoring_Should_Reject_An_Interval_Outside_The_Contract(int? interval)
    {
        var manager = CreateManager();
        var source = CreateSource(manager);
        var document = manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");

        Should.Throw<ArgumentOutOfRangeException>(() =>
            manager.ConfigureDocumentMonitoring(source, document.Id, true, interval, DateTime.UtcNow));
    }

    // Izleme kapatildiginda vadenin dustugunu ama gecmis denemenin korundugunu kanitlar.
    [Fact]
    public void ConfigureDocumentMonitoring_Should_Clear_The_Schedule_But_Keep_History()
    {
        var manager = CreateManager();
        var now = new DateTime(2026, 8, 6, 9, 0, 0, DateTimeKind.Utc);
        var source = CreateSource(manager);
        var document = manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");
        manager.ConfigureDocumentMonitoring(source, document.Id, true, 15, now);
        manager.MarkDocumentChecked(source, document.Id, now);

        manager.ConfigureDocumentMonitoring(source, document.Id, false, null, now);

        document.IsMonitored.ShouldBeFalse();
        document.NextCheckAt.ShouldBeNull();
        document.CheckIntervalMinutes.ShouldBeNull();
        document.LastCheckedAt.ShouldBe(now);
    }

    // Kontrol denemesinin vadeyi tam bir aralik kadar ilerlettigini kanitlar.
    [Fact]
    public void MarkDocumentChecked_Should_Advance_The_Due_Date_By_One_Interval()
    {
        var manager = CreateManager();
        var now = new DateTime(2026, 8, 6, 9, 0, 0, DateTimeKind.Utc);
        var source = CreateSource(manager);
        var document = manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");
        manager.ConfigureDocumentMonitoring(source, document.Id, true, 15, now);

        manager.MarkDocumentChecked(source, document.Id, now);

        document.LastCheckedAt.ShouldBe(now);
        document.NextCheckAt.ShouldBe(now.AddMinutes(15));
    }

    // Kaynak ad ve path guncellemesinin izleme tercihini sessizce sifirlamadigini kanitlar.
    [Fact]
    public void UpdateDocument_Should_Not_Reset_Monitoring()
    {
        var manager = CreateManager();
        var now = new DateTime(2026, 8, 6, 9, 0, 0, DateTimeKind.Utc);
        var source = CreateSource(manager);
        var document = manager.AddDocument(source, Guid.NewGuid(), "v1", "/openapi/v1.json");
        manager.ConfigureDocumentMonitoring(source, document.Id, true, 15, now);

        manager.UpdateDocument(source, document.Id, "v1-renamed", "/openapi/v1-renamed.json");

        document.DocumentName.ShouldBe("v1-renamed");
        document.IsMonitored.ShouldBeTrue();
        document.CheckIntervalMinutes.ShouldBe(15);
        document.NextCheckAt.ShouldBe(now);
    }

    // Kaynak tanimini manager kanoniklestirmesiyle ve dokumansiz olarak kurar.
    private static Ptn.ApiContractChecker.Entities.Sources.SpecSource CreateSource(
        SpecSourceManager manager,
        string baseUrl = "https://orders.test",
        Guid? tenantId = null)
    {
        return manager.Create(Guid.NewGuid(), new CreateSpecSourceModel
        {
            Name = "Orders",
            BaseUrl = baseUrl
        }, tenantId);
    }

    // Saf manager davranis testlerinde veri erisimi kullanmayan ornegi kurar.
    private static SpecSourceManager CreateManager()
        => new(null!, null!);
}

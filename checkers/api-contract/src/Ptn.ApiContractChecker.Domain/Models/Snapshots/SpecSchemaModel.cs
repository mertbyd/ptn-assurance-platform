namespace Ptn.ApiContractChecker.Models.Snapshots;

// islevi: Bir component veya inline semanin tip, ozellik ve birlesim yapisini tasir.
// sistemdeki gorevi: OAS surumlerinden bagimsiz sema diff'inin ve allOf duzlestirmesinin kanonik girdisidir.
public class SpecSchemaModel
{
    // Component semasinin adi; inline parcalarda bos olabilir.
    public string Name { get; set; } = string.Empty;

    // Sema bir component'e bagliysa bulgu adresi icin korunacak referans kimligi.
    public string? ReferenceId { get; set; }

    // Component semasinin x-internal true ile dis kullanima kapali isaretlenip isaretlenmedigi.
    public bool IsInternal { get; set; }

    // Semanin ham tip ifadesi; normalizer null birlesimini ayirir ve kararli siraya sokar.
    public string? Type { get; set; }

    // Semanin null deger kabul edip etmedigi.
    public bool Nullable { get; set; }

    // Semanin format anahtar sozcugu.
    public string? Format { get; set; }

    // String uzunluk ve desen kisitlari.
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; }

    // Sayisal alt ve ust sinirlar.
    public decimal? Minimum { get; set; }
    public decimal? Maximum { get; set; }

    // Dizi boyut ve tekillik kisitlari.
    public int? MinItems { get; set; }
    public int? MaxItems { get; set; }
    public bool UniqueItems { get; set; }

    // Dizi eleman semasi.
    public SpecSchemaModel? Items { get; set; }

    // Belgesiz property kabul karari ve varsa bu property'lerin semasi.
    public bool AllowAdditionalProperties { get; set; } = true;
    public SpecSchemaModel? AdditionalProperties { get; set; }

    // Sema dogrudan enum tanimliyorsa kabul edilen kararli degerler.
    public List<string> EnumValues { get; set; } = new();

    // Semanin alan sozlesmeleri.
    public List<SpecSchemaPropertyModel> Properties { get; set; } = new();

    // Normalizer tarafindan ana semaya duzlestirilecek allOf parcalari.
    public List<SpecSchemaModel> AllOf { get; set; } = new();

    // Runtime validator'a korunarak tasinacak alternatif ve dislama semalari.
    public List<SpecSchemaModel> AnyOf { get; set; } = new();
    public List<SpecSchemaModel> OneOf { get; set; } = new();
    public SpecSchemaModel? Not { get; set; }
}

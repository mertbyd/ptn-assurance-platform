namespace Ptn.DatabaseChecker.Constants.Comparison;

// islevi: Veritabani nesne adlarinin (sema/nesne/parca/kolon) uzunluk sinirlarini tek kaynakta tanimlar.
// sistemdeki gorevi: Owned kapsam/bulgu modelleri (ComparisonScopeRule, SchemaDifferenceModel, DataValueDifferenceModel) ayni ad alanlarini tasir; kapsam-kurali FluentValidation ayni sabiti kullanir, kural bir kez yazilir (golden rule 1).
public static class SchemaObjectConsts
{
    // Sema adinin azami uzunlugu (public, dbo, muhasebe...).
    public const int MaxSchemaNameLength = 128;

    // Nesne (tablo/view/index...) adinin azami uzunlugu.
    public const int MaxObjectNameLength = 256;

    // Nesne icindeki parcanin (kolon/index adi) azami uzunlugu.
    public const int MaxChildNameLength = 256;

    // Kolon adinin azami uzunlugu (DataValueDifferenceModel.ColumnName).
    public const int MaxColumnNameLength = 128;
}

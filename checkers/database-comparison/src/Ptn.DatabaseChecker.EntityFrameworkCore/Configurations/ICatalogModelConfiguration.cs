namespace Ptn.DatabaseChecker.EntityFrameworkCore.Configurations;

// islevi: External veritabani katalog okuma context'lerine ait EF Core konfigurasyonlarini isaretler.
// sistemdeki gorevi: Ana DatabaseCheckerDbContext'in assembly taramasi bu isareti tasiyan konfigurasyonlari dislar; katalog context'leri ise ayni isaret ve provider namespace filtresiyle yalniz kendi mapping'lerini otomatik uygular.
internal interface ICatalogModelConfiguration
{
}

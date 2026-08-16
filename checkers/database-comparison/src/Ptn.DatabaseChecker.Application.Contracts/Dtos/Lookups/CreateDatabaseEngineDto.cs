namespace Ptn.DatabaseChecker.Dtos.Lookups;

// islevi: Veritabani motoru (PostgreSql/SqlServer) lookup'ina yeni satir ekleme isteginin API girdisidir.
// sistemdeki gorevi: Ortak alanlar LookupCreateDto'dan gelir; Id ve audit alanlari tasimaz.
public class CreateDatabaseEngineDto : LookupCreateDto
{
}

namespace Ptn.DatabaseChecker.Dtos.Lookups;

// islevi: Calistirma durumu (Pending/Running/Completed/Failed) lookup'ina yeni satir ekleme isteginin API girdisidir.
// sistemdeki gorevi: Ortak alanlar LookupCreateDto'dan gelir; Id ve audit alanlari tasimaz.
public class CreateComparisonRunStatusDto : LookupCreateDto
{
}

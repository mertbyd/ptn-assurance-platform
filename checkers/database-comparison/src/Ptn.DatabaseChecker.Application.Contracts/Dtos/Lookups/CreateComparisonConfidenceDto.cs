namespace Ptn.DatabaseChecker.Dtos.Lookups;

// islevi: Fark guveni (Exact/Canonical/Approximate/Incomparable) lookup'ina yeni satir ekleme isteginin API girdisidir.
// sistemdeki gorevi: Ortak alanlar LookupCreateDto'dan gelir; Id ve audit alanlari tasimaz.
public class CreateComparisonConfidenceDto : LookupCreateDto
{
}

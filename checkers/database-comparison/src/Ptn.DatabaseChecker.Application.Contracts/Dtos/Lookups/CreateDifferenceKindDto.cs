namespace Ptn.DatabaseChecker.Dtos.Lookups;

// islevi: Fark yonu (OnlyInSource/OnlyInTarget/Modified) lookup'ina yeni satir ekleme isteginin API girdisidir.
// sistemdeki gorevi: Ortak alanlar LookupCreateDto'dan gelir; Id ve audit alanlari tasimaz.
public class CreateDifferenceKindDto : LookupCreateDto
{
}

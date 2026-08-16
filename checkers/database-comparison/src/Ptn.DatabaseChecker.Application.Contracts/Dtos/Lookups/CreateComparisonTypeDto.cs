namespace Ptn.DatabaseChecker.Dtos.Lookups;

// islevi: Karsilastirma modu (SchemaOnly/DataOnly/Both) lookup'ina yeni satir ekleme isteginin API girdisidir.
// sistemdeki gorevi: Ortak alanlar LookupCreateDto'dan gelir; Id ve audit alanlari tasimaz.
public class CreateComparisonTypeDto : LookupCreateDto
{
}

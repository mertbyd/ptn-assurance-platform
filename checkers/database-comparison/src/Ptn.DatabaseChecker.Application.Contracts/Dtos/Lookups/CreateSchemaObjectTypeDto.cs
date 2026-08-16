namespace Ptn.DatabaseChecker.Dtos.Lookups;

// islevi: Sema nesne turu (Table/View/Column/...) lookup'ina yeni satir ekleme isteginin API girdisidir.
// sistemdeki gorevi: Ortak alanlar LookupCreateDto'dan gelir; Id ve audit alanlari tasimaz.
public class CreateSchemaObjectTypeDto : LookupCreateDto
{
}

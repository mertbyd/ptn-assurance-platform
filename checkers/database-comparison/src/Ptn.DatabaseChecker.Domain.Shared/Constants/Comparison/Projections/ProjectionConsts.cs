namespace Ptn.DatabaseChecker.Constants.Comparison.Projections;

// islevi: Salt-okunur projection isteklerinin satir ve kolon butcelerini sabitler.
// sistemdeki gorevi: Validator, Manager ve public sozlesmenin ayni fail-closed limitleri kullanmasini saglar.
public static class ProjectionConsts
{
    public const int DefaultMaxRows = 20;
    public const int MaxRowsCeiling = 100;
    public const int MaxProjectColumns = 50;
}

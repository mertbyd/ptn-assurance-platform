using System.Text.RegularExpressions;
using Ptn.DatabaseChecker.Constants.Comparison;
using Volo.Abp.DependencyInjection;

namespace Ptn.DatabaseChecker.Managers.Comparison;

// islevi: SQL tanim metinlerini karsilastirma oncesi yalanci whitespace/parantez farklarindan arindirir.
// sistemdeki gorevi: Diff motoru raw katalog metinlerini dogrudan kiyaslamak yerine bu tek normalizasyon noktasindan gecirir; provider repository'leri normalization kurali bilmez.
public class SchemaDefinitionNormalizer : ITransientDependency
{
    // Ardisik bosluk/satir sonlarini tek bosluga indiren ortak regex.
    private static readonly Regex WhitespaceRegex = new(
        SchemaComparisonTextConstants.Normalization.WhitespacePattern,
        RegexOptions.Compiled);

    // Noktalama/operator cevresindeki format bosluklarini yalanci fark olmaktan cikaran ortak regex.
    private static readonly Regex PunctuationSpacingRegex = new(
        SchemaComparisonTextConstants.Normalization.PunctuationSpacingPattern,
        RegexOptions.Compiled);

    // islevi: CREATE/view/trigger/function/index/check gibi uzun tanimlarda bosluk ve noktalama cevresi farklarini eler.
    public string NormalizeDefinition(string? definition)
    {
        if (string.IsNullOrWhiteSpace(definition))
        {
            return string.Empty;
        }

        var normalized = definition
            .Replace(
                SchemaComparisonTextConstants.Normalization.WindowsNewLine,
                SchemaComparisonTextConstants.Normalization.NewLine)
            .Replace(
                SchemaComparisonTextConstants.Normalization.CarriageReturn,
                SchemaComparisonTextConstants.Normalization.LineFeed)
            .Trim()
            .TrimEnd(SchemaComparisonTextConstants.Normalization.StatementTerminator);
        normalized = WhitespaceRegex.Replace(normalized, SchemaComparisonTextConstants.Normalization.SingleSpace);
        normalized = PunctuationSpacingRegex.Replace(
            normalized,
            SchemaComparisonTextConstants.Normalization.CapturedTokenReplacement);
        return normalized.Trim();
    }

    // islevi: Default/check gibi scalar expression'larda SQL Server'in fazla dis parantezlerini yalanci fark olmaktan cikarir.
    public string NormalizeExpression(string? expression)
    {
        var normalized = NormalizeDefinition(expression);
        while (HasSingleOuterParentheses(normalized))
        {
            normalized = normalized[1..^1].Trim();
        }

        return normalized;
    }

    // islevi: Identifier kiyasinda koseli/cift tirnak ayrimini ve bas-son bosluklari yalanci fark olmaktan cikarir.
    public string NormalizeIdentifier(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return string.Empty;
        }

        return identifier.Trim().Trim(
            SchemaComparisonTextConstants.Normalization.SqlServerIdentifierOpen,
            SchemaComparisonTextConstants.Normalization.SqlServerIdentifierClose,
            SchemaComparisonTextConstants.Normalization.AnsiIdentifierQuote);
    }

    // islevi: Liste alanlarini kararli siraya gerek olmadan ayni string formata getirir.
    public string NormalizeNameList(IEnumerable<string> names, bool sort)
    {
        var normalizedNames = names.Select(NormalizeIdentifier);
        if (sort)
        {
            normalizedNames = normalizedNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
        }

        return string.Join(SchemaComparisonTextConstants.NameListSeparator, normalizedNames);
    }

    // islevi: Kolon collation'ini veritabani varsayilanina gore normalize eder; varsayilanla ayniysa gurultuyu bos degere indirir.
    public string NormalizeColumnCollation(string? columnCollation, string? databaseCollation)
    {
        var normalizedColumn = NormalizeIdentifier(columnCollation);
        var normalizedDatabase = NormalizeIdentifier(databaseCollation);
        return string.Equals(normalizedColumn, normalizedDatabase, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : normalizedColumn;
    }

    // islevi: Expression'in tamamini saran tek parantez cifti olup olmadigini dengeli parantez sayarak anlar.
    private static bool HasSingleOuterParentheses(string value)
    {
        if (value.Length < 2 || value[0] != '(' || value[^1] != ')')
        {
            return false;
        }

        var depth = 0;
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] == '(')
            {
                depth++;
            }
            else if (value[index] == ')')
            {
                depth--;
                if (depth == 0 && index < value.Length - 1)
                {
                    return false;
                }
            }
        }

        return depth == 0;
    }
}

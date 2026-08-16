using System.Collections.Generic;

namespace Ptn.DatabaseChecker.Repository.Comparison;

// islevi: Onayli dinamik-SQL kurucusunun metin ve bagli parametrelerini adlandirilmis tek degerde tasir.
// sistemdeki gorevi: Row-count, tablo verisi ve anahtarli sorgularin tuple yerine ayni guvenli query kontratini kullanmasini saglar.
/// <summary>
/// Onayli dinamik SQL metnini bagli parametreleriyle tasir.
/// </summary>
internal sealed class DynamicSqlQuery
{
    // islevi: Sorgu metni ve parametreleri degistirilemez bir query degerinde birlestirir.
    internal DynamicSqlQuery(string sql, IReadOnlyList<object> parameters)
    {
        Sql = sql;
        Parameters = parameters;
    }

    internal string Sql { get; }

    internal IReadOnlyList<object> Parameters { get; }
}

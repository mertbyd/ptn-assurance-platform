namespace Ptn.DatabaseChecker.Search;

// islevi: Elasticsearch multi-field alt alan adlarini merkezi tutar.
// sistemdeki gorevi: Exact-match sorgularinda ".keyword" gibi alan suffix'leri string literal olarak dagilmaz.
public static class SearchFieldSuffixes
{
    // Text alanlarin analiz edilmemis keyword alt alani.
    public const string Keyword = "keyword";
}

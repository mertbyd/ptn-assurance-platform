using System;

namespace Ptn.DatabaseChecker.Search;

// islevi: Elasticsearch'e yazilan tum arama dokumanlarinin ortak sozlesmesini tanimlar.
// sistemdeki gorevi: Generic repository index adini ve ES _id degerini compile-time guvenli okur.
public interface ISearchDocument
{
    // Dokumanin ait oldugu Elasticsearch index adi; concrete modul adapteri tarafindan belirlenir.
    static abstract string IndexName { get; }

    // Kaynak entity Id'si; Elasticsearch _id degeri olarak kullanilir.
    Guid Id { get; }
}

using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Models.Assertions;
using Ptn.DatabaseChecker.Models.Correlation;

namespace Ptn.DatabaseChecker.Models.Diagnosis;

// islevi: Assertion sonucu veya yapilandirilmis provider hata alanlarindan tam olarak birini tasir.
// sistemdeki gorevi: Test Module girdisini provider exception nesnesi, mesaj parse etme veya SUT interceptor'i olmadan teshis cekirdegine sokar.
public sealed class FailureSignal
{
    public Guid ConnectionId { get; set; }
    public AssertionFailureSignal? Assertion { get; set; }
    public DatabaseExceptionFailureSignal? DbException { get; set; }
    public CorrelationRef? Correlation { get; set; }
    public string SourceKindCode => Assertion is null
        ? FailureSourceKindCodes.DatabaseException
        : FailureSourceKindCodes.Assertion;
    public string Code => Assertion?.OutcomeCode ?? DbException?.SqlState ?? string.Empty;

    // islevi: Assertion sinyalinin katalog adresi, anahtar degerleri, outcome ve hedefli failure kanitini tasir.
    // sistemdeki gorevi: KBP-704 sonucunu persisted entity veya public DTO'ya baglanmadan domain kimlik cikarimina verir.
    public sealed class AssertionFailureSignal
    {
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string OutcomeCode { get; set; } = string.Empty;
        public List<FailedExpectation> FailedExpectations { get; set; } = new();
    }

    // islevi: Provider'in motor kodu, SQLSTATE/hata numarasi ve yapilandirilmis alanlarini tasir.
    // sistemdeki gorevi: Npgsql alanlarini veya SQL Server numarasini mesaj metnine donusturmeden engine extractor'ina verir.
    public sealed class DatabaseExceptionFailureSignal
    {
        public string EngineCode { get; set; } = string.Empty;
        public string SqlState { get; set; } = string.Empty;
        public Dictionary<string, string?> ProviderFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}

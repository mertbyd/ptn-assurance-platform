using System;
using System.Collections.Generic;
using Ptn.DatabaseChecker.Dtos.Assertions;
using Ptn.DatabaseChecker.Dtos.Correlation;

namespace Ptn.DatabaseChecker.Dtos.Diagnosis;

// islevi: Kayitli baglanti ile tam olarak bir assertion veya yapilandirilmis database-exception sinyalini API girdisinde tasir.
// sistemdeki gorevi: Test Module'un SUT interceptor'i, provider exception nesnesi veya serbest hata mesaji gondermeden teshis istemesini saglar.
public sealed class DiagnoseRequestDto
{
    public Guid ConnectionId { get; set; }
    public AssertionSignalDto? Assertion { get; set; }
    public DatabaseExceptionSignalDto? DbException { get; set; }
    public CorrelationRefDto? Correlation { get; set; }

    // islevi: Assertion kaynakli tablo adresi, anahtar, outcome ve hedefli failed-expectation alanlarini tasir.
    // sistemdeki gorevi: KBP-704 sonucunun teshis endpoint'ine gereken dar ve secret icermeyen parcasidir.
    public sealed class AssertionSignalDto
    {
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public Dictionary<string, string?> KeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string OutcomeCode { get; set; } = string.Empty;
        public List<FailedExpectationDto> FailedExpectations { get; set; } = new();
    }

    // islevi: Provider motor kodu, SQLSTATE/hata numarasi ve yapilandirilmis alan sozlugunu tasir.
    // sistemdeki gorevi: PostgreSQL nesne alanlarini mesaj parse etmeden, SQL Server'i yalniz numarayla extractor'a ulastirir.
    public sealed class DatabaseExceptionSignalDto
    {
        public string EngineCode { get; set; } = string.Empty;
        public string SqlState { get; set; } = string.Empty;
        public Dictionary<string, string?> ProviderFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}

using System;
using System.Collections.Generic;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Models.Bridge.Footprint;

// islevi: Write-set capture kimligini, aday tablolarini ve korelasyon adresini birlikte tasir.
// sistemdeki gorevi: Checker capture DTO'sunun Application servisinde elle kurulmasini onler.
public sealed class WriteSetCaptureRequest
{
    public Guid ConnectionId { get; set; }
    public Guid CaptureRef { get; set; }
    public List<string> CandidateTables { get; set; } = [];
    public CorrelationRef? Correlation { get; set; }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Ptn.TestModule.Models.Bridge;

namespace Ptn.TestModule.Interface.Bridge;

// islevi: Database Checker sema bilgisini Test Module domainine tipli port olarak acar.
// sistemdeki gorevi: Manager'larin checker AppService veya DTO'larina dogrudan baglanmasini engeller.
public interface ISchemaKnowledgePort
{
    // Yazarlik aninda tek tablonun assertion odakli yapisini getirir.
    Task<PtnTableDescription> DescribeTableAsync(PtnTableQuery query, CancellationToken cancellationToken);

    // Baglantinin kanonik ve provider-bagimsiz sema fotografini getirir.
    Task<PtnSchemaSnapshot> GetSnapshotAsync(Guid connectionId, CancellationToken cancellationToken);

    // Baglantinin kanonik sema fotografi icin SHA-256 fingerprint dondurur.
    Task<string> GetSchemaFingerprintAsync(Guid connectionId, CancellationToken cancellationToken);
}

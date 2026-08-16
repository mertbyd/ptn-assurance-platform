using Ptn.ApiContractChecker.Entities.Snapshots;
using Ptn.ApiContractChecker.Models.Snapshots;

namespace Ptn.ApiContractChecker.Interface.Snapshots;

// islevi: Kalici spec iceriginden kosum ani yanit semasini cozer.
// sistemdeki gorevi: Parser, normalizasyon, dialect ve cache ayrintilarini conformance manager'dan saklar.
public interface ISpecSchemaResolver
{
    // CanonicalHash ile cache'lenen normalize edilmis snapshot'i dondurur.
    Task<SpecSnapshotModel> GetSnapshotAsync(SpecContent content);

    // Operasyon, durum kodu ve medya tipi icin validate edilebilir sema dugumunu dondurur.
    Task<ResolvedSpecSchemaModel?> ResolveAsync(
        SpecContent content,
        SpecOperationModel operation,
        string statusCode,
        string mediaType);

    // Operasyon ve medya tipi icin requestBody semasini cozer.
    Task<ResolvedSpecSchemaModel?> ResolveRequestAsync(
        SpecContent content,
        SpecOperationModel operation,
        string mediaType);
}

using Ptn.DatabaseChecker.Interface.Comparison;
using Ptn.DatabaseChecker.Models.Diagnosis;

namespace Ptn.DatabaseChecker.Interface.Diagnosis;

// islevi: Assertion veya provider-yapilandirilmis hata alanlarini motor-bagimsiz FailureIdentity'ye cevirir.
// sistemdeki gorevi: EngineComponentResolver ile secilen extractor'in mesaj parse etmeden kod, guven ve nesne referansi cikarmasini zorunlu kilar.
public interface IFailureIdentityExtractor : IEngineComponent
{
    // islevi: Tek failure sinyalinden provider mesajina bakmadan yapilandirilmis kimlik cikarir.
    FailureIdentity Extract(FailureSignal signal);
}

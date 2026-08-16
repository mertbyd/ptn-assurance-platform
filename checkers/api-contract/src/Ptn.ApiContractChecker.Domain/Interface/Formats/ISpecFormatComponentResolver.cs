namespace Ptn.ApiContractChecker.Interface.Formats;

// islevi: Verilen format koduna uygun TComponent bilesenini DI konteynerinden secer.
// sistemdeki gorevi: "Hangi bilesen?" karari tek yerde; yeni format = yeni TComponent implementasyonu, bu arayuze ve cagiranlara dokunulmaz (acik/kapali). Spec okuyucu ve spec cekici ayni resolver'i paylasir.
public interface ISpecFormatComponentResolver<TComponent>
    where TComponent : ISpecFormatComponent
{
    // Format koduna kayitli bileseni dondurur; destegi olmayan format kodu icin BusinessException firlatir.
    TComponent Resolve(string formatCode);
}

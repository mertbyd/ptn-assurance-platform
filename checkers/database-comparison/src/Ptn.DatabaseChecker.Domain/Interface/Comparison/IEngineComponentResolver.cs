namespace Ptn.DatabaseChecker.Interface.Comparison;

// islevi: Verilen motor koduna uygun TComponent bilesenini DI konteynerinden secer.
// sistemdeki gorevi: "Hangi bilesen?" karari tek yerde; yeni motor = yeni TComponent implementasyonu, bu arayuze/cagiranlara dokunulmaz (acik/kapali). Schema reader ve connection tester ayni resolver'i paylasir.
public interface IEngineComponentResolver<TComponent>
    where TComponent : IEngineComponent
{
    // Motor koduna kayitli bileseni dondurur; destegi olmayan motor kodu icin BusinessException firlatir.
    TComponent Resolve(string engineCode);
}

namespace Ptn.TestModule.MultiTenancy;

// islevi: Test Module'un tenant filtresini destekleyip desteklemedigini bildirir.
// sistemdeki gorevi: Host ve modul testlerinin ayni multi-tenancy kararini kullanmasini saglar.
// ADR-0016 §D: dort ana tablonun hepsi IMultiTenant'tir; kiraci kapsami veritabani katmanindadir.
public static class MultiTenancyConsts
{
    public const bool IsEnabled = true;
}

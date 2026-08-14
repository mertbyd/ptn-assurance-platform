using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Ptn.TestModule.Constants.Bridge.Lookups;
using Ptn.TestModule.ExceptionCodes.Bridge;
using Ptn.TestModule.Interface.Bridge;
using Ptn.TestModule.Managers.Bridge;
using Ptn.TestModule.Models.Bridge;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Ptn.TestModule.Bridge;

// islevi: Profil paketinin sema drift'i, kavram cozumu ve kapsam kararlarini dogrular.
// sistemdeki gorevi: NOT_BOUND ve Proposed gecislerinin provider veya ajan yorumuna kaymasini engeller.
public class ProfilePackManagerTests
{
    // Sema fingerprint'i degistiginde onayli baglamalari yeniden Proposed durumuna dusurur.
    [Fact]
    public async Task Should_downgrade_approved_bindings_when_schema_fingerprint_drifts()
    {
        var fixture = CreateFixture("sha256:current");

        var pack = await fixture.Manager.GetValidatedAsync(
            fixture.Pack.ProfileKey,
            Guid.NewGuid(),
            CancellationToken.None);

        pack.Bindings.ShouldAllBe(binding => binding.StateCode == PtnBindingStateCodes.Proposed);
        pack.Bindings.ShouldAllBe(binding => binding.ApprovedBy == null);
    }

    // Onayli bagi olmayan kavram icin karar uydurmak yerine kararli ConceptNotBound hatasi verir.
    [Fact]
    public void Should_reject_unbound_concept()
    {
        var fixture = CreateFixture("sha256:profile");

        var exception = Should.Throw<BusinessException>(() =>
            fixture.Manager.ResolveConcept(fixture.Pack, PtnConceptCodes.Quota));

        exception.Code.ShouldBe(TestModuleBridgeErrorCodes.ConceptNotBound);
    }

    // Kapsam raporunda onayli ve baglanmamis kavramlari bound/required oraniyla ayirir.
    [Fact]
    public void Should_build_bound_and_unbound_coverage()
    {
        var fixture = CreateFixture("sha256:profile");

        var coverage = fixture.Manager.BuildCoverage(
            fixture.Pack,
            [PtnConceptCodes.Subject, PtnConceptCodes.Quota]);

        coverage.BoundCount.ShouldBe(1);
        coverage.RequiredCount.ShouldBe(2);
        coverage.BoundRatio.ShouldBe(0.5m);
        coverage.UnboundConcepts.ShouldBe([PtnConceptCodes.Quota]);
    }

    // Teste ozel port cevaplariyla gercek manager sahipligini kurar.
    private static Fixture CreateFixture(string currentFingerprint)
    {
        var pack = CreatePack();
        var provider = Substitute.For<IProfilePackProvider>();
        provider.LoadAsync(pack.ProfileKey, Arg.Any<CancellationToken>()).Returns(pack);
        var schema = Substitute.For<ISchemaKnowledgePort>();
        schema.GetSchemaFingerprintAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(currentFingerprint);
        return new Fixture(new ProfilePackManager(provider, schema), pack);
    }

    // Gecerli kapali kodlar ve ifadeler iceren en kucuk profil paketini olusturur.
    private static PtnProfilePack CreatePack()
    {
        return new PtnProfilePack
        {
            ProfileKey = "unit-profile",
            Revision = "1",
            DbSchemaFingerprint = "sha256:profile",
            Bindings =
            [
                new PtnConceptBinding
                {
                    ConceptCode = PtnConceptCodes.Subject,
                    DbSchemaName = "identity",
                    TableName = "users",
                    PatternCode = PtnBindingPatternCodes.SemanticEntity,
                    StateCode = PtnBindingStateCodes.Approved,
                    ApprovedBy = "reviewer"
                }
            ],
            Paths = [CreatePath()]
        };
    }

    // Manager ifade dili kapisindan gecen tek adimli kanit yolu olusturur.
    private static PtnEvidencePathDefinition CreatePath()
    {
        return new PtnEvidencePathDefinition
        {
            PathKey = "unit-path",
            Trigger = new PtnEvidencePathDefinition.PtnEvidencePathTrigger { StatusCodes = [403] },
            Steps =
            [
                new PtnEvidencePathDefinition.PtnEvidencePathStep
                {
                    NodeKindCode = PtnNodeKindCodes.ScopeRequired,
                    SourceCode = PtnEvidenceSourceCodes.ApiFailureIdentity
                }
            ],
            ConfirmedWhen = "ScopeRequired.observed",
            InconclusiveWhen = "any(step.state == Unavailable)"
        };
    }

    // islevi: Manager ve ayni testte kullanilan profil paketini birlikte tasir.
    // sistemdeki gorevi: Test kurulumunun iki bagimli sonucunu adlandirilmis modelde tutar.
    private sealed record Fixture(ProfilePackManager Manager, PtnProfilePack Pack);
}

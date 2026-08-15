using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nexum.Abp.Foundation.Managers;
using Nexum.Abp.Foundation.Querying;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Constants.Catalog;
using Ptn.TestModule.Constants.Runs.Lookups;
using Ptn.TestModule.Entities.Catalog;
using Ptn.TestModule.Entities.Lookups;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.Interface.Catalog;
using Ptn.TestModule.Interface.Lookups;
using Ptn.TestModule.Models.Catalog;
using Ptn.TestModule.Models.Compilation;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Ptn.TestModule.Managers.Catalog;

// islevi: Senaryo surumunun normalizasyon, benzersizlik, onay ve durum gecislerini yonetir.
// sistemdeki gorevi: Entity'yi veri kabugu, AppService'i duz orkestrasyon olarak tutan tek is kurali sahibidir.
public class TestScenarioManager : FoundationManager<TestScenario, Guid>
{
    private readonly ITestScenarioRepository _repository;
    private readonly ITestScenarioStateRepository _stateRepository;
    private readonly ScenarioScheduleManager _scheduleManager;

    protected override string AlreadyExistsErrorCode => TestModuleScenarioErrorCodes.VersionAlreadyExists;

    // Senaryo, durum lookup ve zamanlama kurallarini domain akisina baglar.
    public TestScenarioManager(
        ITestScenarioRepository repository,
        ITestScenarioStateRepository stateRepository,
        ScenarioScheduleManager scheduleManager)
        : base(repository)
    {
        _repository = repository;
        _stateRepository = stateRepository;
        _scheduleManager = scheduleManager;
    }

    // Yeni senaryoyu kanoniklestirir, siradaki surumu ve iki benzersizlik kuralini uygular.
    public async Task<TestScenario> CreateAsync(
        TestScenarioCreateModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var normalized = Normalize(model);
        var versionNo = await _repository.GetNextVersionNoAsync(normalized.ScenarioKey, cancellationToken);

        await EnsureVersionAvailableAsync(normalized.ScenarioKey, versionNo, cancellationToken);
        await EnsureContentAvailableAsync(normalized.ScenarioKey, normalized.SourceHash, null, cancellationToken);
        var draftStateId = await GetStateIdAsync(TestScenarioStateCodes.Draft, cancellationToken);

        return new TestScenario(
            GuidGenerator.Create(),
            versionNo,
            draftStateId,
            CurrentTenant.Id,
            normalized);
    }

    // Draft veya onay bekleyen surumu gunceller; kaynak hash degisince onayi ve bekleme durumunu geri alir.
    public async Task<TestScenario> UpdateAsync(
        TestScenario entity,
        TestScenarioUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(model);
        await EnsureEditableStateAsync(entity, cancellationToken);

        var normalized = Normalize(model);
        await EnsureContentAvailableAsync(entity.ScenarioKey, normalized.SourceHash, entity.Id, cancellationToken);
        var sourceChanged = !string.Equals(entity.SourceHash, normalized.SourceHash, StringComparison.Ordinal);

        Apply(entity, normalized);
        if (sourceChanged)
        {
            await InvalidateApprovalAsync(entity, cancellationToken);
        }

        return entity;
    }

    // Checker'in guncel sema fingerprint'ini katalogdaki 64 karakterlik digest bicimine uygular.
    public void ApplyDbSchemaFingerprint(TestScenarioMaterialSeal seal, string fingerprint)
    {
        ArgumentNullException.ThrowIfNull(seal);
        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);

        var normalized = fingerprint.Trim();
        if (normalized.StartsWith(PtnBridgeSettingNames.FingerprintPrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[PtnBridgeSettingNames.FingerprintPrefix.Length..];
        }
        seal.DbSchemaFingerprint = NormalizeRequiredHash(normalized, nameof(seal.DbSchemaFingerprint));
    }

    // Draft surumu insan onayi bekleyen duruma tasir.
    public async Task<TestScenario> SubmitForApprovalAsync(
        TestScenario entity,
        CancellationToken cancellationToken = default)
    {
        await EnsureStateAsync(entity, TestScenarioStateCodes.Draft, cancellationToken);
        entity.StateId = await GetStateIdAsync(TestScenarioStateCodes.PendingApproval, cancellationToken);
        return entity;
    }

    // Insan onayini o anda onaylanan source hash'e baglar.
    public async Task<TestScenario> BindApprovalAsync(
        TestScenario entity,
        Guid approvedBy,
        DateTime approvedAt,
        CancellationToken cancellationToken = default)
    {
        await EnsureStateAsync(entity, TestScenarioStateCodes.PendingApproval, cancellationToken);
        if (approvedBy == Guid.Empty)
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.ApprovalRequired);
        }

        entity.ApprovedBy = approvedBy;
        entity.ApprovedAt = approvedAt;
        entity.ApprovalBoundToHash = entity.SourceHash;
        return entity;
    }

    // Tum kapilar gecmis onayli surume makine kanitini yazar ve Published durumuna tasir.
    public async Task<TestScenario> PublishAsync(
        TestScenario entity,
        TestScenarioPublishDecision decision,
        ScenarioCompilationEvidence evidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(evidence);
        await EnsureStateAsync(entity, TestScenarioStateCodes.PendingApproval, cancellationToken);
        EnsureApprovalIsCurrent(entity);
        if (!decision.IsPublishable)
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.PublicationGateFailed)
                .WithData(nameof(decision.FailedGateCodes), decision.FailedGateCodes);
        }

        var publishedStateId = await GetStateIdAsync(TestScenarioStateCodes.Published, cancellationToken);
        var previous = await _repository.FindPublishedAsync(entity.ScenarioKey, publishedStateId, cancellationToken);
        ApplyCompilation(entity, evidence);
        entity.StateId = publishedStateId;

        // Zamanlama yayinlanmis surumun alanidir; onceki surum vadesini birakir, iki surum ayni anda vadeli olamaz.
        ScenarioScheduleManager.Transfer(previous, entity);
        return entity;
    }

    // Yayinlanmis surume cron zamanlamasi yazar; vade ve dogrulama kararlarini zamanlama Manager'i verir.
    /// <summary>Yayinlanmis senaryo surumunun zamanlamasini gunceller.</summary>
    public async Task<TestScenario> UpdateScheduleAsync(
        TestScenario entity,
        TestScenarioScheduleModel model,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var publishedStateId = await GetStateIdAsync(TestScenarioStateCodes.Published, cancellationToken);
        if (entity.StateId != publishedStateId)
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.ScheduleRequiresPublishedVersion);
        }

        return _scheduleManager.Apply(entity, model, now);
    }

    // Derleyicinin urettigi belgeyi, kanonik hash'i ve assertion sayisini satira yazar (ADR-0015 §C).
    private static void ApplyCompilation(TestScenario entity, ScenarioCompilationEvidence evidence)
    {
        entity.CompiledDocument = EnsureDocument(
            evidence.CompiledDocument,
            nameof(entity.CompiledDocument),
            TestModuleScenarioErrorCodes.Validation.CompiledDocumentRequired);
        entity.CompiledHash = NormalizeRequiredHash(evidence.CompiledHash, nameof(entity.CompiledHash));
        entity.AssertionCount = evidence.AssertionCount;
    }

    // Yayinlanmis surumu silmeden kullanimdan kaldirir ve zamanlamasini birakir.
    public async Task<TestScenario> DeprecateAsync(
        TestScenario entity,
        CancellationToken cancellationToken = default)
    {
        await EnsureStateAsync(entity, TestScenarioStateCodes.Published, cancellationToken);
        entity.StateId = await GetStateIdAsync(TestScenarioStateCodes.Deprecated, cancellationToken);

        // Kullanimdan kalkan surum vadeli kalamaz; aksi halde vade tarayicisi olu surumu kosardi.
        ScenarioScheduleManager.Clear(entity);
        return entity;
    }

    // Senaryo surumleri tarihsel kayit oldugu icin tum delete akislarini reddeder.
    public Task EnsureCanDeleteAsync(
        TestScenario entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        throw new BusinessException(TestModuleScenarioErrorCodes.DeletionNotAllowed);
    }

    // Senaryo anahtari ve surum numarasi ciftinin bos oldugunu dogrular.
    private Task EnsureVersionAvailableAsync(
        string scenarioKey,
        int versionNo,
        CancellationToken cancellationToken)
    {
        var query = new RepositoryQuery<TestScenario>()
            .Where(entity => entity.ScenarioKey == scenarioKey && entity.VersionNo == versionNo);
        return EnsureUniqueAsync(query, TestModuleScenarioErrorCodes.VersionAlreadyExists, cancellationToken);
    }

    // Senaryo anahtari ve kaynak hash ciftini create veya update icin benzersiz tutar.
    private Task EnsureContentAvailableAsync(
        string scenarioKey,
        string sourceHash,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        var query = new RepositoryQuery<TestScenario>()
            .Where(entity => entity.ScenarioKey == scenarioKey && entity.SourceHash == sourceHash);
        if (excludedId.HasValue)
        {
            query = query.Where(entity => entity.Id != excludedId.Value);
        }
        return EnsureUniqueAsync(query, TestModuleScenarioErrorCodes.ContentAlreadyExists, cancellationToken);
    }

    // Update edilebilir iki durumdan birinde bulunmayan surumu reddeder.
    private async Task EnsureEditableStateAsync(TestScenario entity, CancellationToken cancellationToken)
    {
        var draftStateId = await GetStateIdAsync(TestScenarioStateCodes.Draft, cancellationToken);
        var pendingStateId = await GetStateIdAsync(TestScenarioStateCodes.PendingApproval, cancellationToken);
        if (entity.StateId != draftStateId && entity.StateId != pendingStateId)
        {
            throw InvalidStateTransition(entity.StateId);
        }
    }

    // Aggregate durumunun beklenen lookup koduna bagli oldugunu dogrular.
    private async Task EnsureStateAsync(
        TestScenario entity,
        string expectedStateCode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var expectedStateId = await GetStateIdAsync(expectedStateCode, cancellationToken);
        if (entity.StateId != expectedStateId)
        {
            throw InvalidStateTransition(entity.StateId);
        }
    }

    // Durum lookup kodunu aggregate'e yazilacak kimlige cozer.
    private async Task<Guid> GetStateIdAsync(string stateCode, CancellationToken cancellationToken)
    {
        var state = await _stateRepository.FindAsync(
            new RepositoryQuery<TestScenarioState>().Where(item => item.Code == stateCode),
            cancellationToken);
        return state?.Id ?? throw new EntityNotFoundException(typeof(TestScenarioState));
    }

    // Kaynak belge degisince insan onayini temizler ve surumu yeniden Draft'a indirir.
    private async Task InvalidateApprovalAsync(TestScenario entity, CancellationToken cancellationToken)
    {
        entity.ApprovedBy = null;
        entity.ApprovedAt = null;
        entity.ApprovalBoundToHash = null;
        entity.StateId = await GetStateIdAsync(TestScenarioStateCodes.Draft, cancellationToken);
    }

    // Onayin varligini ve mevcut source hash'e bagli kalmasini dogrular.
    private static void EnsureApprovalIsCurrent(TestScenario entity)
    {
        if (!entity.ApprovedBy.HasValue || !entity.ApprovedAt.HasValue)
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.ApprovalRequired);
        }
        if (!string.Equals(entity.ApprovalBoundToHash, entity.SourceHash, StringComparison.Ordinal))
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.ApprovalHashMismatch);
        }
    }

    // Create girdisinin kararli ve hash alanlarini kanoniklestirir.
    private static TestScenarioCreateModel Normalize(TestScenarioCreateModel model)
    {
        return new TestScenarioCreateModel
        {
            ScenarioKey = NormalizeScenarioKey(model.ScenarioKey),
            Title = NormalizeRequiredText(model.Title, nameof(model.Title), TestScenarioConsts.MaxTitleLength),
            Description = NormalizeOptionalText(model.Description, nameof(model.Description), TestScenarioConsts.MaxDescriptionLength),
            SourceDocument = EnsureDocument(
                model.SourceDocument,
                nameof(model.SourceDocument),
                TestModuleScenarioErrorCodes.Validation.SourceDocumentRequired),
            SourceHash = NormalizeRequiredHash(model.SourceHash, nameof(model.SourceHash)),
            MaterialSeal = Normalize(model.MaterialSeal),
            DerivabilityCode = NormalizeOptionalText(model.DerivabilityCode, nameof(model.DerivabilityCode), TestScenarioConsts.MaxDerivabilityCodeLength),
            AuthoredByAgent = model.AuthoredByAgent,
            AgentModelRef = NormalizeOptionalText(model.AgentModelRef, nameof(model.AgentModelRef), TestScenarioConsts.MaxAgentModelRefLength),
            Notes = NormalizeOptionalText(model.Notes, nameof(model.Notes), TestScenarioConsts.MaxNotesLength)
        };
    }

    // Update girdisinin metin ve hash alanlarini create ile ayni bicime getirir.
    private static TestScenarioUpdateModel Normalize(TestScenarioUpdateModel model)
    {
        return new TestScenarioUpdateModel
        {
            Title = NormalizeRequiredText(model.Title, nameof(model.Title), TestScenarioConsts.MaxTitleLength),
            Description = NormalizeOptionalText(model.Description, nameof(model.Description), TestScenarioConsts.MaxDescriptionLength),
            SourceDocument = EnsureDocument(
                model.SourceDocument,
                nameof(model.SourceDocument),
                TestModuleScenarioErrorCodes.Validation.SourceDocumentRequired),
            SourceHash = NormalizeRequiredHash(model.SourceHash, nameof(model.SourceHash)),
            MaterialSeal = Normalize(model.MaterialSeal),
            DerivabilityCode = NormalizeOptionalText(model.DerivabilityCode, nameof(model.DerivabilityCode), TestScenarioConsts.MaxDerivabilityCodeLength),
            AuthoredByAgent = model.AuthoredByAgent,
            AgentModelRef = NormalizeOptionalText(model.AgentModelRef, nameof(model.AgentModelRef), TestScenarioConsts.MaxAgentModelRefLength),
            Notes = NormalizeOptionalText(model.Notes, nameof(model.Notes), TestScenarioConsts.MaxNotesLength)
        };
    }

    // Malzeme kimliklerini korur, digest alanlarini ortak SHA-256 bicimine getirir.
    private static TestScenarioMaterialSeal Normalize(TestScenarioMaterialSeal seal)
    {
        ArgumentNullException.ThrowIfNull(seal);
        return new TestScenarioMaterialSeal
        {
            RulesFingerprint = NormalizeOptionalHash(seal.RulesFingerprint, nameof(seal.RulesFingerprint)),
            SpecSnapshotId = seal.SpecSnapshotId,
            SpecFingerprint = NormalizeOptionalHash(seal.SpecFingerprint, nameof(seal.SpecFingerprint)),
            DbConnectionId = seal.DbConnectionId,
            DbSchemaFingerprint = NormalizeOptionalHash(seal.DbSchemaFingerprint, nameof(seal.DbSchemaFingerprint)),
            ProfileFingerprint = NormalizeOptionalHash(seal.ProfileFingerprint, nameof(seal.ProfileFingerprint))
        };
    }

    // Normalize edilmis update alanlarini entity veri kabuguna atar.
    private static void Apply(TestScenario entity, TestScenarioUpdateModel model)
    {
        entity.Title = model.Title;
        entity.Description = model.Description;
        entity.SourceDocument = model.SourceDocument;
        entity.SourceHash = model.SourceHash;
        entity.RulesFingerprint = model.MaterialSeal.RulesFingerprint;
        entity.SpecSnapshotId = model.MaterialSeal.SpecSnapshotId;
        entity.SpecFingerprint = model.MaterialSeal.SpecFingerprint;
        entity.DbConnectionId = model.MaterialSeal.DbConnectionId;
        entity.DbSchemaFingerprint = model.MaterialSeal.DbSchemaFingerprint;
        entity.ProfileFingerprint = model.MaterialSeal.ProfileFingerprint;
        entity.DerivabilityCode = model.DerivabilityCode;
        entity.AuthoredByAgent = model.AuthoredByAgent;
        entity.AgentModelRef = model.AgentModelRef;
        entity.Notes = model.Notes;
    }

    // Senaryo anahtarini kucuk harfli kapali formata indirger.
    private static string NormalizeScenarioKey(string? value)
    {
        var normalized = NormalizeRequiredText(value, nameof(TestScenario.ScenarioKey), TestScenarioConsts.MaxScenarioKeyLength)
            .ToLowerInvariant();
        if (!Regex.IsMatch(normalized, TestScenarioConsts.ScenarioKeyPattern))
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.InvalidScenarioKey);
        }
        return normalized;
    }

    // Belge icerigini hash bagini bozmadan oldugu gibi korur ve yalniz boslugu reddeder.
    private static string EnsureDocument(string? value, string field, string errorCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessException(errorCode)
                .WithData(nameof(field), field);
        }
        return value;
    }

    // Zorunlu SHA-256 metnini dogrular ve kucuk harfli kanonik bicime getirir.
    private static string NormalizeRequiredHash(string? value, string field)
    {
        var normalized = NormalizeRequiredText(value, field, TestScenarioConsts.HashLength);
        if (!Regex.IsMatch(normalized, TestScenarioConsts.HashPattern))
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.InvalidHash).WithData(nameof(field), field);
        }
        return normalized.ToLowerInvariant();
    }

    // Opsiyonel SHA-256 metnini null veya kucuk harfli kanonik bicime getirir.
    private static string? NormalizeOptionalHash(string? value, string field)
    {
        return string.IsNullOrWhiteSpace(value) ? null : NormalizeRequiredHash(value, field);
    }

    // Gecis hatasini mevcut durum kimligiyle kodlu olarak olusturur.
    private static BusinessException InvalidStateTransition(Guid stateId)
    {
        return new BusinessException(TestModuleScenarioErrorCodes.InvalidStateTransition)
            .WithData(nameof(stateId), stateId);
    }
}

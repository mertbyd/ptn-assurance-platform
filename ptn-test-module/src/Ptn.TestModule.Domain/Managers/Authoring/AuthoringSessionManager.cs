using System;
using System.Collections.Generic;
using System.Linq;
using Ptn.TestModule.Constants.Authoring;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.ExceptionCodes.Catalog;
using Ptn.TestModule.Managers.Compilation;
using Ptn.TestModule.Models.Authoring;
using Ptn.TestModule.Models.Bridge;
using Ptn.TestModule.Models.Bridge.Agent;
using Volo.Abp;

namespace Ptn.TestModule.Managers.Authoring;

// islevi: Tenant authoring oturumunun kapali cevap ve tek-adim birlestirme kurallarini uygular.
// sistemdeki gorevi: Cache'i I/O sahibi, Arazzo belgesini compiler sahibi olarak tutan domain karar noktasidir.
public class AuthoringSessionManager : TestModuleDomainService
{
    private readonly ArazzoCompilerManager _compilerManager;

    // Mekanik belge ureticisini session mutasyon kurallarina baglar.
    public AuthoringSessionManager(ArazzoCompilerManager compilerManager)
    {
        _compilerManager = compilerManager;
    }

    // Gercek grounding sonucundan tenant kapsamli yeni cache oturumu kurar.
    public AuthoringSession Create(
        Guid sessionId,
        Guid? tenantId,
        AuthoringSessionCreateModel model,
        GroundRequest groundingRequest,
        GroundingResult grounding)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(groundingRequest);
        ArgumentNullException.ThrowIfNull(grounding);
        var session = new AuthoringSession
        {
            Id = sessionId,
            TenantId = tenantId,
            SpecSnapshotId = groundingRequest.SpecSnapshotId,
            WorkflowId = model.WorkflowId.Trim(),
            WorkflowSummary = model.WorkflowSummary.Trim(),
            ApiSourceUrl = model.ApiSourceUrl.Trim(),
            DatabaseSourceUrl = model.DatabaseSourceUrl.Trim(),
            Questions = grounding.Questions.ToList(),
            Operations = grounding.OperationBinding?.Suggestions.ToList() ?? [],
            TtlMs = AuthoringSessionConsts.TtlMilliseconds
        };
        session.SourceDocument = _compilerManager.BuildAuthoringDocument(session);
        return session;
    }

    // Cache miss veya tenant uyusmazligini ayni fail-closed session sinirinda denetler.
    public AuthoringSession EnsureAvailable(AuthoringSession? session, Guid? tenantId)
    {
        if (session is null)
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.AuthoringSessionNotFound);
        }
        if (session.TenantId != tenantId)
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.AuthoringSessionTenantMismatch);
        }
        return session;
    }

    // Cevabi yalniz mevcut sorunun kapali secenek kumesinden kabul eder.
    public AuthoringSession Answer(
        AuthoringSession session,
        Guid? tenantId,
        AuthoringAnswerModel model)
    {
        EnsureAvailable(session, tenantId);
        var question = session.Questions.FirstOrDefault(item =>
            item.QuestionCode == model.QuestionCode);
        if (question is null || !question.Options.Contains(model.SelectedOption, StringComparer.Ordinal))
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.AuthoringAnswerInvalid);
        }
        session.Answers[question.QuestionCode] = model.SelectedOption;
        return session;
    }

    // Cevaplanmis grounding referansini tek adima cozer ve tam belgeyi mekanik yeniden uretir.
    public AuthoringSession AddStep(
        AuthoringSession session,
        Guid? tenantId,
        AuthoringStepModel model)
    {
        EnsureAvailable(session, tenantId);
        EnsureQuestionsAnswered(session);
        var operation = ResolveOperation(session, model.OperationReferenceId);
        if (session.Steps.Any(item => item.StepId == model.StepId))
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.AuthoringStepAlreadyExists);
        }
        session.Steps.Add(new AuthoringStep
        {
            StepId = model.StepId,
            OperationReferenceId = operation.ReferenceId,
            OperationId = operation.SourceOperationId,
            Method = operation.SourceMethod,
            Path = operation.SourcePath,
            RequestBodyJson = model.RequestBodyJson,
            AssertionPaths = model.AssertionPaths.ToList()
        });
        session.SourceDocument = _compilerManager.BuildAuthoringDocument(session);
        return session;
    }

    // Cevaplanmis veritabani zemin adayini tek adima cozer ve tam belgeyi mekanik yeniden uretir.
    public AuthoringSession AddDatabaseStep(
        AuthoringSession session,
        Guid? tenantId,
        AuthoringDatabaseStep step)
    {
        EnsureAvailable(session, tenantId);
        EnsureQuestionsAnswered(session);

        if (session.DatabaseSteps.Any(item => item.StepId == step.StepId))
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.AuthoringStepAlreadyExists);
        }

        // Anahtar baglamalari kolon adiyla eslesir; buyuk-kucuk harf farki adim uretmemelidir.
        step.KeyBindings = new Dictionary<string, string?>(step.KeyBindings, StringComparer.OrdinalIgnoreCase);
        session.DatabaseSteps.Add(step);
        session.SourceDocument = _compilerManager.BuildAuthoringDocument(session);
        return session;
    }

    // Zemin cagrisinda verilen oturumu dogrular ve onerilen adim varsa ayni AddStep yolundan gecirir.
    public AuthoringSession Continue(
        AuthoringSession? session,
        Guid? tenantId,
        AuthoringStepModel? model)
    {
        var current = EnsureAvailable(session, tenantId);
        return model is null ? current : AddStep(current, tenantId, model);
    }

    // Zemin sonucuna oturum kimligini, adim sayisini ve bekleyen kapali sorulari ekler.
    public GroundingResult Attach(GroundingResult grounding, AuthoringSession? session)
    {
        ArgumentNullException.ThrowIfNull(grounding);
        if (session is null)
        {
            return grounding;
        }
        grounding.SessionId = session.Id;
        grounding.StepCount = session.Steps.Count;
        grounding.PendingQuestionCodes = session.Questions
            .Where(question => !session.Answers.ContainsKey(question.QuestionCode))
            .Select(question => question.QuestionCode)
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToList();
        return grounding;
    }

    // Tum deterministik sorular cevaplanmadan model adimi kabul etmez.
    private static void EnsureQuestionsAnswered(AuthoringSession session)
    {
        if (session.Questions.Any(question => !session.Answers.ContainsKey(question.QuestionCode)))
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.AuthoringQuestionsUnanswered);
        }
    }

    // Operasyon referansini grounding adaylarinda ve varsa secilmis cevapta dogrular.
    private static OperationSuggestion ResolveOperation(AuthoringSession session, Guid referenceId)
    {
        session.Answers.TryGetValue(PtnOpenQuestionCodes.OperationReferenceRequired, out var selected);
        if (selected is not null && selected != referenceId.ToString(PtnBridgeConsts.ReferenceIdFormat))
        {
            throw new BusinessException(TestModuleScenarioErrorCodes.AuthoringOperationNotGrounded);
        }
        return session.Operations.FirstOrDefault(item => item.ReferenceId == referenceId) ??
               throw new BusinessException(TestModuleScenarioErrorCodes.AuthoringOperationNotGrounded);
    }
}

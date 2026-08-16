using System;
using FluentValidation.TestHelper;
using Ptn.TestModule.Constants.Bridge;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Dtos.Catalog;
using Ptn.TestModule.FluentValidation.Bridge.Agent;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Services.Bridge;

// islevi: Validate isteginin eski Inconclusive yolu ile yeni muhurlu yayin adayi seklini dogrular.
// sistemdeki gorevi: Malzeme kimliklerinin checker baglamindan kopuk tasinmasini public sinirda engeller.
public class ValidateRequestDtoValidatorTests
{
    // Kaynak kaniti olmayan geriye uyumlu istek gate'e gitmeden Inconclusive kalabilmelidir.
    [Fact]
    public void Should_accept_a_legacy_request_without_publication_evidence()
    {
        var result = new ValidateRequestDtoValidator().TestValidate(ValidInput());

        result.IsValid.ShouldBeTrue();
    }

    // Kaynak ve eksiksiz muhur birlikte verildiginde gercek yayin yolu kabul edilmelidir.
    [Fact]
    public void Should_accept_source_document_with_matching_material_identities()
    {
        var input = ValidInput();
        input.SourceDocument = "arazzo: 1.0.1";
        input.MaterialSeal = CreateMaterialSeal(input);

        var result = new ValidateRequestDtoValidator().TestValidate(input);

        result.IsValid.ShouldBeTrue();
    }

    // Muhur baska DB baglantisina aitse checker sorgusu ile yayin malzemesi karistirilmamalidir.
    [Fact]
    public void Should_reject_a_material_seal_from_another_connection()
    {
        var input = ValidInput();
        input.SourceDocument = "arazzo: 1.0.1";
        input.MaterialSeal = CreateMaterialSeal(input);
        input.MaterialSeal.DbConnectionId = Guid.NewGuid();

        var result = new ValidateRequestDtoValidator().TestValidate(input);

        result.ShouldHaveValidationErrorFor(item => item.MaterialSeal);
    }

    // Validate sinirinin zorunlu profil, snapshot, baglanti ve sunum alanlarini kurar.
    private static ValidateRequestDto ValidInput() => new()
    {
        ProfileKey = "unit-profile",
        SpecSnapshotId = Guid.NewGuid(),
        ConnectionId = Guid.NewGuid(),
        ResponseFormat = PtnResponseFormatCodes.Detailed
    };

    // Request kimlikleriyle bagli ve bicimsel olarak gecerli malzeme muhrunu kurar.
    private static TestScenarioMaterialSealDto CreateMaterialSeal(ValidateRequestDto input) => new()
    {
        RulesFingerprint = Hash('a'),
        SpecSnapshotId = input.SpecSnapshotId,
        SpecFingerprint = Hash('b'),
        DbConnectionId = input.ConnectionId,
        DbSchemaFingerprint = Hash('c'),
        ProfileFingerprint = Hash('d')
    };

    // Test muhurlerini 64 karakterlik SHA-256 digest biciminde uretir.
    private static string Hash(char value) => new(value, 64);
}

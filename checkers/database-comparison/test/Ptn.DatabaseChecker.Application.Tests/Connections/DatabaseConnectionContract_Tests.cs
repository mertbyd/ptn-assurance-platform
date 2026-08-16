using System;
using Ptn.DatabaseChecker.Application.Mappers.Connections;
using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Dtos.Connections;
using Ptn.DatabaseChecker.Entities.Connections;
using Ptn.DatabaseChecker.Entities.Lookups;
using Ptn.DatabaseChecker.FluentValidation.Connections;
using Ptn.DatabaseChecker.Models.Comparison;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.Connections;

// islevi: Baglanti TLS alanlarinin ortak validator ve Mapperly sozlesmesinden eksiksiz gectigini dogrular.
// sistemdeki gorevi: Create/Update'in farkli TLS kurallari gelistirmesini ve test sonucu privilege alanlarinin DTO'da kaybolmasini engeller.
public class DatabaseConnectionContract_Tests
{
    [Fact]
    public void Create_And_Update_Should_Reject_The_Same_Invalid_Tls_Code()
    {
        var createResult = new CreateDatabaseConnectionDtoValidator().Validate(CreateDto("invalid"));
        var updateResult = new UpdateDatabaseConnectionDtoValidator().Validate(UpdateDto("invalid"));

        createResult.Errors.ShouldContain(error => error.PropertyName == nameof(CreateDatabaseConnectionDto.TlsModeCode));
        updateResult.Errors.ShouldContain(error => error.PropertyName == nameof(UpdateDatabaseConnectionDto.TlsModeCode));
    }

    [Fact]
    public void Mapperly_Should_Carry_Tls_And_Privilege_Fields()
    {
        var mapper = new DatabaseConnectionMapper();
        var createModel = mapper.MapToCreateModel(CreateDto(TlsModeCodes.Prefer));
        var entity = new DatabaseConnection(Guid.NewGuid())
        {
            EngineId = Guid.NewGuid(),
            Engine = new DatabaseEngine(Guid.NewGuid(), DatabaseEngineCodes.PostgreSql, "PostgreSQL"),
            Name = "connection",
            Host = "database.example",
            Port = 5432,
            DatabaseName = "sample",
            VaultSecretPath = string.Empty,
            IsActive = true,
            TlsModeCode = TlsModeCodes.Disable,
            TrustServerCertificate = false
        };
        var connectionDto = mapper.MapToDto(entity);
        var testResult = mapper.MapToTestResultDto(new ConnectionTestResult
        {
            Succeeded = true,
            CanWrite = true,
            IsSuperUser = false,
            PrivilegeWarningCode = "warning-code"
        });

        createModel.TlsModeCode.ShouldBe(TlsModeCodes.Prefer);
        createModel.TrustServerCertificate.ShouldBeTrue();
        connectionDto.TlsModeCode.ShouldBe(TlsModeCodes.Disable);
        connectionDto.TrustServerCertificate.ShouldBeFalse();
        testResult.CanWrite.ShouldBeTrue();
        testResult.IsSuperUser.ShouldBeFalse();
        testResult.PrivilegeWarningCode.ShouldBe("warning-code");
    }

    // islevi: Create validator ve mapper testleri icin tum zorunlu alanlari gecerli DTO kurar.
    private static CreateDatabaseConnectionDto CreateDto(string tlsModeCode)
        => new()
        {
            EngineId = Guid.NewGuid(),
            Name = "connection",
            Host = "database.example",
            Port = 5432,
            DatabaseName = "sample",
            Username = "reader",
            Password = new string('x', 1),
            IsActive = true,
            TlsModeCode = tlsModeCode,
            TrustServerCertificate = true
        };

    // islevi: Update validator testi icin tum zorunlu alanlari gecerli DTO kurar.
    private static UpdateDatabaseConnectionDto UpdateDto(string tlsModeCode)
        => new()
        {
            EngineId = Guid.NewGuid(),
            Name = "connection",
            Host = "database.example",
            Port = 5432,
            DatabaseName = "sample",
            IsActive = true,
            TlsModeCode = tlsModeCode,
            TrustServerCertificate = true
        };
}

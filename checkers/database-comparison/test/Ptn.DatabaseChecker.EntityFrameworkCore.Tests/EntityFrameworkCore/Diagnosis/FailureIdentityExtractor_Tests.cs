using Ptn.DatabaseChecker.Constants.Comparison;
using Ptn.DatabaseChecker.Constants.Diagnosis;
using Ptn.DatabaseChecker.Managers.Diagnosis;
using Ptn.DatabaseChecker.Models.Diagnosis;
using Shouldly;
using Xunit;
using DomainPostgreSqlFailureIdentityExtractor = Ptn.DatabaseChecker.Managers.Diagnosis.PostgreSqlFailureIdentityExtractor;
using DomainSqlServerFailureIdentityExtractor = Ptn.DatabaseChecker.Managers.Diagnosis.SqlServerFailureIdentityExtractor;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Diagnosis;

// islevi: PostgreSQL ve SQL Server extractor'larinin mesaj parse etmeden yapilandirilmis kimlik guveni uretmesini dogrular.
// sistemdeki gorevi: SQLSTATE sinif-23 High guveni ve SQL Server ad cikarimi yapmayan Low guven sinirinin regresyon kanitidir.
public class FailureIdentityExtractor_Tests
{
    // islevi: PostgreSQL sinif-23 SQLSTATE'in structured alanlarla High guvenli kimlik urettigini dogrular.
    [Fact]
    public void PostgreSql_Class_23_Should_Use_Structured_Fields_With_High_Confidence()
    {
        var extractor = new DomainPostgreSqlFailureIdentityExtractor();
        var signal = CreateDatabaseSignal(
            DatabaseEngineCodes.PostgreSql,
            PostgreSqlSqlStateCodes.ForeignKeyViolation);
        signal.DbException!.ProviderFields[PostgreSqlSqlStateCodes.ProviderFields.ConstraintName] = "fk_orders_customer";

        var identity = extractor.Extract(signal);

        identity.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.High);
        identity.CodeClassCode.ShouldBe(FailureCodeClassCodes.IntegrityConstraint);
        identity.ObjectReferences.Single().ConstraintName.ShouldBe("fk_orders_customer");
    }

    // islevi: SQL Server extractor'in adlari atip yalniz numara ve Low guven tuttugunu dogrular.
    [Fact]
    public void SqlServer_Should_Keep_Only_Number_And_Low_Confidence()
    {
        var extractor = new DomainSqlServerFailureIdentityExtractor();
        var signal = CreateDatabaseSignal(DatabaseEngineCodes.SqlServer, "547");
        signal.DbException!.ProviderFields[PostgreSqlSqlStateCodes.ProviderFields.ConstraintName] = "FK_should_not_escape";

        var identity = extractor.Extract(signal);

        identity.Code.ShouldBe("547");
        identity.ConfidenceCode.ShouldBe(DiagnosisConfidenceCodes.Low);
        identity.ObjectReferences.ShouldBeEmpty();
    }

    // islevi: Extractor testine engine ve provider hata kodu tasiyan yapilandirilmis sinyal kurar.
    private static FailureSignal CreateDatabaseSignal(string engineCode, string errorCode)
        => new()
        {
            DbException = new FailureSignal.DatabaseExceptionFailureSignal
            {
                EngineCode = engineCode,
                SqlState = errorCode
            }
        };
}

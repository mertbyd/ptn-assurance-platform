using Ptn.DatabaseChecker.Connections;
using Ptn.DatabaseChecker.ExceptionCodes;
using Shouldly;
using Xunit;

namespace Ptn.DatabaseChecker.EntityFrameworkCore.Comparison;

// islevi: PostgreSQL ve SQL Server privilege probe bayraklarinin ortak uyari modeline cevrilmesini dogrular.
// sistemdeki gorevi: Yazma/yuksek rolun hata yerine ExcessivePrivilege bulgusu, salt-okuma rolun uyarisiz sonuc vermesini korur.
public class EnginePrivilegeProbe_Tests
{
    [Fact]
    public void PostgreSql_Write_Privilege_Should_Return_Warning()
    {
        var result = PostgreSqlEnginePrivilegeProbe.CreateResult(
            hasWriteAllDataRole: true,
            isSuperUser: false,
            canCreateInDatabase: false);

        result.CanWrite.ShouldBeTrue();
        result.IsSuperUser.ShouldBeFalse();
        result.WarningCode.ShouldBe(DatabaseConnectionExceptionCodes.ExcessivePrivilege);
    }

    [Fact]
    public void PostgreSql_Read_Only_Privilege_Should_Not_Return_Warning()
    {
        var result = PostgreSqlEnginePrivilegeProbe.CreateResult(
            hasWriteAllDataRole: false,
            isSuperUser: false,
            canCreateInDatabase: false);

        result.CanWrite.ShouldBeFalse();
        result.IsSuperUser.ShouldBeFalse();
        result.WarningCode.ShouldBeNull();
    }

    [Fact]
    public void SqlServer_DataWriter_Should_Return_Warning()
    {
        var result = SqlServerEnginePrivilegeProbe.CreateResult(
            isSysAdmin: false,
            isDatabaseOwner: false,
            isDataWriter: true);

        result.CanWrite.ShouldBeTrue();
        result.IsSuperUser.ShouldBeFalse();
        result.WarningCode.ShouldBe(DatabaseConnectionExceptionCodes.ExcessivePrivilege);
    }

    [Fact]
    public void SqlServer_Read_Only_Privilege_Should_Not_Return_Warning()
    {
        var result = SqlServerEnginePrivilegeProbe.CreateResult(
            isSysAdmin: false,
            isDatabaseOwner: false,
            isDataWriter: false);

        result.CanWrite.ShouldBeFalse();
        result.IsSuperUser.ShouldBeFalse();
        result.WarningCode.ShouldBeNull();
    }
}

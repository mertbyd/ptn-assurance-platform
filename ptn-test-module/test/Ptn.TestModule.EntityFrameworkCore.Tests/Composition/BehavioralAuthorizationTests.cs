using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ptn.TestModule.Dtos.Bridge;
using Ptn.TestModule.Permissions;
using Ptn.TestModule.Services.Bridge;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Ptn.TestModule.EntityFrameworkCore.Tests.Composition;

// islevi: Bridge yuzeyi icin davranissal yetki kapisi testleri.
// sistemdeki gorevi: AddAlwaysAllowAuthorization() kapaliyken dogru claims kimliklerinin reddedilip kabul edildigini kanitlar.
public class BehavioralAuthorizationTests : TestModuleTestBase<TestModuleIsolatedAuthTestModule>
{
    private readonly IPtnBridgeAppService _bridgeAppService;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public BehavioralAuthorizationTests()
    {
        _bridgeAppService = GetRequiredService<IPtnBridgeAppService>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    [Fact]
    public async Task Unauthorized_identity_should_be_rejected_with_authorization_exception()
    {
        // Arrange: Hicbir yetkisi (claim) olmayan bos bir kimlik
        using (_currentPrincipalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity())))
        {
            // Act & Assert
            var ex = await Record.ExceptionAsync(async () =>
            {
                await _bridgeAppService.GroundAsync(CreateValidDto());
            });
            
            if (ex is not AbpAuthorizationException)
            {
                throw new Exception($"Expected AbpAuthorizationException, but got {ex?.GetType().Name}: {ex?.Message}\n{ex?.StackTrace}", ex);
            }
        }
    }

    [Fact]
    public async Task Authorized_identity_should_pass_authorization_gate()
    {
        // Arrange: Sadece Ground yetkisine sahip kimlik
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("TestPermission", TestModulePermissions.Bridge.Ground));
        
        using (_currentPrincipalAccessor.Change(new ClaimsPrincipal(identity)))
        {
            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _bridgeAppService.GroundAsync(CreateValidDto());
            });

            // Assert: AbpAuthorizationException firlatmamali. Diger hatalar (validation vb) gelebilir.
            if (exception != null)
            {
                exception.ShouldNotBeOfType<AbpAuthorizationException>();
            }
        }
    }

    private static GroundRequestDto CreateValidDto()
    {
        return new GroundRequestDto
        {
            ProfileKey = "valid-profile",
            SpecSnapshotId = Guid.NewGuid(),
            ConnectionId = Guid.NewGuid(),
            StepIntent = "test",
            ResponseFormat = Ptn.TestModule.Constants.Bridge.PtnResponseFormatCodes.Concise
        };
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CheckNexus.ApiContracts;
using CheckNexus.DatabaseComparison;
using CheckNexus.Vault;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using ModelContextProtocol.AspNetCore;
using Ptn.TestModule.Mcp;
using Pintern.Notifications;
using Piton.Emailing;
using Ptn.TestModule.Constants;
using Ptn.TestModule.EntityFrameworkCore;
using Ptn.TestModule.MultiTenancy;
using StackExchange.Redis;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Swashbuckle;
using Volo.Abp.VirtualFileSystem;

namespace Ptn.TestModule;

// islevi: Test Module composition hostunun HTTP pipeline'ini, yetenek modullerini ve kimlik dogrulamasini kurar.
// sistemdeki gorevi: Iki checker'i, Notifications'i, Emailing'i ve Test Module katmanlarini tek deploy edilen
// uygulamada birlestirir; kimligi ayri deploy edilen Authenticator'dan bearer token ile tuketir (ADR-0005).
[DependsOn(
    typeof(TestModuleApplicationModule),
    typeof(TestModuleEntityFrameworkCoreModule),
    typeof(TestModuleHttpApiModule),
    typeof(ApiContractCheckerModule),
    typeof(DatabaseCheckerModule),
    typeof(CheckNexusVaultModule),
    typeof(NotificationsHttpApiModule),
    typeof(EmailingApplicationModule),
    typeof(EmailingHttpApiModule),
    typeof(EmailingInfrastructureModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpAutofacModule),
    typeof(AbpBlobStoringFileSystemModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
)]
public class TestModuleHttpApiHostModule : AbpModule
{
    private const string SwaggerDocumentName = "v1";
    private const string SwaggerScopeName = "TestModule";
    private const string ApplicationName = "TestModule";

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        Configure<AbpDbContextOptions>(options => options.UseNpgsql());
        Configure<AbpMultiTenancyOptions>(options => options.IsEnabled = MultiTenancyConsts.IsEnabled);

        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureLocalization();
        ConfigureExternalBearerAuthentication(context, configuration);
        ConfigureCache(configuration);
        ConfigureBlobStoring(configuration, hostingEnvironment);
        ConfigureDataProtection(context, configuration, hostingEnvironment);
        ConfigureSwagger(context, configuration);
        ConfigureCors(context, configuration);

        context.Services.AddHealthChecks();
        context.Services.AddMcpServer()
            .WithHttpTransport()
            .WithTools<PtnMcpTools>()
            .WithResources<AuthoringRulesResource>();
    }

    // Gelistirmede embedded localization dosyalari yerine kaynak dosyalari kullanilir.
    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (!hostingEnvironment.IsDevelopment())
        {
            return;
        }

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.ReplaceEmbeddedByPhysical<TestModuleDomainSharedModule>(SourceProjectPath(hostingEnvironment, "Ptn.TestModule.Domain.Shared"));
            options.FileSets.ReplaceEmbeddedByPhysical<TestModuleDomainModule>(SourceProjectPath(hostingEnvironment, "Ptn.TestModule.Domain"));
            options.FileSets.ReplaceEmbeddedByPhysical<TestModuleApplicationContractsModule>(SourceProjectPath(hostingEnvironment, "Ptn.TestModule.Application.Contracts"));
            options.FileSets.ReplaceEmbeddedByPhysical<TestModuleApplicationModule>(SourceProjectPath(hostingEnvironment, "Ptn.TestModule.Application"));
        });
    }

    private static string SourceProjectPath(IHostEnvironment hostingEnvironment, string projectName)
    {
        return Path.Combine(hostingEnvironment.ContentRootPath, "..", "..", "src", projectName);
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("tr", "tr", "Turkce"));
        });
    }

    // Kimlik sahibi ayri deploy edilen Authenticator'dir; bu host yalniz bearer token dogrular.
    private static void ConfigureExternalBearerAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddAbpJwtBearer(options =>
            {
                options.Authority = configuration[TestModuleConfigurationKeys.AuthServerAuthority];
                options.Audience = configuration[TestModuleConfigurationKeys.AuthServerAudience];
                options.RequireHttpsMetadata = configuration.GetValue(TestModuleConfigurationKeys.AuthServerRequireHttpsMetadata, true);
            });
    }

    private void ConfigureCache(IConfiguration configuration)
    {
        Configure<AbpDistributedCacheOptions>(options => options.KeyPrefix = $"{ApplicationName}:");
    }

    // Kosum artefaktlari ve HAR govdeleri dosya sisteminde durur; saglayici olmadan BLOB Storing kurulamaz.
    private void ConfigureBlobStoring(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        var basePath = configuration[TestModuleConfigurationKeys.BlobStoringFileSystemBasePath];
        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = Path.Combine(hostingEnvironment.ContentRootPath, "blobs");
        }

        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.ConfigureAll((_, container) =>
                container.UseFileSystem(fileSystem => fileSystem.BasePath = basePath));
        });
    }

    private static void ConfigureDataProtection(
        ServiceConfigurationContext context,
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
    {
        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName(ApplicationName);
        if (hostingEnvironment.IsDevelopment())
        {
            return;
        }

        var redis = ConnectionMultiplexer.Connect(configuration[TestModuleConfigurationKeys.RedisConfiguration]!);
        dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, $"{ApplicationName}-Protection-Keys");
    }

    private static void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration[TestModuleConfigurationKeys.AuthServerAuthority]!,
            new Dictionary<string, string>
            {
                { SwaggerScopeName, "Test Module API" }
            },
            options =>
            {
                options.SwaggerDoc(SwaggerDocumentName, new OpenApiInfo
                {
                    Title = "Test Module API",
                    Version = SwaggerDocumentName
                });
                options.DocInclusionPredicate((_, _) => true);
                options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);

                IncludeXmlDocumentation(options, "Ptn.TestModule.HttpApi.xml");
                IncludeXmlDocumentation(options, "Ptn.TestModule.Application.Contracts.xml");
            });
    }

    private static void IncludeXmlDocumentation(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options, string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, fileName);
        if (File.Exists(path))
        {
            options.IncludeXmlComments(path, includeControllerXmlComments: true);
        }
    }

    private static void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var origins = configuration[TestModuleConfigurationKeys.CorsOrigins]?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(origin => origin.TrimEnd('/'))
            .ToArray() ?? [];

        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(origins)
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var environment = context.GetEnvironment();

        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseCorrelationId();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseAbpRequestLocalization();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{SwaggerDocumentName}/swagger.json", "Test Module API");
            options.OAuthClientId(context.GetConfiguration()[TestModuleConfigurationKeys.AuthServerSwaggerClientId]);
            options.OAuthScopes(SwaggerScopeName);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            endpoints.MapMcp("/mcp").RequireAuthorization();
        });
    }
}

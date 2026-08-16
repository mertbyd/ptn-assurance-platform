using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using CheckNexus.ApiContracts;
using Ptn.ApiContractChecker.EntityFrameworkCore;
using Ptn.ApiContractChecker.MultiTenancy;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Swashbuckle;
using Volo.Abp.VirtualFileSystem;

namespace Ptn.ApiContractChecker;

[DependsOn(
    typeof(ApiContractCheckerModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
)]
// islevi: API Contract Checker modulunun gelistirme zamani HTTP pipeline'ini kurar.
// sistemdeki gorevi: Auth veya Notification sahibi olmadan controller, DI, Swagger ve EF migration entegrasyonunu dogrular.
public class ApiContractCheckerHttpApiHostModule : AbpModule
{
    private const string SwaggerDocumentName = "v1";
    private const string AutoMigrateConfigurationKey = "Database:AutoMigrate";
    private const string SeedOnStartupConfigurationKey = "Database:SeedOnStartup";

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        ConfigureExternalBearerAuthentication(context);
        ConfigureApiBehavior();
        ConfigureMultiTenancy();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureLocalization();
        ConfigureSwagger(context);
        ConfigureCors(context);
        context.Services.AddHealthChecks();
    }

    private static void ConfigureExternalBearerAuthentication(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.Audience = configuration["AuthServer:Audience"];
                options.RequireHttpsMetadata = configuration.GetValue("AuthServer:RequireHttpsMetadata", true);
            });
    }

    private void ConfigureApiBehavior()
    {
        Configure<AbpAntiForgeryOptions>(options => options.AutoValidate = false);
    }

    private void ConfigureMultiTenancy()
    {
        Configure<AbpMultiTenancyOptions>(options => options.IsEnabled = MultiTenancyConsts.IsEnabled);
    }

    private void ConfigureVirtualFileSystem(IHostEnvironment hostingEnvironment)
    {
        if (!hostingEnvironment.IsDevelopment())
        {
            return;
        }

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.ReplaceEmbeddedByPhysical<ApiContractCheckerDomainSharedModule>(SourceProjectPath(hostingEnvironment, "Ptn.ApiContractChecker.Domain.Shared"));
            options.FileSets.ReplaceEmbeddedByPhysical<ApiContractCheckerDomainModule>(SourceProjectPath(hostingEnvironment, "Ptn.ApiContractChecker.Domain"));
            options.FileSets.ReplaceEmbeddedByPhysical<ApiContractCheckerApplicationContractsModule>(SourceProjectPath(hostingEnvironment, "Ptn.ApiContractChecker.Application.Contracts"));
            options.FileSets.ReplaceEmbeddedByPhysical<ApiContractCheckerApplicationModule>(SourceProjectPath(hostingEnvironment, "Ptn.ApiContractChecker.Application"));
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

    private static void ConfigureSwagger(ServiceConfigurationContext context)
    {
        context.Services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc(SwaggerDocumentName, new OpenApiInfo
            {
                Title = "API Contract Checker API",
                Version = SwaggerDocumentName
            });
            options.DocInclusionPredicate((_, _) => true);
            options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Authenticator tarafindan uretilen JWT access token."
            });
            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", null),
                    new List<string>()
                }
            });

            IncludeXmlDocumentation(options, "Ptn.ApiContractChecker.HttpApi.xml");
            IncludeXmlDocumentation(options, "Ptn.ApiContractChecker.Application.Contracts.xml");
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

    private static void ConfigureCors(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var origins = configuration["App:CorsOrigins"]?
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

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
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
            options.SwaggerEndpoint($"/swagger/{SwaggerDocumentName}/swagger.json", "API Contract Checker API");
        });
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints(endpoints => endpoints.MapHealthChecks("/health"));

        await MigrateAndSeedAsync(context);
    }

    private static async Task MigrateAndSeedAsync(ApplicationInitializationContext context)
    {
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
        var autoMigrate = configuration.GetValue<bool>(AutoMigrateConfigurationKey);
        var seedOnStartup = configuration.GetValue<bool>(SeedOnStartupConfigurationKey);
        if (!autoMigrate && !seedOnStartup)
        {
            return;
        }

        using var scope = context.ServiceProvider.CreateScope();
        if (autoMigrate)
        {
            await scope.ServiceProvider
                .GetRequiredService<ApiContractCheckerDbContext>()
                .Database
                .MigrateAsync();
        }

        if (seedOnStartup)
        {
            await scope.ServiceProvider
                .GetRequiredService<IDataSeeder>()
                .SeedAsync();
        }
    }
}

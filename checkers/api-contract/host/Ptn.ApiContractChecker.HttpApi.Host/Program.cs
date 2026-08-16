using Serilog;
using Serilog.Events;

namespace Ptn.ApiContractChecker;

// islevi: API Contract Checker gelistirme ve HTTP dogrulama hostunu baslatir.
// sistemdeki gorevi: Paketlenmeyen ince composition katmani olarak checker modulunu dis Authenticator JWT'leriyle calistirir.
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(sink => sink.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting API Contract Checker validation host.");

            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.secrets.json", optional: true, reloadOnChange: true);
            builder.Host.UseAutofac().UseSerilog();

            await builder.AddApplicationAsync<ApiContractCheckerHttpApiHostModule>();

            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception exception) when (exception is not HostAbortedException)
        {
            Log.Fatal(exception, "API Contract Checker validation host terminated unexpectedly.");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}

using AotServiceProvider;
using Authi.Common.Services;
using Authi.Server;
using Authi.Server.ApiVersions;
using Authi.Server.Database;
using Authi.Server.Extensions;
using Authi.Server.Services;
using Authi.Server.Web;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;

[assembly: GenerateServiceConfig(
    typeof(Clock),             // Authi.Common
    typeof(Program)            // Authi.Server
)]

namespace Authi.Server
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            AotServiceProvider.Services.Init(new ServiceConfig());

            var configuration = AotServiceProvider.Services.Current.Get<IConfiguration>();
            var healthMonitor = AotServiceProvider.Services.Current.Get<IAppHealthMonitor>();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services
                .NoCors();                          // The API is never used by browser clients

            var app = builder.Build()
                .OnException(healthMonitor.ReportEvent)
                .OnApplicationStopping(healthMonitor.Flush);

            if (configuration.Exists)
            {
                using var db = AotServiceProvider.Services.Current.Get<IDatabase>().CreateScope();
                await db.CleanUpAsync();

                app.MapApiVersion(new ApiV1());
                app.MapApiVersion(new DashboardPage());
                app.MapApiVersion(new HealthPage());
            }
            app.MapApiVersion(new ConfigPage());
            app.NoCors();

            app.Run();
        }
    }
}

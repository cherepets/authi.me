using Authi.Common.Services;
using Authi.Server.ApiVersions;
using Authi.Server.Database;
using Authi.Server.Extensions;
using Authi.Server.Services;
using Authi.Server.Web;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using ServiceProvider = Authi.Common.Services.ServiceProvider;

namespace Authi.Server
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            ServiceLocator.Init(
                typeof(ServiceLocator).Assembly,    // Authi.Common
                typeof(Program).Assembly);          // Authi.Server

            var configuration = ServiceProvider.Current.Get<IConfiguration>();
            var healthMonitor = ServiceProvider.Current.Get<IAppHealthMonitor>();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services
                .NoCors();                          // The API is never used by browser clients

            var app = builder.Build()
                .OnException(healthMonitor.ReportEvent)
                .OnApplicationStopping(healthMonitor.Flush);

            if (configuration.Exists)
            {
                using var db = ServiceProvider.Current.Get<IDatabase>().CreateScope();
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

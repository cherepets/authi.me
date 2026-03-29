using Authi.Common.Services;
using Authi.Server.ApiVersions;
using Authi.Server.Extensions;
using Authi.Server.Services;
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

            var healthMonitor = ServiceProvider.Current.Get<IAppHealthMonitor>();
            var dbContext = ServiceProvider.Current.Get<IAppDbContext>();

            await dbContext.CleanUpAsync();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .NoCors();                          // The API is never used by browser clients

            var app = builder.Build()
                .NoCors()
                .OnException(healthMonitor.ReportEvent)
                .OnApplicationStopping(healthMonitor.Flush);

            app.MapApiVersion(new ApiV1());
            app.MapApiVersion(new HealthApi());

#if DEBUG
            app.MapApiVersion(new DebugApi());
#endif

            app.Run();
        }
    }
}
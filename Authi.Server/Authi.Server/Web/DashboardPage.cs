using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Globalization;
using System.Threading.Tasks;

namespace Authi.Server.Web
{
    public class DashboardPage : WebPageBase
    {
        public override void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/", async Task<IResult> (HttpContext context) =>
                await AuthorizedPage(
                    context,
                    async () => Results.Content(
                        Html("dashboard")
                            .Replace(
                                "{{DB_SIZE}}", 
                                await GetDbSize()),
                        ContentType)));

            app.MapPost("/auth", (HttpContext context) =>
            {
                return TryAuthorize(context, out var error)
                    ? Results.Ok()
                    : Results.BadRequest(error);
            });
        }

        private async Task<string> GetDbSize()
        {
            long bytes;
            try
            {
                bytes = await Services.Database.GetSizeAsync();
            }
            catch (System.Exception exception)
            {
                Services.AppHealthMonitor.ReportEvent(exception);
                return "Unavailable";
            }

            string[] units = ["B", "KB", "MB", "GB", "TB"];
            var size = (double)bytes;
            var unit = 0;

            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return $"{size.ToString(unit == 0 ? "0" : "0.##", CultureInfo.InvariantCulture)} {units[unit]}";
        }
    }

}

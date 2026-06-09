using Authi.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Authi.Server.Web
{
    public class HealthPage : WebPageBase
    {
        public override void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/health", (HttpContext context) =>
                AuthorizedPage(
                    context,
                    () => Results.Content(
                        Html("health")
                            .Replace("{{DATA}}", Services.AppHealthMonitor.GetEvents().ToJson()),
                        ContentType)));
        }
    }
}

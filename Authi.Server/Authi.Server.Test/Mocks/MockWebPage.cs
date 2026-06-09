using Authi.Server.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Authi.Server.Test.Mocks
{
    internal class MockWebPage : WebPageBase
    {
        public bool IsAuthorized(HttpContext context) => TryAuthorize(context, out _);
        public override void ConfigureRoutes(IEndpointRouteBuilder app) { }
    }
}

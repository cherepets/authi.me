using AotServiceProvider.Abstractions;
using Authi.Server.Services;
using Microsoft.AspNetCore.Routing;

namespace Authi.Server.ApiVersions
{
    public abstract class ApiVersionBase : IServiceConsumer
    {
        public abstract void ConfigureRoutes(IEndpointRouteBuilder app);

        #region IServiceConsumer
        internal virtual IServiceConsumer Services => this;
        IServices IServiceConsumer.ServiceProvider => AotServiceProvider.Services.Current;
        #endregion
    }
}

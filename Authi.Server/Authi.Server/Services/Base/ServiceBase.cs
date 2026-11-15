using AotServiceProvider.Abstractions;

namespace Authi.Server.Services
{
    public abstract class ServiceBase : IServiceConsumer
    {
        #region IServiceConsumer
        internal virtual IServiceConsumer Services => this;
        IServices IServiceConsumer.ServiceProvider => AotServiceProvider.Services.Current;
        #endregion
    }
}

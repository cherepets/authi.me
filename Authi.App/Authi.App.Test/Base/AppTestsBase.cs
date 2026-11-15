using AotServiceProvider;
using AotServiceProvider.Abstractions;
using Authi.App.Logic;
using Authi.App.Logic.Services;
using Authi.App.Test;
using Authi.Common.Services;
using Authi.Common.Test;

[assembly: GenerateServiceConfig(
    typeof(Clock),             // Authi.Common
    typeof(Config),            // Authi.App.Logic
    typeof(AppTestsBase)       // Authi.App.Test
)]

namespace Authi.App.Test
{
    public class AppTestsBase : TestsBase, IServiceConsumer
    {
        protected override IConfig CreateServiceConfig() => new ServiceConfig();

        #region IServiceConsumer
        internal virtual IServiceConsumer Services => this;
        IServices IServiceConsumer.ServiceProvider => AotServiceProvider.Services.Current;
        #endregion
    }
}

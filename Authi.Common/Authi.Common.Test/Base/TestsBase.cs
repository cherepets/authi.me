using AotServiceProvider;
using AotServiceProvider.Abstractions;
using AotServiceProvider.Testing;
using Authi.Common.Services;

[assembly: GenerateServiceConfig(
    typeof(Clock)              // Authi.Common
)]

namespace Authi.Common.Test
{
    public class TestsBase
    {
        private MockServices? _servicesMock;

        public MockServices ServicesMock
        {
            get => _servicesMock!;
            protected set => _servicesMock = value;
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            ServicesMock = new MockServices(CreateServiceConfig());
            AotServiceProvider.Services.Hide(ServicesMock);
            InitServices();
        }

        protected virtual IConfig CreateServiceConfig() => new ServiceConfig();

        protected virtual void InitServices()
        {
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            _servicesMock = null;
        }
    }
}

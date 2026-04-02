using Authi.App.Logic.Services;
using System;
using System.Threading.Tasks;

namespace Authi.App.Test.Mocks
{
    internal class MockSettings : ISettings
    {
        public IAsyncValueSetting<Guid> ClientId { get; } = new MockAsyncValueSetting<Guid>();
        public IAsyncValueSetting<Guid> Version { get; } = new MockAsyncValueSetting<Guid>();
        public IAsyncSetting<byte[]> SyncPrivateKey { get; } = new MockAsyncSetting<byte[]>();
        public IAsyncSetting<byte[]> SyncPublicKey { get; } = new MockAsyncSetting<byte[]>();
        public IAsyncSetting<byte[]> DataKey { get; } = new MockAsyncSetting<byte[]>();

        public class MockAsyncSetting<T> : IAsyncSetting<T> where T : class
        {
            private T? _value;

            public ValueTask<T?> GetAsync()
            {
                return ValueTask.FromResult(_value);
            }

            public ValueTask SetAsync(T? value)
            {
                _value = value;
                return ValueTask.CompletedTask;
            }
        }

        public class MockAsyncValueSetting<T> : IAsyncValueSetting<T> where T : struct
        {
            private T? _value;

            public ValueTask<T?> GetAsync()
            {
                return ValueTask.FromResult(_value);
            }

            public ValueTask SetAsync(T? value)
            {
                _value = value;
                return ValueTask.CompletedTask;
            }
        }
    }
}

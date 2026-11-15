using AotServiceProvider.Abstractions;
using Authi.App.Logic.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Authi.App.Logic.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IServiceConsumer
    {
        private readonly Dictionary<string, object?> _valueStore = [];

        protected T? Get<T>([CallerMemberName] string? property = null)
        {
            Debug.Assert(property != null);

            return _valueStore.TryGetValue(property, out var value) && value is T typed
                ? typed
                : default;
        }

        protected T Set<T>(T value, [CallerMemberName] string? property = null)
        {
            Debug.Assert(property != null);

            _valueStore[property] = value!;
            OnPropertyChanged(property);
            return value;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region IServiceConsumer
        internal virtual IServiceConsumer Services => this;
        IServices IServiceConsumer.ServiceProvider => AotServiceProvider.Services.Current;
        #endregion
    }
}

using AotServiceProvider;
using System;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    public interface IDialogManager
    {
        Task ShowDialogAsync(string title, string message, string? primaryButtonText = null, string? cancelButtonText = null, Action? onPrimary = null, Action? onCancel = null);
    }

    public interface IDialogManagerView
    {
        Task ShowDialogAsync(string title, string message, string? primaryButtonText = null, string? cancelButtonText = null, Action? onPrimary = null, Action? onCancel = null);
    }

    public class DialogManager : IDialogManager
    {
        private static IDialogManagerView? _view;

        public Task ShowDialogAsync(string title, string message, string? primaryButtonText = null, string? cancelButtonText = null, Action? onPrimary = null, Action? onCancel = null)
            => _view?.ShowDialogAsync(title, message, primaryButtonText, cancelButtonText, onPrimary, onCancel) ?? Task.CompletedTask;

        public static void Register(IDialogManagerView view)
        {
            _view = view;
        }
    }
}

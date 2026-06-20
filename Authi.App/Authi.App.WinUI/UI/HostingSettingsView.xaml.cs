using Authi.App.Logic.ViewModels;

namespace Authi.App.WinUI.UI
{
    public sealed partial class HostingSettingsView
    {
        public SettingsViewModel? ViewModel { get; set; }

        public HostingSettingsView()
        {
            InitializeComponent();
        }
    }
}

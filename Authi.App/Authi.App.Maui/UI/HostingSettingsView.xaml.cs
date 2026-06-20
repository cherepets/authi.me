using Authi.App.Logic.ViewModels;
using System;

namespace Authi.App.Maui.UI;

public partial class HostingSettingsView
{
    private SettingsViewModel ViewModel => BindingContext as SettingsViewModel;

    public HostingSettingsView()
	{
		InitializeComponent();
    }

    private void OnLearnMore(object sender, EventArgs e)
    {
        ViewModel?.LearnMore();
    }
}
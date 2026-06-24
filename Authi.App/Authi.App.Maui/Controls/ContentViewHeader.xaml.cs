using Authi.App.Maui.UI;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;

namespace Authi.App.Maui.Controls;

public partial class ContentViewHeader : IAdaptiveView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(ContentViewHeader), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public event EventHandler? Close;

    public ContentViewHeader()
    {
        BindingContext = this;

        InitializeComponent();
    }

    public void SetCompactSize(bool isCompact)
    {
        if (isCompact)
        {
            BackButton.IsVisible = true;
            CloseButton.IsVisible = false;
        }
        else
        {
            Margin = new Thickness(0);
            BackButton.IsVisible = false;
            CloseButton.IsVisible = true;
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close?.Invoke(this, e);
    }
}
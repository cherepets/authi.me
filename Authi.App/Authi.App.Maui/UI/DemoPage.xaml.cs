using MaterialColorUtilities.Maui;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace Authi.App.Maui.UI;

public partial class DemoPage
{
    public IReadOnlyCollection<DemoColor> Colors { get; }

    public DemoPage()
    {
        Colors = new List<DemoColor>
        {
            new("Primary"),
            new("PrimaryContainer"),
            new("Secondary"),
            new("SecondaryContainer"),
            new("Tertiary"),
            new("TertiaryContainer"),
            new("Surface"),
            new("SurfaceVariant"),
            new("Background"),
            new("Error"),
            new("ErrorContainer"),
            new("OnPrimary"),
            new("OnPrimaryContainer"),
            new("OnSecondary"),
            new("OnSecondaryContainer"),
            new("OnTertiary"),
            new("OnTertiaryContainer"),
            new("OnSurface"),
            new("OnSurfaceVariant"),
            new("OnError"),
            new("OnErrorContainer"),
            new("OnBackground"),
            new("Outline"),
            new("Shadow"),
            new("InverseSurface"),
            new("InverseOnSurface"),
            new("InversePrimary"),
            new("Surface1"),
            new("Surface2"),
            new("Surface3"),
            new("Surface4"),
            new("Surface5")
        };
        BindingContext = this;
        InitializeComponent();
    }
}

public class DemoColor
{
    public string Name { get; }
    public Color Color { get; }
    public Color Foreground { get; }

    public DemoColor(string name)
    {
        Name = name;
        Color = AuthiApp.Current.GetResource<Color>(name);
        var l = Color.GetLuminosity();
        Foreground = l > 0.5f ? Colors.Black : Colors.White;
    }
}
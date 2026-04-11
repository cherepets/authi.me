using Authi.App.Logic.ViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.Maui.Converters
{
    public class SyncStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (SyncStatus)value;
            return status switch
            {
                SyncStatus.NotSynced => AuthiApp.Current.GetThemedResource("ColorNeutral"),
                SyncStatus.Offline => AuthiApp.Current.GetThemedResource("ColorOffline"),
                SyncStatus.Syncing => AuthiApp.Current.GetThemedResource("ColorCaution"),
                SyncStatus.Synced => AuthiApp.Current.GetThemedResource("ColorSuccess"),
                SyncStatus.Error => AuthiApp.Current.GetThemedResource("ColorCritical"),
                _ => throw new ArgumentOutOfRangeException(nameof(value), status.ToString())
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SyncStatusToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (SyncStatus)value;
            return status switch
            {
                SyncStatus.NotSynced => L10n.SyncStatus.NotSynced,
                SyncStatus.Offline => L10n.SyncStatus.Offline,
                SyncStatus.Syncing => L10n.SyncStatus.Syncing,
                SyncStatus.Synced => L10n.SyncStatus.Synced,
                SyncStatus.Error => L10n.SyncStatus.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(value), status.ToString())
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

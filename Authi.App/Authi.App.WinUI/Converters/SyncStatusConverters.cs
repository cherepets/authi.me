using Authi.App.Logic.ViewModels;
using Microsoft.UI.Xaml.Data;
using System;

namespace Authi.App.WinUI.Converters
{
    public partial class SyncStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (SyncStatus)value;
            return status switch
            {
                SyncStatus.NotSynced => App.Current.GetResource("SystemFillColorSolidNeutralBrush"),
                SyncStatus.Offline => App.Current.GetResource("SystemFillColorCriticalBackgroundBrush"),
                SyncStatus.Syncing => App.Current.GetResource("SystemFillColorCautionBrush"),
                SyncStatus.Synced => App.Current.GetResource("SystemFillColorSuccessBrush"),
                SyncStatus.Error => App.Current.GetResource("SystemFillColorCriticalBrush"),
                _ => throw new ArgumentOutOfRangeException(nameof(value), status.ToString())
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class SyncStatusToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (SyncStatus)value;
            return status switch
            {
                SyncStatus.NotSynced => "not synced",
                SyncStatus.Offline => "offline",
                SyncStatus.Syncing => "syncing...",
                SyncStatus.Synced => "synced",
                SyncStatus.Error => "sync error!",
                _ => throw new ArgumentOutOfRangeException(nameof(value), status.ToString())
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

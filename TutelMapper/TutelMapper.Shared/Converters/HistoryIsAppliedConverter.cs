#nullable enable
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TutelMapper.Converters
{
    public class HistoryIsAppliedConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool isApplied))
                return null;
            if (isApplied)
                return Application.Current.Resources["DefaultTextForegroundThemeBrush"];
            return "Gray";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
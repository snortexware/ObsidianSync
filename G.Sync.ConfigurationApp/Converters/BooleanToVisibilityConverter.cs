using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace G.Sync.ConfigurationApp.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool invert = parameter?.ToString() == "Invert";
            bool b = (bool)value;
            if (invert) b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
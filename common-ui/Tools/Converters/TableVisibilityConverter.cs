using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BaseApp.Resource.Converters
{
    public class TableVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the item has children, then show the checkbox, otherwise hide it
            return value != null ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

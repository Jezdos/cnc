using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UI.Tools.Converters
{
    public class BaseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentPanel = value.ToString();
            var targetPanel = parameter as string;

            if (currentPanel == targetPanel)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Globalization;
using System.Windows.Data;

namespace UI.Tools.Converters
{
    public class BaseNullZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            int val;
            return int.TryParse((string)value, out val) ? val : 0;
        }
    }
}

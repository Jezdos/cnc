using System.Globalization;
using System.Windows.Data;

namespace BaseApp.Resource.Converters
{
    public class EnumIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType().IsEnum)
            {
                Array enumValues = Enum.GetValues(value.GetType());
                return Array.IndexOf(enumValues, value);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

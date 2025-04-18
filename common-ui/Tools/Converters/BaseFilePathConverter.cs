

using Core.Utils;
using System.Globalization;
using System.Windows.Data;

namespace UI.Tools.Converters
{
    public class BaseFilePathConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BaseFileUtil.GetOriFilePath(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

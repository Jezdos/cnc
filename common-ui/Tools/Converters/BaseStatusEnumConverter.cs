
using Core.Enums;
using System.Globalization;
using System.Windows.Data;

namespace UI.Tools.Converters
{
    public class BaseStatusEnumConverter : IValueConverter
    {
        //Convert方法中的value参数，为绑定源数据，此例中为string类型
        //如果value为NORMAL，则返回true，否则返回false
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BaseStatusEnum.NORMAL == (BaseStatusEnum)value;
        }
        //ConvertBack方法的value参数，为绑定目标数据，此例中为BaseStatusEnum类型
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? BaseStatusEnum.NORMAL : BaseStatusEnum.EXCEPTION;
        }
    }
}

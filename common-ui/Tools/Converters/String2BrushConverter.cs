﻿using HandyControl.Tools;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UI.Tools.Converters;

public class String2BrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string str ? ResourceHelper.GetResource<Brush>(str) : default;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

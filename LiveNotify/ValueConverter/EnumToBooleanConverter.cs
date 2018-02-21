using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveNotify.ValueConverter
{
    class EnumToBooleanConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                {
                    return Enum.Parse(targetType, parameter.ToString(), true);
                }
            }
            catch
            {

            }
            return null;
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;
using System.Collections.Generic;
using Reactive.Bindings;

namespace LiveNotify.ValueConverter
{
    public class FavoriteMatchedToStringConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var streams = (value as ReactiveCollection<Models.LiveItem>);
            if (streams != null)
            {
                if (0 < streams.Count)
                {
                    return $"{streams[0].Url.ToString()}";
                }
                return string.Empty;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

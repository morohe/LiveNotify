using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace LiveNotify.ValueConverter
{
    /// <summary>
    /// Thanks!
    /// https://stackoverflow.com/questions/11274731/dynamic-generate-column-mvvm
    /// </summary>
    public class LiveItemToGridColumnConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var columns = (value as Models.LiveItemColumn[]);
            if (columns != null)
            {
                var view = new GridView();
                foreach (var i in columns)
                {
                    var binding = new Binding(i.DataMemberPath);
                    view.Columns.Add(new GridViewColumn { Header = i.Header, DisplayMemberBinding = binding});
                }
                return view;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"{nameof(LiveItemToGridColumnConverter)}.{nameof(ConvertBack)}");
        }
    }
}

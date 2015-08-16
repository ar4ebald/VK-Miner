using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VK_Miner.Converters
{
    class ObjectToVisibilityConverter : IValueConverter
    {
        private static bool ToBool(object value)
        {
            if (value == null)
                return false;

            if (value is bool)
                return (bool)value;
            if (value is int)
                return (int)value != 0;
            if (value is long)
                return (long)value != 0;

            var strValue = value as string;
            if (strValue != null)
                return strValue != string.Empty;

            return true;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ToBool(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

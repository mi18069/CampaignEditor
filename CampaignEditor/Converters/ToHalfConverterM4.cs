using System.Globalization;
using System;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class ToHalfConverterM4 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2 - 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

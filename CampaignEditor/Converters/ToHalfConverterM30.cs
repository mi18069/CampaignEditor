using System.Globalization;
using System;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class ToHalfConverterM30 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2 - 30;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

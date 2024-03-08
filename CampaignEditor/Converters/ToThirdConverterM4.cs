using System;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class ToThirdConverterM4 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 3 - 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

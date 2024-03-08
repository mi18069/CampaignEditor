using System;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class DateComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                DateTime date = TimeFormat.YMDStringToDateTime((string)value);
                return date < DateTime.Now;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
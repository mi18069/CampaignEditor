using System;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class DateStrToDateOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is string == true)
                return TimeFormat.YMDStringToDateOnly(value.ToString()).ToShortDateString();
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

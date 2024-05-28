using System;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class TimeStrToRepresentativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string timestr)
            {
                return TimeFormat.TimeStrToRepresentative(timestr);
            }

            return string.Empty; // Return an empty string if the conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

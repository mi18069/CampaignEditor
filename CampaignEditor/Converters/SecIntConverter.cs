using System.Globalization;
using System;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class SecIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && int.TryParse(stringValue, out int intValue))
            {
                int hours = intValue / 3600; // Calculate the number of hours
                int minutes = (intValue % 3600) / 60; // Calculate the remaining minutes

                return hours.ToString().PadLeft(2, '0') + " : " + minutes.ToString().PadLeft(2, '0');
            }

            // Handle the case when the value is not of type string or cannot be parsed to int
            return string.Empty;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

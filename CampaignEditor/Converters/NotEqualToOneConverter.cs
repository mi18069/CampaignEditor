
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CampaignEditor.Converters
{
    public class NotEqualToOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal progCoef && progCoef != 1.0M)
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

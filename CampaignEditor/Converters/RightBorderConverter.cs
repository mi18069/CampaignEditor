using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class RightBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasRightBorder && hasRightBorder)
            {
                // Return a thicker right border thickness
                return new Thickness(0, 0, 2, 0);
            }

            // Return the default border thickness
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

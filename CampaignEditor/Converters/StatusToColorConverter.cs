using System;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class StatusToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is int status && values[1] is int statusAD)
            {
                if (statusAD == 2)
                    return Brushes.Gray;
                if (statusAD == 1)
                    return Brushes.DodgerBlue;

                if (status == 5)
                    return Brushes.OrangeRed;
                if (status == 1)
                    return Brushes.LightGreen;

            }

            // Default background color if no condition is met
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

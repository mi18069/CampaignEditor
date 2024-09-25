using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CampaignEditor.Converters
{
    public class StatusRealizedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                if (status == -1)
                    return Brushes.Transparent;
                if (status == 0)
                    return Brushes.LightGray;
                if (status == 1)
                    return Brushes.LightGreen;
                if (status == 2)
                    return Brushes.Orange;
                if (status == 3)
                    return Brushes.Yellow;
                if (status == 4)
                    return Brushes.LightGoldenrodYellow;
                if (status == 5)
                    return Brushes.Red;
                if (status == 6)
                    return Brushes.PaleVioletRed;

            }

            // Default background color if no condition is met
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

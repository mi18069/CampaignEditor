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
    public class TermStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                return status switch
                {
                    0 => Brushes.Transparent,
                    1 => Brushes.Green,
                    2 => Brushes.Red,
                    3 => Brushes.Yellow,
                    _ => Brushes.Transparent, // Default color for unknown status
                };
            }

            return Brushes.Transparent; // Default color for null or invalid status
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

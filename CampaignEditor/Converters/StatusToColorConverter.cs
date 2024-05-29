using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                switch (intValue)
                {
                    case -1: return Brushes.Transparent;
                    case 0: return Brushes.LightGray;
                    case 1: return Brushes.LightGreen;
                    case 2: return Brushes.OrangeRed;
                    case 5: return Brushes.Violet;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

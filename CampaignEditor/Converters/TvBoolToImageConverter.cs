using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CampaignEditor.Converters
{
    public class TvBoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool tv)
            {
                return tv ? new BitmapImage(new Uri("\\images\\tv_icon.png", UriKind.RelativeOrAbsolute)) :
                            new BitmapImage(new Uri("\\images\\radio_icon1.png", UriKind.RelativeOrAbsolute));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

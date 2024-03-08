using System;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class UserLevelToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int userLevel && parameter is object parameterObject)
            {
                if (int.TryParse(parameterObject.ToString(), out int requiredValue))
                {
                    return userLevel <= requiredValue;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

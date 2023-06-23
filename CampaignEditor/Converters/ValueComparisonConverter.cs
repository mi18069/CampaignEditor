using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CampaignEditor.Converters
{
    public class ValueComparisonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double firstValue && values[1] is string secondValue)
            {
                var numericValue = double.TryParse(secondValue.TrimStart('/'), out var result) ? result : 0;
                if (numericValue == 0)
                    return Brushes.Black;
                else if (firstValue > numericValue)
                    return Brushes.Green;
                else
                    return Brushes.Red;
            }
            else if (values.Length == 2 && values[0] is int firstIntValue && values[1] is string secondIntValue)
            {
                var numericValue = int.TryParse(secondIntValue.TrimStart('/'), out var result) ? result : 0;
                if (numericValue == 0)
                    return Brushes.Black;
                else if (firstIntValue > numericValue)
                    return Brushes.Green;
                else
                    return Brushes.Red;
            }
            return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
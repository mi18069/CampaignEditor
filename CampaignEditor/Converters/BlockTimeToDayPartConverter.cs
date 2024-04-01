using Database.DTOs.DayPartDTO;
using Database.DTOs.DPTimeDTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class BlockTimeToDayPartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string time && parameter is IDictionary<DayPartDTO, List<DPTimeDTO>> dayPartsDict)
            {
                foreach (var dayPart in dayPartsDict.Keys)
                {
                    foreach (var dpTime in dayPartsDict[dayPart])
                    {
                        if ((String.Compare(dpTime.stime, time) <= 0) &&
                            (String.Compare (dpTime.etime, time) >= 0))
                        {
                            return dayPart.name;
                        }
                    }
                }
            }

            return string.Empty; // Return an empty string if the conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

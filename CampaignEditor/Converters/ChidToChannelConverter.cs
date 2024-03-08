using CampaignEditor.Controllers;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class ChidToChannelConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int chid && parameter is IDictionary<int, string> channelDictionary)
            {
                if (channelDictionary.ContainsKey(chid))
                {
                    return channelDictionary[chid];
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

using System;
using System.Data;
using System.Globalization;
using System.Windows.Data;

namespace CampaignEditor.Converters
{
    public class HeaderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int ind = -1;
            DataView dv = values[0] as DataView;
            if (dv != null)
            {
                DataRowView drv = values[0] as DataRowView;
                ind = dv.Table.Rows.IndexOf(drv.Row);
            }
            else
            {
                System.Collections.IEnumerable ien = values[0] as System.Collections.IEnumerable;
                ind = IndexOf(ien, values[1]);
            }
            if (ind == -1)
                return "";
            else
                return (ind + 1).ToString();
        }
        static int IndexOf(System.Collections.IEnumerable source, object value)
        {
            int index = 0;
            foreach (var item in source)
            {
                if (item.Equals(value))
                    return index;
                index++;
            }
            return -1;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Windows.Controls;

namespace CampaignEditor
{
    public static class TimeFormat
    {
        public static string? DPToYMDString(DatePicker dp)
        {
            DateTime? date = dp.SelectedDate;
            if (date == null)
            {
                return null;
            }

            string year = DateTime.Parse(date.ToString()!).Year.ToString().PadLeft(4, '0');
            string month = DateTime.Parse(date.ToString()!).Month.ToString().PadLeft(2,'0');
            string day = DateTime.Parse(date.ToString()!).Day.ToString().PadLeft(2,'0');

            return year + month + day;
        }
    }
}

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
        public static DateTime YMDStringToDateTime(string timeString)
        {
            int year = int.Parse(timeString.Substring(0, 4));
            int month = int.Parse(timeString.Substring(4, 2));
            int day = int.Parse(timeString.Substring(6, 2));

            return new DateTime(year, month, day);
        }
    }
}

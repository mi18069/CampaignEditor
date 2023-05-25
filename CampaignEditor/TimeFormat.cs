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
        public static string? DTToTimeString(DateTime dt)
        {

            string hour = DateTime.Parse(dt.ToString()!).Hour.ToString().PadLeft(2, '0');
            string minute = DateTime.Parse(dt.ToString()!).Minute.ToString().PadLeft(2, '0');
            string second = DateTime.Parse(dt.ToString()!).Second.ToString().PadLeft(2, '0');

            return hour + minute + second;
        }

        public static int? DPToYMDInt(DatePicker dp)
        {
            DateTime? date = dp.SelectedDate;
            if (date == null)
            {
                return null;
            }

            string year = DateTime.Parse(date.ToString()!).Year.ToString().PadLeft(4, '0');
            string month = DateTime.Parse(date.ToString()!).Month.ToString().PadLeft(2, '0');
            string day = DateTime.Parse(date.ToString()!).Day.ToString().PadLeft(2, '0');

            return Int32.Parse(year + month + day);
        }

        public static DateTime YMDStringToDateTime(string timeString)
        {
            int year = int.Parse(timeString.Substring(0, 4));
            int month = int.Parse(timeString.Substring(4, 2));
            int day = int.Parse(timeString.Substring(6, 2));

            return new DateTime(year, month, day);
        }
        public static string YMDStringToRepresentative(string timeString)
        {
            int year = int.Parse(timeString.Substring(0, 4));
            int month = int.Parse(timeString.Substring(4, 2));
            int day = int.Parse(timeString.Substring(6, 2));

            return day + "." + month + "." + year;
        }

        public static int Time5CharCompare(string time1, string time2)
        {
            int dot1Index = time1.IndexOf(':');
            int h1 = Int32.Parse(time1.Substring(0, dot1Index));
            int m1 = Int32.Parse(time1.Substring(dot1Index + 1));

            int dot2Index = time2.IndexOf(':');
            int h2 = Int32.Parse(time2.Substring(0, dot2Index));
            int m2 = Int32.Parse(time2.Substring(dot2Index + 1));

            if (h1 > h2)
                return 1;
            else if (h1 < h2)
                return -1;
            else if (h1 == h2 && m1 > m2)
                return 1;
            else if (h1 == h2 && m1 < m2)
                return -1;
            else
                return 0;
        }

        public static string SecIntToReporesentative(int secInt)
        {
            int hours = secInt / 3600; // Calculate the number of hours
            int minutes = (secInt % 3600) / 60; // Calculate the remaining minutes

            return hours.ToString().PadLeft(2, '0') + " : " + minutes.ToString().PadLeft(2, '0');
        }

    }
}

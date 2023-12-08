using System;
using System.Globalization;
using System.Linq;
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

        public static int CompareRepresentative(string time1, string time2)
        {
            int h1, h2, m1, m2; 
            if (Int32.TryParse(time1.Substring(0, 2), out h1) &&
                Int32.TryParse(time1.Substring(3, 2), out m1) &&
                Int32.TryParse(time2.Substring(0, 2), out h2) &&
                Int32.TryParse(time2.Substring(3, 2), out m2))
            {
                return h1 < h2 ? -1 : (h1 > h2 ? 1 : (m1 < m2 ? -1 : (m1 > m2 ? 1 : 0)));
            }
            return 2;
        }

        public static DateTime? IntToDateTime(int date)
        {

            string dateString = date.ToString("00000000"); // Convert the int to a zero-padded string

            DateTime dateTime;
            if (DateTime.TryParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                // Parsing successful, 'dateTime' variable now holds the parsed date
                return dateTime;
            }
            else
            {
                // Parsing failed
                Console.WriteLine("Invalid date");
                return null;
            }
        }

        public static int DateTimeToInt(DateTime date)
        {
            return int.Parse(date.ToString("yyyyMMdd"));

        }

        public static string MinToRepresentative(int timeInMins)
        {
            int hours = timeInMins / 60;
            int minutes = timeInMins % 60;
            string hoursStr = hours.ToString().PadLeft(2, '0');
            string minutesStr = minutes.ToString().PadLeft(2, '0');
            return hoursStr + ":" + minutesStr;
            
        }

        public static int CalculateMinutesBetweenRepresentatives(string time1, string time2)
        {
            // Parse the hours and minutes from the time strings
            if (DateTime.TryParseExact(time1, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime1) &&
                DateTime.TryParseExact(time2, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime2))
            {
                // Calculate the time difference in minutes
                TimeSpan timeDifference = dateTime2 - dateTime1;
                return (int)timeDifference.TotalMinutes;
            }
            else
            {
                // Handle invalid time format
                throw new ArgumentException("Invalid time format. Use 'hh:mm' format.");
            }
        }

        // Transforms hhmm format to hh:mm
        public static string ReturnGoodTimeFormat(string timeString)
        {
            if (timeString == null)
            {
                return null;
            }
            else if (timeString.Trim() == "")
            {
                return "";
            }
            else if (timeString.Length == 4)
            {
                int mins, hours;
                if (timeString.Any(c => !Char.IsDigit(c)))
                {
                    throw new ArgumentException("Invalid timeString format");
                }

                if (Int32.TryParse(timeString.Substring(2, 2), out mins) && 
                    Int32.TryParse(timeString.Substring(0, 2), out hours))
                {
                    if (mins > 59)
                    {
                        throw new ArgumentException("Invalid timeString format");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid timeString format");
                }

                return timeString.Substring(0,2) + ":" + timeString.Substring(2,2);
            }
            else if (timeString.Length == 5)
            {
                int hours, mins;
                if (Int32.TryParse(timeString.Substring(0, 2), out hours) &&
                    Int32.TryParse(timeString.Substring(3, 2), out mins))
                {
                    if (mins > 59)
                    {
                        throw new ArgumentException("Invalid timeString format");
                    }
                    return timeString.Substring(0, 2) + ":" + timeString.Substring(3, 2);
                }
                else
                {
                    throw new ArgumentException("Invalid timeString format");
                }
            }
            else
            {
                throw new ArgumentException("Invalid timeString format");
            }
            return null;
        }
    }
}

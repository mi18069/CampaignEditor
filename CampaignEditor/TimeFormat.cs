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

        public static DateOnly YMDStringToDateOnly(string timeString)
        {
            int year = int.Parse(timeString.Substring(0, 4));
            int month = int.Parse(timeString.Substring(4, 2));
            int day = int.Parse(timeString.Substring(6, 2));

            return new DateOnly(year, month, day);
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

        public static TimeOnly GetAverageTime(TimeOnly time1, TimeOnly time2)
        {
            int minutes1 = time1.Hour * 60 + time1.Minute;
            int minutes2 = time2.Hour * 60 + time2.Minute;

            int averageMinutes = (minutes1 + minutes2) / 2;

            return new TimeOnly(averageMinutes / 60, averageMinutes % 60, 0);
        }

        public static string TimeOnlyToRepresentative(TimeOnly timeOnly)
        {
            return $"{timeOnly.Hour.ToString().PadLeft(2, '0')}:{timeOnly.Minute.ToString().PadLeft(2, '0')}";
        }

        public static TimeOnly? RepresentativeToTimeOnly(string timeString)
        {
            // Parse the time string and convert it to TimeOnly
            if (TimeOnly.TryParseExact(timeString, "HH:mm", out var time))
            {
                return time;
            }
            else
            {
                return null;
            }

        }

        public static bool IsGoodRepresentativeTimeFormat(string timeString)
        {
            if (timeString == null || timeString.Length != 5)
            {
                return false;
            }
            if (timeString[2] != ':')
            {
                return false;
            }
            int hours, minutes;
            if (!int.TryParse(timeString.Substring(0, 2), out hours) ||
                !int.TryParse(timeString.Substring(3, 2), out minutes))
            {
                return false;
            }

            if (hours >= 02 && hours <= 25 && minutes >= 0 && minutes <= 59)
            {
                return true;
            }
            
            return false;
        }

        public static string SecIntToReporesentative(int secInt)
        {
            int hours = secInt / 3600; // Calculate the number of hours
            int minutes = (secInt % 3600) / 60; // Calculate the remaining minutes

            return hours.ToString().PadLeft(2, '0') + " : " + minutes.ToString().PadLeft(2, '0');
        }

        public static string TimeStrToRepresentative(string timeStr)
        {           
            return timeStr.Substring(0, 2) + ":" + timeStr.Substring(2, 2);
        }

        public static int GetDayOfWeekInt(string dateString)
        {
            // Parse the date string to a DateTime object
            DateTime date = DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture);

            // Get the day of the week
            DayOfWeek dayOfWeek = date.DayOfWeek;

            // Map the DayOfWeek enum to the desired integer values (Monday = 1, ..., Sunday = 7)
            int dayOfWeekInt = dayOfWeek switch
            {
                DayOfWeek.Monday => 1,
                DayOfWeek.Tuesday => 2,
                DayOfWeek.Wednesday => 3,
                DayOfWeek.Thursday => 4,
                DayOfWeek.Friday => 5,
                DayOfWeek.Saturday => 6,
                DayOfWeek.Sunday => 7,
            };

            return dayOfWeekInt;
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

        public static int RepresentativeToMins(string timeString)
        {
            if (!IsGoodRepresentativeTimeFormat(timeString))
            {
                return -1;
            }
            
            int hours = int.Parse(timeString.Substring(0, 2));
            int minutes = int.Parse(timeString.Substring(3, 2));

            return hours * 60 + minutes;
        }

        public static int CalculateMinutesBetweenRepresentatives(string time1, string time2)
        {
            // Parse the hours and minutes from the time strings
            /*if (DateTime.TryParseExact(time1, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime1) &&
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
            }*/
            return Math.Abs(RepresentativeToMins(time2) - RepresentativeToMins(time1));

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

        public static int GetWeeksBetween(DateTime startDate, DateTime endDate)
        {
            // Calculate the time difference between two dates
            int firstWeekNum = GetWeekOfYear(startDate);
            int lastWeekNum = GetWeekOfYear(endDate);

            if (firstWeekNum > lastWeekNum)
                lastWeekNum = lastWeekNum + TimeFormat.GetWeeksInYear(startDate.Year);

            // Calculate the number of weeks
            int weeksBetween;
            if (firstWeekNum <= lastWeekNum)
                weeksBetween = lastWeekNum - firstWeekNum;
            else
                weeksBetween = TimeFormat.GetWeeksInYear(startDate.Year) - firstWeekNum + lastWeekNum;

            return weeksBetween + 1;
        }

        public static int GetWeekOfYear(DateTime date)
        {
            System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            System.Globalization.Calendar calendar = cultureInfo.Calendar;

            System.Globalization.DateTimeFormatInfo dtfi = cultureInfo.DateTimeFormat;
            dtfi.FirstDayOfWeek = DayOfWeek.Monday;

            return calendar.GetWeekOfYear(date, dtfi.CalendarWeekRule, dtfi.FirstDayOfWeek);
        }
        public static int GetWeekOfYear(DateOnly date)
        {
            DateTime dateTime = date.ToDateTime(TimeOnly.Parse("00:01 AM"));
            return GetWeekOfYear(dateTime);
        }

        public static int GetWeeksInYear(int year)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            System.Globalization.Calendar calendar = dfi.Calendar;

            int weeksInYear = calendar.GetWeekOfYear(new DateTime(year, 12, 31), dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

            return weeksInYear;
        }
    }
}

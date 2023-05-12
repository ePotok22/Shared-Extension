using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FFF.Shared
{
    public static class DateTimeExtensions
    {
        public static string ToLocalizedString(this DateTime dateTime) =>
            dateTime.ToString((IFormatProvider)CultureInfo.CurrentUICulture.DateTimeFormat);

        public static string ToLocalizedShortString(this DateTime dateTime) =>
            dateTime.ToString("d", (IFormatProvider)CultureInfo.CurrentUICulture.DateTimeFormat);

        public static string FirstDayOfMonth(this DateTime date) =>
            new DateTime(date.Year, date.Month, 1).ToString("MM/dd/yyyy");

        public static string LastDayOfMonth(this DateTime date) =>
            new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1, 1).AddDays(-1).ToString("MM/dd/yyyy");

        public static IEnumerable<DateTime> GetDateRangeTo(this DateTime self, DateTime toDate)
        {
            IEnumerable<int> range = Enumerable.Range(0, new TimeSpan(toDate.Ticks - self.Ticks).Days);

            return from p in range
                   select self.Date.AddDays(p);
        }

        public static bool IsWeekend(this DateTime value) =>
            value.DayOfWeek == DayOfWeek.Sunday || value.DayOfWeek == DayOfWeek.Saturday;

        public static bool IsWeekend(this DayOfWeek d) =>
            !d.IsWeekday();

        public static bool IsWeekday(this DayOfWeek d)
        {
            switch (d)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday: return false;
                default: return true;
            }
        }

        public static DateTime GetLastDayOfMonth(this DateTime dateTime) =>
            new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1).AddDays(-1);

        public static DateTime AddWorkdays(this DateTime d, int days)
        {
            // start from a weekday
            while (d.DayOfWeek.IsWeekday()) d = d.AddDays(1.0);
            for (int i = 0; i < days; ++i)
            {
                d = d.AddDays(1.0);
                while (d.DayOfWeek.IsWeekday()) d = d.AddDays(1.0);
            }
            return d;
        }

        public static bool Intersects(this DateTime startDate, DateTime endDate, DateTime intersectingStartDate, DateTime intersectingEndDate) =>
            intersectingEndDate >= startDate && intersectingStartDate <= endDate;

        public static int Age(this DateTime dateOfBirth)
        {
            if (DateTime.Today.Month < dateOfBirth.Month ||
            DateTime.Today.Month == dateOfBirth.Month &&
             DateTime.Today.Day < dateOfBirth.Day)
                return DateTime.Today.Year - dateOfBirth.Year - 1;
            else
                return DateTime.Today.Year - dateOfBirth.Year;
        }

        /// <summary>
        /// Converts a System.DateTime object to Unix timestamp
        /// </summary>
        /// <returns>The Unix timestamp</returns>
        public static long ToUnixTimestamp(this DateTime date)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan unixTimeSpan = date - unixEpoch;

            return (long)unixTimeSpan.TotalSeconds;
        }

        public static string ToRFC822Date(this DateTime date)
        {
            int offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours;
            string timeZone = "+" + offset.ToString().PadLeft(2, '0');

            if (offset < 0)
            {
                int i = offset * -1;
                timeZone = "-" + i.ToString().PadLeft(2, '0');
            }

            return date.ToString("ddd, dd MMM yyyy HH:mm:ss " + timeZone.PadRight(5, '0'));
        }

        public static int DateDiff(this DateTime StartDate, String DatePart, DateTime EndDate)
        {
            int DateDiffVal = 0;
            Calendar cal = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar;
            TimeSpan ts = new TimeSpan(EndDate.Ticks - StartDate.Ticks);
            switch (DatePart.ToLower().Trim())
            {
                #region year
                case "year":
                case "yy":
                case "yyyy":
                    DateDiffVal = (int)(cal.GetYear(EndDate) - cal.GetYear(StartDate));
                    break;
                #endregion

                #region quarter
                case "quarter":
                case "qq":
                case "q":
                    DateDiffVal = (int)((((cal.GetYear(EndDate)
                                        - cal.GetYear(StartDate)) * 4)
                                        + ((cal.GetMonth(EndDate) - 1) / 3))
                                        - ((cal.GetMonth(StartDate) - 1) / 3));
                    break;
                #endregion

                #region month
                case "month":
                case "mm":
                case "m":
                    DateDiffVal = (int)(((cal.GetYear(EndDate)
                                        - cal.GetYear(StartDate)) * 12
                                        + cal.GetMonth(EndDate))
                                        - cal.GetMonth(StartDate));
                    break;
                #endregion

                #region day
                case "day":
                case "d":
                case "dd":
                    DateDiffVal = (int)ts.TotalDays;
                    break;
                #endregion

                #region week
                case "week":
                case "wk":
                case "ww":
                    DateDiffVal = (int)(ts.TotalDays / 7);
                    break;
                #endregion

                #region hour
                case "hour":
                case "hh":
                    DateDiffVal = (int)ts.TotalHours;
                    break;
                #endregion

                #region minute
                case "minute":
                case "mi":
                case "n":
                    DateDiffVal = (int)ts.TotalMinutes;
                    break;
                #endregion

                #region second
                case "second":
                case "ss":
                case "s":
                    DateDiffVal = (int)ts.TotalSeconds;
                    break;
                #endregion

                #region millisecond
                case "millisecond":
                case "ms":
                    DateDiffVal = (int)ts.TotalMilliseconds;
                    break;
                #endregion

                default:
                    throw new Exception(string.Format("DatePart \"{0}\" is unknown", DatePart));
            }
            return DateDiffVal;
        }

        public static bool IsBetween(this DateTime dt, DateTime startDate, DateTime endDate, bool compareTime = false)
        {
            return compareTime ?
               dt >= startDate && dt <= endDate :
               dt.Date >= startDate.Date && dt.Date <= endDate.Date;
        }
    }
}

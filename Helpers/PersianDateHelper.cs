using System;
using System.Globalization;

namespace Vakilaw.Helpers
{
    public static class PersianDateHelper
    {
        private static readonly PersianCalendar _persianCalendar = new PersianCalendar();

        /// <summary>
        /// تبدیل DateTime میلادی به رشته تاریخ شمسی با فرمت yyyy/MM/dd
        /// </summary>
        public static string ToPersianDate(DateTime dateTime)
        {
            int year = _persianCalendar.GetYear(dateTime);
            int month = _persianCalendar.GetMonth(dateTime);
            int day = _persianCalendar.GetDayOfMonth(dateTime);

            return $"{year:0000}/{month:00}/{day:00}";
        }

        /// <summary>
        /// تبدیل DateTime میلادی به رشته تاریخ شمسی با فرمت دلخواه
        /// </summary>
        public static string ToPersianDate(DateTime dateTime, string format)
        {
            int year = _persianCalendar.GetYear(dateTime);
            int month = _persianCalendar.GetMonth(dateTime);
            int day = _persianCalendar.GetDayOfMonth(dateTime);

            return format
                .Replace("yyyy", year.ToString("0000"))
                .Replace("MM", month.ToString("00"))
                .Replace("dd", day.ToString("00"));
        }
    }
}
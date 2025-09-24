using System.Globalization;

namespace Vakilaw.Models
{
    public class SmsHistoryItem
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // تاریخ میلادی که توی دیتابیس ذخیره میشه
        public DateTime? SetDate { get; set; }

        public string StatusText { get; set; } = string.Empty;
        public bool IsGroup { get; set; }

        // تاریخ شمسی فقط برای نمایش
        public string DateShamsi => ToShamsi(SetDate, true);

        private static string ToShamsi(DateTime? dt, bool showTime = false)
        {
            if (dt == null) return " -";

            var local = dt.Value.Kind == DateTimeKind.Utc ? dt.Value.ToLocalTime() : dt.Value;
            var pc = new PersianCalendar();

            int y = pc.GetYear(local);
            int m = pc.GetMonth(local);
            int d = pc.GetDayOfMonth(local);

            string datePart = $"{y:0000}/{m:00}/{d:00}";

            if (!showTime)
                return datePart;

            int hh = local.Hour;
            int mm = local.Minute;
            return $"{datePart} {hh:00}:{mm:00}";
        }
    }
}
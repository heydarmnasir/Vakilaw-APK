using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vakilaw.Models
{
    public class LicenseInfo
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;   // چون همیشه باید مقدار داشته باشه
        public string LicenseKey { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string SubscriptionType { get; set; } = "Trial";
    }
}
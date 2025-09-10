using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vakilaw.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Type { get; set; } // "Trial", "3Month", "6Month", "Yearly"
        public bool IsTrial { get; set; }
        public string? PaymentTrackingCode { get; set; }
    }
}
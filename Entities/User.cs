using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vakilaw.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; } // "Lawyer" یا "Client"
        public string? LicenseNumber { get; set; } // فقط برای وکلا
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
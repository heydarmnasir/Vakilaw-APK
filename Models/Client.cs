using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vakilaw.Models
{
    public class Client
    {
        public int Id { get; set; } // Primary Key
        public string FullName { get; set; } // نام و نام خانوادگی
        public string NationalCode { get; set; } // کد ملی
        public string PhoneNumber { get; set; } // شماره تماس
        public string Address { get; set; } // آدرس
        public string Description { get; set; } // توضیحات اضافی

        // Navigation
        public List<Case> Cases { get; set; } = new();
    }
}
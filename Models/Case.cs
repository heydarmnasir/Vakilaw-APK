using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vakilaw.Models
{
    public class Case
    {
        public int Id { get; set; } // Primary Key
        public string Title { get; set; } // عنوان پرونده
        public string CaseNumber { get; set; } // شماره پرونده
        public string CourtName { get; set; } // نام دادگاه
        public string JudgeName { get; set; } // نام قاضی
        public string StartDate { get; set; } // تاریخ شروع
        public string? EndDate { get; set; } // تاریخ پایان (اختیاری)
        public string Status { get; set; } // وضعیت (جاری، مختومه، در حال بررسی و …)
        public string Description { get; set; } // توضیحات اضافی

        // ✨ پراپرتی جدید برای فایل‌های پیوست
        public List<CaseAttachment> CaseAttachments { get; set; } = new();

        // Foreign Key
        public int ClientId { get; set; }
        public Client Client { get; set; }
    }

    public class CaseAttachment
    {
        public int Id { get; set; }
        public int CaseId { get; set; } // کلید خارجی به پرونده
        public string FileName { get; set; } // مثلا name.pdf
        public string FilePath { get; set; } // مسیر ذخیره روی دیسک
        public string FileType { get; set; } // عکس، pdf و …
    }
}
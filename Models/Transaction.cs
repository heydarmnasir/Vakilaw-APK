namespace Vakilaw.Models;

public class Transaction
{
    public int Id { get; set; }             // کلید اصلی
    public string Title { get; set; }       // عنوان تراکنش
    public double Amount { get; set; }      // مبلغ
    public bool IsIncome { get; set; }      // درآمد یا هزینه
    public DateTime Date { get; set; }      // تاریخ
    public string Description { get; set; } // توضیحات
}
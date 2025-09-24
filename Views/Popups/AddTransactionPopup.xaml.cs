using Mopups.Pages;
using Mopups.Services;
using Vakilaw.Services;
using Vakilaw.ViewModels;

namespace Vakilaw.Views.Popups;

public partial class AddTransactionPopup : PopupPage
{
    public AddTransactionPopup(TransactionService service, Func<Task> onAdded)
    {
        InitializeComponent();
        BindingContext = new AddTransactionPopupVM(service, async () =>
        {
            await onAdded();
            await MopupService.Instance.PopAsync(); // بستن پاپ‌آپ بعد از ذخیره
        });
    }

    private bool _isUpdating = false;

    private void AmountEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        var entry = (Entry)sender;
        if (string.IsNullOrWhiteSpace(entry.Text))
            return;

        // حذف هر چیزی غیر از عدد
        string numeric = new string(entry.Text.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(numeric))
        {
            entry.Text = "";
            return;
        }

        if (long.TryParse(numeric, out long value))
        {
            _isUpdating = true;

            string formatted = string.Format("{0:N0}", value);
            entry.Text = formatted;

            // ست کردن مکان‌نما، فقط اگر طول معتبر بود
            int newPos = formatted.Length;
            if (newPos <= entry.Text.Length)
                entry.CursorPosition = newPos;

            _isUpdating = false;
        }
    }


}
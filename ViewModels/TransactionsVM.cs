using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using Vakilaw.Services;
using Vakilaw.Views;
using Vakilaw.Views.Popups;

namespace Vakilaw.ViewModels;

public partial class TransactionsVM : ObservableObject
{
    private readonly TransactionService _transactionService;

    [ObservableProperty]
    private ObservableCollection<Transaction> transactions = new();

    public TransactionsVM(TransactionService transactionService)
    {
        _transactionService = transactionService;
        LoadTransactions().SafeFireAndForget();
    }

    // 📌 بارگذاری تراکنش‌ها
    private async Task LoadTransactions()
    {
        Transactions.Clear();
        var list = await _transactionService.GetAll();
        foreach (var t in list)
            Transactions.Add(t);
    }

    // 📌 نمایش پاپ‌آپ افزودن تراکنش
    [RelayCommand]
    private async Task ShowAddTransactionPopup()
    {
        var popup = new AddTransactionPopup(_transactionService, async () =>
        {
            await LoadTransactions(); // بعد از ثبت، لیست آپدیت میشه
        });

        await MopupService.Instance.PushAsync(popup);
    }

    // 📌 حذف تراکنش
    [RelayCommand]
    private async Task DeleteTransaction(Transaction transaction)
    {
        if (transaction == null) return;

        await _transactionService.Delete(transaction.Id);
        await LoadTransactions();
    }
}
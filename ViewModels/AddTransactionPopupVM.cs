using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vakilaw.Models;
using Vakilaw.Services;

namespace Vakilaw.ViewModels;

public partial class AddTransactionPopupVM : ObservableObject
{
    private readonly TransactionService _transactionService;
    private readonly Func<Task> _onTransactionAdded;

    [ObservableProperty] private string title;
    [ObservableProperty] private double amount;
    [ObservableProperty] private bool isIncome = true;
    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private string description;

    public AddTransactionPopupVM(TransactionService transactionService, Func<Task> onTransactionAdded)
    {
        _transactionService = transactionService;
        _onTransactionAdded = onTransactionAdded;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Title) || Amount <= 0)
            return; // میشه بعداً Validation بهتر گذاشت

        var transaction = new Transaction
        {
            Title = Title,
            Amount = Amount,
            IsIncome = IsIncome,
            Date = Date,
            Description = Description
        };

        await _transactionService.Add(transaction);

        if (_onTransactionAdded != null)
            await _onTransactionAdded.Invoke();
    }
}
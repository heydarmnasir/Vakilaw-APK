using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using Vakilaw.Models;
using Vakilaw.Views;

namespace Vakilaw.ViewModels;

public partial class AddCasePopupViewModel : ObservableObject
{
    private readonly AddCasePopup _popup;  
    private readonly Client _client;

    [ObservableProperty] private string clientName;

    public AddCasePopupViewModel(AddCasePopup popup, Client client)
    {
        _popup = popup;      
        _client = client;
        ClientName = client.FullName;
        StartDate = DateTime.Now;
    }

    // برای ذخیره در DB نیاز داریم
    public int ClientId => _client.Id;

    [ObservableProperty] private string title;
    [ObservableProperty] private string caseNumber;
    [ObservableProperty] private string courtName;
    [ObservableProperty] private string judgeName;
    [ObservableProperty] private DateTime startDate;
    [ObservableProperty] private DateTime? endDate;
    [ObservableProperty] private string status;
    [ObservableProperty] private string description;

    [RelayCommand]
    private async Task Save()
    {
        var newCase = new Case
        {
            Title = Title,
            CaseNumber = CaseNumber,
            CourtName = CourtName,
            JudgeName = JudgeName,
            StartDate = StartDate,
            EndDate = EndDate,
            Status = Status,
            Description = Description,
            ClientId = _client.Id,
            Client = _client
        };

        // فقط ارسال به View (ClientsAndCasesViewModel)
        _popup.RaiseCaseCreated(newCase);

        // بستن Popup
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await MopupService.Instance.PopAsync();
    }
}
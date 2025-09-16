using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using Vakilaw.Models;

namespace Vakilaw.ViewModels
{
    public partial class CaseDetailsPopupViewModel : ObservableObject
    {
        [ObservableProperty] private string title;
        [ObservableProperty] private string caseNumber;
        [ObservableProperty] private string courtName;
        [ObservableProperty] private string judgeName;
        [ObservableProperty] private string startDate;
        [ObservableProperty] private string endDate;
        [ObservableProperty] private string status;
        [ObservableProperty] private string description;

        public CaseDetailsPopupViewModel(Case caseItem)
        {
            Title = caseItem.Title;
            CaseNumber = caseItem.CaseNumber;
            CourtName = caseItem.CourtName;
            JudgeName = caseItem.JudgeName;
            StartDate = caseItem.StartDate;         
            Status = caseItem.Status;
            EndDate = caseItem.EndDate;
            Description = caseItem.Description;
        }

        [RelayCommand]
        private async Task Close()
        {
            await MopupService.Instance.PopAsync();
        }
    }
}
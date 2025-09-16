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
        [ObservableProperty] private string status;
        [ObservableProperty] private string description;

        public CaseDetailsPopupViewModel(Case caseItem)
        {
            Title = caseItem.Title;
            CaseNumber = caseItem.CaseNumber;
            CourtName = caseItem.CourtName;
            JudgeName = caseItem.JudgeName;
            Status = caseItem.Status;
            Description = caseItem.Description;
        }

        [RelayCommand]
        private async Task Close()
        {
            await MopupService.Instance.PopAsync();
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using Vakilaw.Views.Popups;

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
        [ObservableProperty] private ObservableCollection<CaseAttachment> caseAttachments = new();

        private readonly CaseService _caseService;
        private readonly Case _caseItem; // نگهداری نمونهٔ اصلی

        public CaseDetailsPopupViewModel(Case caseItem, CaseService caseService)
        {
            _caseItem = caseItem ?? throw new ArgumentNullException(nameof(caseItem));
            _caseService = caseService ?? throw new ArgumentNullException(nameof(caseService));

            // مقداردهی اولیه از نمونه
            Title = _caseItem.Title;
            CaseNumber = _caseItem.CaseNumber;
            CourtName = _caseItem.CourtName;
            JudgeName = _caseItem.JudgeName;
            StartDate = _caseItem.StartDate;
            EndDate = _caseItem.EndDate;
            Status = _caseItem.Status;
            Description = _caseItem.Description;

            // بارگذاری Attachments
            if (caseItem.CaseAttachments != null)
                CaseAttachments = new ObservableCollection<CaseAttachment>(caseItem.CaseAttachments);       
        }

        [ObservableProperty] private Case selectedCase;

        [RelayCommand]
        private async Task Edit()
        {
            // استفاده از نمونه موجود (نه نام نوع Case)
            var editPopup = new EditCasePopup(_caseItem, _caseService);

            // اگر خواستیم بعد از ویرایش UI این popup رو آپدیت کنیم به رویداد گوش میدیم:
            editPopup.CaseUpdated += updatedCase =>
            {
                // آپدیت پراپرتی‌ها در thread UI
                MainThread.BeginInvokeOnMainThread(() =>
                {                 
                    CourtName = updatedCase.CourtName;
                    JudgeName = updatedCase.JudgeName;                   
                    EndDate = updatedCase.EndDate;
                    Status = updatedCase.Status;
                    Description = updatedCase.Description;

                    CaseAttachments = new ObservableCollection<CaseAttachment>(updatedCase.CaseAttachments ?? new List<CaseAttachment>());
                });
            };

            await MopupService.Instance.PushAsync(editPopup);
        }

        [RelayCommand]
        private async Task Close()
        {
            await MopupService.Instance.PopAsync();
        }

        [RelayCommand]
        private async Task OpenAttachment(CaseAttachment attachment)
        {
            if (attachment == null || string.IsNullOrEmpty(attachment.FilePath))
                return;

            try
            {
#if ANDROID || IOS
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(attachment.FilePath)
                });
#elif WINDOWS
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(attachment.FilePath)
                    {
                        UseShellExecute = true
                    }
                };
                process.Start();
#else
                await Application.Current.MainPage.DisplayAlert("خطا", "باز کردن فایل روی این پلتفرم پشتیبانی نمی‌شود.", "باشه");
#endif
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("خطا", $"باز کردن فایل با مشکل مواجه شد:\n{ex.Message}", "باشه");
            }
        }

        [RelayCommand]
        private async Task DeleteAttachment(CaseAttachment attachment)
        {
            if (attachment == null) return;

            var result = await Application.Current.MainPage.DisplayAlert("حذف فایل",
                $"آیا می‌خواهید فایل {attachment.FileName} حذف شود؟", "بله", "خیر");

            if (!result) return;

            try
            {
                // استفاده از همان CaseService inject شده
                await _caseService.DeleteAttachment(attachment.Id);

                CaseAttachments.Remove(attachment);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("خطا", $"حذف فایل با مشکل مواجه شد:\n{ex.Message}", "باشه");
            }
        }
    }
}
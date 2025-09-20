using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using Vakilaw.Views.Popups;

namespace Vakilaw.ViewModels;

public partial class EditCasePopupViewModel : ObservableObject
{
    private readonly EditCasePopup _popup;
    private readonly CaseService _caseService;
    private readonly Case _case; // مرجع اصلی پرونده

    [ObservableProperty] private bool endDateIsEnabled = false;
    [ObservableProperty] private string courtName;
    [ObservableProperty] private string judgeName;
    [ObservableProperty] private string? endDate;
    [ObservableProperty] private string status;
    [ObservableProperty] private string description;
    [ObservableProperty] private ObservableCollection<CaseAttachment> attachments = new();

    public EditCasePopupViewModel(EditCasePopup popup, Case caseItem, CaseService caseService)
    {
        _popup = popup;
        _caseService = caseService;
        _case = caseItem;

        // مقداردهی اولیه
        CourtName = _case.CourtName;
        JudgeName = _case.JudgeName;
        EndDate = _case.EndDate;
        Status = _case.Status;
        Description = _case.Description;

        if (_case.CaseAttachments != null)
            Attachments = new ObservableCollection<CaseAttachment>(_case.CaseAttachments);
    }

    partial void OnStatusChanged(string value)
    {
        EndDateIsEnabled = value == "مختومه";
    }

    [RelayCommand]
    private async Task Save()
    {
        // بروزرسانی نمونه اصلی
        _case.CourtName = CourtName;
        _case.JudgeName = JudgeName;
        _case.EndDate = EndDate;
        _case.Status = Status;
        _case.Description = Description;
        _case.CaseAttachments = Attachments.ToList();

        // ذخیره در DB
        await _caseService.UpdateCase(_case);

        // اطلاع به بازکننده popup
        _popup.RaiseCaseUpdated(_case);

        // بستن popup
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private async Task AddAttachment()
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/pdf", "image/*" } },
                { DevicePlatform.WinUI, new[] { ".pdf", ".jpg", ".jpeg", ".png" } },
                { DevicePlatform.iOS, new[] { "com.adobe.pdf", "public.image" } },
                { DevicePlatform.MacCatalyst, new[] { "com.adobe.pdf", "public.image" } }
            });

            var result = await FilePicker.Default.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "انتخاب فایل‌های پیوست",
                FileTypes = customFileType
            });

            if (result != null)
            {
                foreach (var file in result)
                {
                    Attachments.Add(new CaseAttachment
                    {
                        FileName = Path.GetFileName(file.FullPath),
                        FilePath = file.FullPath,
                        FileType = Path.GetExtension(file.FullPath).ToLower()
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("خطا", $"انتخاب فایل با مشکل مواجه شد:\n{ex.Message}", "باشه");
        }
    }

    [RelayCommand]
    private void RemoveAttachment(CaseAttachment attachment)
    {
        if (attachment != null && Attachments.Contains(attachment))
            Attachments.Remove(attachment);
    }
}
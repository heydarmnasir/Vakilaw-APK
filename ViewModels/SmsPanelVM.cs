using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Vakilaw.ViewModels
{
    public partial class SmsPanelVM : ObservableObject
    {
        private readonly ClientService _clientService;
        private readonly SmsService _smsService;

        [ObservableProperty]
        private ObservableCollection<Client> clients = new();

        [ObservableProperty]
        private Client selectedClient;

        [ObservableProperty]
        private string singleMessage;

        [ObservableProperty]
        private string groupMessage;

        // ============================
        // گروهی
        // ============================
        [ObservableProperty]
        private ObservableCollection<SelectableClient> filteredClients = new();

        [ObservableProperty]
        private string groupSearchText;

        [ObservableProperty]
        private bool isAllSelected;

        partial void OnIsAllSelectedChanged(bool value)
        {
            if (FilteredClients == null) return;

            // فقط وقتی خود "انتخاب همه" تغییر کرد، همه آپدیت بشن
            foreach (var c in FilteredClients)
                c.IsSelected = value;
        }

        // ============================
        // تاریخچه
        // ============================
        [ObservableProperty]
        private ObservableCollection<SmsHistoryItem> smsHistory = new();

        // Popup ها
        [ObservableProperty] private bool isSingleDetailsVisible;
        [ObservableProperty] private bool isGroupDetailsVisible;
        [ObservableProperty] private string singleDetailsText;
        [ObservableProperty] private string groupDetailsText;

        public SmsPanelVM(ClientService clientService, SmsService smsService)
        {
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            LoadClients();
            LoadHistory();
        }

        private void LoadClients()
        {
            var list = _clientService.GetClients();
            Clients = new ObservableCollection<Client>(list);

            FilteredClients = new ObservableCollection<SelectableClient>(
                Clients.Select(c => new SelectableClient
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber
                })
            );

            foreach (var c in FilteredClients)
            {
                c.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SelectableClient.IsSelected))
                    {
                        // فقط وضعیت IsAllSelected رو آپدیت کن، نه چیز دیگه
                        if (FilteredClients.All(x => x.IsSelected))
                            isAllSelected = true;   // 👈 دقت کن: اینجا مستقیم فیلد رو ست می‌کنیم
                        else
                            isAllSelected = false;  // نه setter پراپرتی (IsAllSelected)
                    }
                };
            }
        }

        private void UpdateIsAllSelected()
        {
            if (FilteredClients == null || FilteredClients.Count == 0)
            {
                IsAllSelected = false;
                return;
            }

            // اگر همه انتخاب بودن → true
            // اگر حتی یکی انتخاب نشده بود → false
            IsAllSelected = FilteredClients.All(c => c.IsSelected);
        }

        private async void LoadHistory()
        {
            var list = await _smsService.GetHistoryAsync();
            SmsHistory = new ObservableCollection<SmsHistoryItem>(list);
        }

        // ============================
        // ارسال تکی
        // ============================
        [RelayCommand]
        private async Task SendSingleSms()
        {
            if (SelectedClient == null)
            {
                await Toast.Make("لطفاً یک موکل انتخاب کنید", ToastDuration.Short).Show();
                return;
            }

            if (string.IsNullOrWhiteSpace(SingleMessage))
            {
                await Toast.Make("متن پیام خالی است", ToastDuration.Short).Show();
                return;
            }

            try
            {
                await _smsService.SendSingleAsync(
                    SelectedClient.PhoneNumber,
                    SingleMessage,
                    SelectedClient.FullName // 📌 نام موکل برای ذخیره تاریخچه
                );

                // ری‌لود تاریخچه
                LoadHistory();

                SingleMessage = string.Empty;
            }
            catch (Exception ex)
            {
                await Toast.Make($"{ex.Message} خطا در ارسال پیامک", ToastDuration.Short).Show();              
            }
        }

        // ============================
        // ارسال گروهی
        // ============================
        [RelayCommand]
        private async Task SendGroupSms()
        {
            var selected = FilteredClients.Where(c => c.IsSelected).ToList();

            if (!selected.Any())
            {
                await Toast.Make("هیچ موکلی انتخاب نشده است", ToastDuration.Short).Show();            
                return;
            }

            // ✅ حداقل دو موکل باید انتخاب شوند
            if (selected.Count < 2)
            {
                await Toast.Make("حداقل دو موکل باید انتخاب شوند", ToastDuration.Short).Show();      
                return;
            }

            if (string.IsNullOrWhiteSpace(GroupMessage))
            {
                await Toast.Make("متن پیام خالی است", ToastDuration.Short).Show();              
                return;
            }

            var phoneNumbers = selected.Select(c => c.PhoneNumber).ToList();
            await _smsService.SendGroupAsync(phoneNumbers, GroupMessage);

            // ری‌لود تاریخچه
            LoadHistory();

            GroupMessage = string.Empty;
            foreach (var c in FilteredClients) c.IsSelected = false;
            IsAllSelected = false;
        }

        // ============================
        // سرچ گروهی
        // ============================
        partial void OnGroupSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                FilteredClients = new ObservableCollection<SelectableClient>(
                    Clients.Select(c => new SelectableClient
                    {
                        Id = c.Id,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber
                    })
                );
            }
            else
            {
                var query = Clients.Where(c =>
                    (!string.IsNullOrEmpty(c.FullName) && c.FullName.Contains(value)) ||
                    (!string.IsNullOrEmpty(c.PhoneNumber) && c.PhoneNumber.Contains(value))
                );

                FilteredClients = new ObservableCollection<SelectableClient>(
                    query.Select(c => new SelectableClient
                    {
                        Id = c.Id,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber
                    })
                );
            }
        }

        // ============================
        // نمایش جزئیات پیام
        // ============================
        [RelayCommand]
        private async Task ShowSmsDetails(int id)
        {
            var sms = SmsHistory.FirstOrDefault(x => x.Id == id);
            if (sms == null) return;

            if (sms.IsGroup)
            {
                GroupDetailsText = sms.Message;
                IsGroupDetailsVisible = true;
            }
            else
            {
                SingleDetailsText = sms.Message;
                IsSingleDetailsVisible = true;
            }
        }

        [RelayCommand] private void CloseSingleDetails() => IsSingleDetailsVisible = false;
        [RelayCommand] private void CloseGroupDetails() => IsGroupDetailsVisible = false;
    }
}
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using System;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using Vakilaw.Views;

namespace Vakilaw.ViewModels
{
    public partial class ClientWithCasesViewModel : ObservableObject
    {
        private readonly CaseService _caseService;
        private readonly ClientsAndCasesViewModel _parent;

        public Client Client { get; }

        public ObservableCollection<Case> Cases { get; } = new();

        public ClientWithCasesViewModel(Client client, CaseService caseService, ClientsAndCasesViewModel parent)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            _caseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            // load cases for this client
            var list = _caseService.GetCasesByClient(Client.Id);
            foreach (var c in list) Cases.Add(c);
        }

        [ObservableProperty] private bool isExpanded;

        // وقتی Expander باز میشه، این فرمان اجرا میشه — parent.SelectedClient رو ست کن
        [RelayCommand]
        private void ToggleExpand()
        {
            IsExpanded = !IsExpanded;

            if (IsExpanded)
            {
                // تنظیم موکل انتخاب‌شده در ViewModel والد
                _parent.SelectedClient = Client;

                // بارگذاری پرونده‌ها (در صورت لزوم دوباره)
                RefreshCases();
            }
            else
            {
                // اگر خواستی، parent.SelectedClient = null; اما ما نگه می‌داریم تا دکمه افزودن پرونده فعال بمونه
            }
        }

        private void RefreshCases()
        {
            Cases.Clear();
            var list = _caseService.GetCasesByClient(Client.Id);
            foreach (var c in list) Cases.Add(c);
        }

        // نمایش Popup جزئیات پرونده (مستقیماً Mopups را صدا می‌زند)
        [RelayCommand]
        private async Task ShowClientDetailsAsync(Client client)
        {
            if (client == null) return;

            var popup = new ClientDetailsPopup(client);
            await MopupService.Instance.PushAsync(popup);
        }

        // نمایش Popup جزئیات پرونده (مستقیماً Mopups را صدا می‌زند)
        [RelayCommand]
        private async Task ShowCaseDetailsAsync(Case caseItem)
        {
            if (caseItem == null) return;

            var popup = new CaseDetailsPopup(caseItem);
            await MopupService.Instance.PushAsync(popup);
        }

        // وقتی به صورت خارجی پرونده‌ای اضافه شد، این متد را فراخوانی کن تا لیست به‌روز شود
        public void AddCaseToList(Case newCase)
        {
            if (newCase == null) return;
            if (newCase.ClientId != Client.Id) return;
            Cases.Add(newCase);
        }

        // حذف از لیست (در صورت حذف)
        public void RemoveCaseFromList(int caseId)
        {
            var item = Cases.FirstOrDefault(x => x.Id == caseId);
            if (item != null) Cases.Remove(item);
        }
    }
}
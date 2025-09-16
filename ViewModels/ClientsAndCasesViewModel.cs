using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using Vakilaw.Views;
using System.Timers;

namespace Vakilaw.ViewModels
{
    public partial class ClientsAndCasesViewModel : ObservableObject
    {
        [ObservableProperty] private bool isClientsVisible = true;
        [ObservableProperty] private bool isCasesVisible = false;

        [ObservableProperty]
        private string countsLabel;

        [RelayCommand]
        private async Task ShowClients()
        {
            IsClientsVisible = true;
            IsCasesVisible = false;

            // بروزرسانی متن Label
            CountsLabel = $"تعداد موکل‌ها: {await _clientService.GetClientsCount()}";
        }

        [RelayCommand]
        private async Task ShowCases()
        {
            IsClientsVisible = false;
            IsCasesVisible = true;

            // بروزرسانی متن Label
            CountsLabel = $"تعداد پرونده‌ها: {await _caseService.GetCasesCount()}";
        }

        private async Task LoadCountsAsync()
        {
            var count = await _clientService.GetClientsCount();
            CountsLabel = $"تعداد موکل‌ها: {count}";
        }

        private readonly ClientService _clientService;
        private readonly CaseService _caseService;

        private System.Timers.Timer _debounceTimerClients;
        private System.Timers.Timer _debounceTimerCases;

        public ClientsAndCasesViewModel(ClientService clientService, CaseService caseService)
        {
            _clientService = clientService;
            _caseService = caseService;

            // بارگذاری async شمارش‌ها
            _ = LoadCountsAsync();

            // بارگذاری اولیه
            SearchClients();
            //SearchCases();
            LoadClientsWithCases();
        }

        [ObservableProperty]
        private ObservableCollection<ClientWithCasesViewModel> clientsWithCases = new();

        public void LoadClientsWithCases()
        {
            ClientsWithCases.Clear();
            var clientsList = _clientService.GetClients();
            foreach (var c in clientsList)
            {
                var wrapper = new ClientWithCasesViewModel(c, _caseService, this);
                ClientsWithCases.Add(wrapper);
            }
        }

        [ObservableProperty] private string clientSearchText;
        [ObservableProperty] private string caseSearchText;

        //public int ClientsCount { get; set; }
        //public int CasesCount { get; set; }

        //public async Task LoadCounts()
        //{
        //    ClientsCount = await _clientService.GetClientsCount();
        //    CasesCount = await _caseService.GetCasesCount();
        //    OnPropertyChanged(nameof(ClientsCount));
        //    OnPropertyChanged(nameof(CasesCount));
        //}

        #region Client      

        [ObservableProperty]
        private ObservableCollection<Client> clients = new();

        [ObservableProperty]
        private Client selectedClient;

        [ObservableProperty] private string fullName;
        [ObservableProperty] private string nationalCode;
        [ObservableProperty] private string phoneNumber;
        [ObservableProperty] private string address;
        [ObservableProperty] private string clientDescription;

        [RelayCommand]
        private void SearchClients()
        {
            ClientsWithCases.Clear();

            var filteredClients = _clientService.SearchClients(ClientSearchText);

            foreach (var client in filteredClients)
            {
                var wrapper = new ClientWithCasesViewModel(client, _caseService, this);
                ClientsWithCases.Add(wrapper);
            }
        }

        partial void OnClientSearchTextChanged(string value)
        {
            _debounceTimerClients?.Stop();
            _debounceTimerClients = new System.Timers.Timer(400) { AutoReset = false };
            _debounceTimerClients.Elapsed += (s, e) =>
            {
                SearchClients();
            };
            _debounceTimerClients.Start();
        }

        [RelayCommand]
        private void AddClient(Client newClient)
        {
            if (newClient == null) return;

            // فقط wrapper را اضافه کن و به ObservableCollection نمایش بده
            var wrapper = new ClientWithCasesViewModel(newClient, _caseService, this);
            ClientsWithCases.Add(wrapper);

            // نیازی به اضافه کردن دوباره به دیتابیس نیست
            // نیازی به SearchClients یا LoadClientsWithCases هم نیست

            // پاک کردن فیلدهای ورودی صفحه اصلی (در صورت استفاده)
            FullName = NationalCode = PhoneNumber = Address = ClientDescription = string.Empty;
        }


        [RelayCommand]
        private async Task UpdateClient()
        {
            if (SelectedClient == null) return;

            await _clientService.UpdateClient(SelectedClient);
            SearchClients();
            LoadClientsWithCases();
        }

        [RelayCommand]
        private async Task DeleteClient()
        {
            if (SelectedClient == null) return;

            await _clientService.DeleteClient(SelectedClient.Id);
            SearchClients();
            LoadClientsWithCases();
        }

        [RelayCommand]
        private async Task ShowAddClientPopup()
        {
            // پاس دادن ClientService به Popup
            var popup = new AddClientPopup(_clientService);

            popup.ClientCreated += newClient =>
            {
                // اضافه کردن مستقیم به لیست و Wrapper
                var wrapper = new ClientWithCasesViewModel(newClient, _caseService, this);
                ClientsWithCases.Add(wrapper);

                // به‌روزرسانی لیست جستجو و CollectionView
                SearchClients();
            };

            await MopupService.Instance.PushAsync(popup);
        }

        [RelayCommand]
        private async Task ShowClientDetailsAsync(Client client)
        {
            if (client == null) return;

            var popup = new ClientDetailsPopup(client);
            await MopupService.Instance.PushAsync(popup);
        }
        #endregion

        #region Case
        [ObservableProperty]
        private ObservableCollection<Case> cases = new();

        [ObservableProperty]
        private Case selectedCase;

        [ObservableProperty]
        private int clientId;

        [ObservableProperty] private string title;
        [ObservableProperty] private string caseNumber;
        [ObservableProperty] private string courtName;
        [ObservableProperty] private string judgeName;
        [ObservableProperty] private string startDate;
        [ObservableProperty] private string? endDate;
        [ObservableProperty] private string status;
        [ObservableProperty] private string caseDescription;

        partial void OnCaseSearchTextChanged(string value)
        {
            _debounceTimerCases?.Stop();
            _debounceTimerCases = new System.Timers.Timer(400) { AutoReset = false };
            _debounceTimerCases.Elapsed += (s, e) =>
            {
                SearchClients();
            };
            _debounceTimerCases.Start();
        }

        [RelayCommand]
        private void AddCase()
        {
            if (SelectedClient == null) return;

            var caseItem = new Case
            {
                Title = Title,
                CaseNumber = CaseNumber,
                CourtName = CourtName,
                JudgeName = JudgeName,
                StartDate = StartDate,
                EndDate = EndDate,
                Status = CaseDescription,
                Description = CaseDescription,
                ClientId = SelectedClient.Id,
                Client = SelectedClient
            };

            _caseService.AddCase(caseItem);
            SearchClients();

            // ریست کردن فرم
            Title = CaseNumber = CourtName = JudgeName = Status = CaseDescription = string.Empty;         
            EndDate = null;

            // اضافه کردن به ClientsWithCases
            var wrapper = ClientsWithCases.FirstOrDefault(w => w.Client.Id == caseItem.ClientId);
            wrapper?.AddCaseToList(caseItem);
        }

        [RelayCommand]
        private void UpdateCase()
        {
            if (SelectedCase == null) return;

            _caseService.UpdateCase(SelectedCase);
            SearchClients();
        }

        [RelayCommand]
        private void DeleteCase()
        {
            if (SelectedCase == null) return;

            _caseService.DeleteCase(SelectedCase.Id);
            SearchClients();
        }

        [RelayCommand]
        private async Task ShowAddCasePopup()
        {
            if (SelectedClient == null) return;

            // پاس دادن سرویس به Popup در صورت نیاز (یا فقط Client)
            var popup = new AddCasePopup(SelectedClient);

            popup.CaseCreated += newCase =>
            {
                // 1️⃣ اضافه کردن به دیتابیس
                _caseService.AddCase(newCase);

                // 2️⃣ به‌روزرسانی CollectionView
                SearchClients();

                // 3️⃣ اضافه کردن به Wrapper مرتبط با Client
                var wrapper = ClientsWithCases.FirstOrDefault(w => w.Client.Id == newCase.ClientId);
                wrapper?.AddCaseToList(newCase);
            };

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
        #endregion
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using Vakilaw.Views;

namespace Vakilaw.ViewModels
{
    public partial class ClientWithCasesViewModel : ObservableObject
    {
        private readonly CaseService _caseService;
        private readonly ClientService _clientService;
        private readonly ClientsAndCasesViewModel _parent;

        public Client Client { get; }

        public ObservableCollection<Case> Cases { get; } = new();

        public ClientWithCasesViewModel(Client client, ClientService clientService, CaseService caseService, ClientsAndCasesViewModel parent)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            _caseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            _ = RefreshCasesAsync();
        }

        [ObservableProperty] private bool isExpanded;

        [RelayCommand]
        private void ToggleExpand()
        {
            IsExpanded = !IsExpanded;

            if (IsExpanded)
            {
                _parent.SelectedClient = Client;
                _ = RefreshCasesAsync();
            }
        }

        public async Task RefreshCasesAsync()
        {
            Cases.Clear();
            var list = _caseService.GetCasesByClient(Client.Id);
            foreach (var c in list)
                Cases.Add(c);
        }

        [RelayCommand]
        private async Task ShowClientDetailsAsync(Client client)
        {
            if (client == null) return;

            var popup = new ClientDetailsPopup(client, _clientService);
            await MopupService.Instance.PushAsync(popup);
        }

        [RelayCommand]
        private async Task ShowCaseDetailsAsync(Case caseItem)
        {
            if (caseItem == null) return;
            var popup = new CaseDetailsPopup(caseItem, _caseService);
            await MopupService.Instance.PushAsync(popup);
        }

        public async Task AddCaseToListAsync(Case newCase)
        {
            if (newCase == null || newCase.ClientId != Client.Id) return;

            // ✅ اضافه کردن فقط اگر موجود نباشد
            if (!Cases.Any(c => c.Id == newCase.Id))
                Cases.Add(newCase);
        }

        public void RemoveCaseFromList(int caseId)
        {
            var item = Cases.FirstOrDefault(x => x.Id == caseId);
            if (item != null)
                Cases.Remove(item);
        }
    }
}
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vakilaw.Models;
using Vakilaw.Services;
using Vakilaw.Views;
using static Vakilaw.ViewModels.LawBankVM;
using static Vakilaw.Views.LawyerSubmitPopup;

namespace Vakilaw.ViewModels;

public partial class MainPageVM : ObservableObject
{
    private readonly UserService _userService;
    private readonly LawService _lawService;
    private readonly LawyerService _lawyerService;

    [ObservableProperty] private ObservableCollection<LawItem> bookmarkedLaws;
    [ObservableProperty] private ObservableCollection<Lawyer> lawyers;
    [ObservableProperty] private ObservableCollection<Lawyer> allLawyers;
    [ObservableProperty] private ObservableCollection<string> cities;

    [ObservableProperty] private bool isLawyer;
    [ObservableProperty] private bool canRegisterLawyer;
    [ObservableProperty] private bool showRegisterLabel;
    [ObservableProperty] private bool lawyerLicenseVisibility;
    [ObservableProperty] private string lawyerFullName;
    [ObservableProperty] private string lawyerLicense;

    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageSize = 7;
    [ObservableProperty] private bool hasMorePages = true;
    [ObservableProperty] private bool isBusy;

    [ObservableProperty] private string selectedCity;
    [ObservableProperty] private string searchQuery;

    [ObservableProperty] private bool isLawyersListVisible;
    [ObservableProperty] private bool isBookmarkVisible;
    [ObservableProperty] private bool isSettingsVisible;

    private CancellationTokenSource _searchCts;


    // مقدار پیش‌فرض: Home
    [ObservableProperty]
    private string selectedTab = "Home";

    // کامند انتخاب تب
    [RelayCommand]
    private async Task SelectTab(string tabName)
    {
        SelectedTab = tabName;

        switch (tabName)
        {
            case "Home":
                await ToggleHomeAsync();
                break;

            case "AdlIran":
                await OpenAdlIranSiteAsync();
                break;

            case "Bookmarks":
                await ToggleBookmarkPanelAsync();
                break;

            case "Settings":
                await ToggleSettingsPanelAsync();
                break;

            default:
                // برای تب‌های دیگه فقط تغییر SelectedTab کفایت می‌کنه
                break;
        }
    }

    public MainPageVM(UserService userService, LawService lawService, LawyerService lawyerService)
    {
        _userService = userService;
        _lawService = lawService;
        _lawyerService = lawyerService;

        BookmarkedLaws = new ObservableCollection<LawItem>();
        Lawyers = new ObservableCollection<Lawyer>();
        AllLawyers = new ObservableCollection<Lawyer>();
        Cities = new ObservableCollection<string>();

        Task.Run(async () => await InitializeLawyersAsync());
        Task.Run(async () => await LoadBookmarksAsync());

        WeakReferenceMessenger.Default.Register<BookmarkChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (m.Law.IsBookmarked)
                {
                    if (!BookmarkedLaws.Any(x => x.Id == m.Law.Id))
                    {
                        // اضافه کردن نسخه مستقل از LawItem
                        BookmarkedLaws.Add(new LawItem
                        {
                            Id = m.Law.Id,
                            ArticleNumber = m.Law.ArticleNumber,
                            LawType = m.Law.LawType,
                            Title = m.Law.Title,
                            Text = m.Law.Text,
                            Notes = m.Law.Notes.ToList(),
                            IsBookmarked = true,
                            IsExpanded = false
                        });
                    }
                }
                else
                {
                    var item = BookmarkedLaws.FirstOrDefault(x => x.Id == m.Law.Id);
                    if (item != null)
                        BookmarkedLaws.Remove(item);
                }
            });
        });

        LoadUserState();

        WeakReferenceMessenger.Default.Register<LawyerRegisteredMessage>(this, async (r, m) =>
        {
            IsLawyer = true;
            CanRegisterLawyer = false;
            ShowRegisterLabel = false;
            LawyerLicenseVisibility = true;
            LawyerFullName = m.Value;
            LawyerLicense = m.LicenseNumber;

            CurrentPage = 1;
            Lawyers.Clear();
            await LoadNotesAsync();
        });

        Task.Run(async () => await LoadNotesAsync());
    }

    #region Initialize Lawyers
    private async Task InitializeLawyersAsync()
    {
        string jsonPath = Path.Combine(FileSystem.AppDataDirectory, "Lawyers.json");

        if (!File.Exists(jsonPath))
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("Lawyers.json");
            using var fileStream = File.Create(jsonPath);
            await stream.CopyToAsync(fileStream);
        }

        await _lawyerService.SeedDataFromJsonAsync(jsonPath);

        var allLawyersList = await _lawyerService.GetAllLawyersAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            AllLawyers.Clear();
            foreach (var l in allLawyersList)
                AllLawyers.Add(l);

            var cityList = allLawyersList
                .Select(l => l.City)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            cityList.Insert(0, "همه");

            Cities.Clear();
            foreach (var c in cityList)
                Cities.Add(c);
        });

        await LoadNotesAsync();
    }
    #endregion

    #region Load Bookmarks
    private async Task LoadBookmarksAsync()
    {
        try
        {
            var items = await _lawService.GetBookmarkedLawsAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BookmarkedLaws.Clear();
                foreach (var law in items)
                {
                    BookmarkedLaws.Add(new LawItem
                    {
                        Id = law.Id,
                        ArticleNumber = law.ArticleNumber,
                        LawType = law.LawType,
                        Title = law.Title,
                        Text = law.Text,
                        Notes = law.Notes.ToList(),
                        IsBookmarked = law.IsBookmarked,
                        IsExpanded = false
                    });
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadBookmarksAsync Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task OpenArticleAsync(LawItem law)
    {
        if (law == null) return;
        await App.Current.MainPage.DisplayAlert($"تبصره: {law.Title}", law.NotesText, "برگشت", FlowDirection.RightToLeft);
    }

    [RelayCommand]
    private void ToggleBookmark(LawItem law)
    {
        if (law == null) return;

        // تغییر وضعیت
        law.IsBookmarked = !law.IsBookmarked;

        // حذف از پنل بوکمارک‌ها در صورت آنبوکمارک
        if (!law.IsBookmarked)
        {
            var item = BookmarkedLaws.FirstOrDefault(x => x.Id == law.Id);
            if (item != null)
                BookmarkedLaws.Remove(item);
        }

        // پیام به سایر ViewModel ها که این آیتم تغییر کرده
        WeakReferenceMessenger.Default.Send(new BookmarkChangedMessage(law));
    }
    #endregion

    #region Notes & Lazy Loading
    [RelayCommand]
    public async Task LoadNotesAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        await Task.Yield();       
        try
        {
            IEnumerable<Lawyer> filtered = AllLawyers;

            if (!string.IsNullOrWhiteSpace(SelectedCity) && SelectedCity != "همه")
                filtered = filtered.Where(l => string.Equals(l.City, SelectedCity, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(SearchQuery))
                filtered = filtered.Where(l =>
                    (!string.IsNullOrEmpty(l.FullName) && l.FullName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.PhoneNumber) && l.PhoneNumber.Contains(SearchQuery))
                );

            var filteredList = filtered.ToList();

            var pagedLawyers = filteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            if (CurrentPage == 1) Lawyers.Clear();

            foreach (var lawyer in pagedLawyers)
            {
                if (!Lawyers.Any(x => x.Id == lawyer.Id))
                    Lawyers.Add(lawyer);
            }

            HasMorePages = (CurrentPage * PageSize) < filteredList.Count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadNotesAsync Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedCityChanged(string value)
    {
        CurrentPage = 1;
        Lawyers.Clear();
        LoadNotesAsync().SafeFireAndForget();
    }

    partial void OnSearchQueryChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var ct = _searchCts.Token;

        Task.Run(async () =>
        {
            await Task.Delay(200, ct);
            if (!ct.IsCancellationRequested)
            {
                CurrentPage = 1;
                Lawyers.Clear();
                await LoadNotesAsync();
            }
        }, ct);
    }

    [RelayCommand]
    public async Task LoadNextPageAsync()
    {
        if (!HasMorePages || IsBusy) return;
        CurrentPage++;
        await LoadNotesAsync();
    }
    #endregion

    #region Toggle Panels
    private async Task CloseAllPanelsAsync()
    {
        if (IsLawyersListVisible)
        {
            await SlideOutPanel(LawyersListPanelRef);
            IsLawyersListVisible = false;
        }
        if (IsBookmarkVisible)
        {
            await SlideOutPanel(BookmarkPanelRef);
            IsBookmarkVisible = false;
        }
        if (IsSettingsVisible)
        {
            await SlideOutPanel(SettingsPanelRef);
            IsSettingsVisible = false;
        }
        // هر پنل دیگری هم اضافه شود
    }

    [RelayCommand]
    public async Task ToggleHomeAsync()
    {
        await CloseAllPanelsAsync();
    }

    [RelayCommand]
    public async Task ToggleLawyersListAsync()
    {
        if (IsLawyersListVisible)
            await SlideOutPanel(LawyersListPanelRef);
        else
            await SlideInPanel(LawyersListPanelRef);



        IsLawyersListVisible = !IsLawyersListVisible;
    }

    [RelayCommand]
    public async Task ToggleBookmarkPanelAsync()
    {
        if (IsBookmarkVisible)
            await SlideOutPanel(BookmarkPanelRef);
        else
            await SlideInPanel(BookmarkPanelRef);
        if (IsSettingsVisible)
            await SlideOutPanel(SettingsPanelRef);

        IsBookmarkVisible = !IsBookmarkVisible;
    }

    [RelayCommand]
    public async Task ToggleSettingsPanelAsync()
    {
        if (IsSettingsVisible)
            await SlideOutPanel(SettingsPanelRef);
        else
            await SlideInPanel(SettingsPanelRef);
        if (IsBookmarkVisible)
            await SlideOutPanel(BookmarkPanelRef);

        IsSettingsVisible = !IsSettingsVisible;
    }
    #endregion

    #region Animation Helpers
    // مقادیر Ref باید در Code-behind تنظیم شود (با DI یا BindingContext)
    public Grid LawyersListPanelRef { get; set; }
    public Grid BookmarkPanelRef { get; set; }
    public Grid SettingsPanelRef { get; set; }

    private async Task SlideInPanel(VisualElement panel)
    {
        if (panel == null) return;

        // مطمئن می‌شیم که پنل قابل مشاهده باشه
        panel.IsVisible = true;

        // مقدار اولیه TranslationX رو خارج از صفحه می‌بریم
        var width = Application.Current.MainPage?.Width > 0
                    ? Application.Current.MainPage.Width
                    : panel.Width;

        panel.TranslationX = width;

        // حالا انیمیشن ورود
        await panel.TranslateTo(0, 0, 400, Easing.CubicOut);
    }

    private async Task SlideOutPanel(VisualElement panel)
    {
        if (panel == null) return;

        var width = Application.Current.MainPage?.Width > 0
                    ? Application.Current.MainPage.Width
                    : panel.Width;

        // انیمیشن خروج
        await panel.TranslateTo(width, 0, 400, Easing.CubicIn);

        // بعد از خروج، نامرئی کنیم
        panel.IsVisible = false;
    }

    #endregion


    #region Navigation & Popups
    [RelayCommand]
    public async Task ShowDetailsAsync(Lawyer lawyer)
    {
        if (lawyer == null) return;
        var popup = new LawyerDetailsPopup(lawyer.FullName, lawyer.PhoneNumber, lawyer.Address);
        await MopupService.Instance.PushAsync(popup);
    }

    [RelayCommand]
    public async Task OpenLawyerPopupAsync()
    {
        var popup = new LawyerSubmitPopup(_userService);
        await MopupService.Instance.PushAsync(popup);
    }

    [RelayCommand]
    public async Task LawBankPageAsync()
    {
        await Shell.Current.GoToAsync("LawBankPage");
    }

    [RelayCommand]
    public async Task OpenAdlIranSiteAsync()
    {
        await Launcher.OpenAsync("https://adliran.ir/");
    }

    [RelayCommand]
    private async Task HamiVakilAsync()
    {
        await Launcher.OpenAsync("https://search-hamivakil.ir/");
    }
    #endregion

    #region Helpers
    private void LoadUserState()
    {
        var role = Preferences.Get("UserRole", "Unknown");
        var isRegistered = Preferences.Get("IsLawyerRegistered", false);

        if (role == "Unknown")
        {
            IsLawyer = false;
            CanRegisterLawyer = true;
            ShowRegisterLabel = true;
            LawyerLicenseVisibility = false;
        }
        else
        {
            IsLawyer = role == "Lawyer" && isRegistered;
            CanRegisterLawyer = !isRegistered;
            ShowRegisterLabel = !isRegistered;
            LawyerLicenseVisibility = isRegistered;
            LawyerFullName = Preferences.Get("LawyerFullName", string.Empty);
            LawyerLicense = Preferences.Get("LawyerLicense", string.Empty);
        }
    }
    #endregion
}
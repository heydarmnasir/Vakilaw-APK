using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Vakilaw.Models;
using Vakilaw.Models.Messages;
using Vakilaw.Services;

namespace Vakilaw.ViewModels;

public partial class LawBankVM : ObservableObject
{
    private readonly LawImporter _importer;
    private readonly LawService _database;
    private readonly Dictionary<string, string> _fileMap;

    [ObservableProperty] private bool isLoading;

    // همه مواد بدون فیلتر
    private List<LawItem> _allLaws = new();

    [ObservableProperty]
    private ObservableCollection<LawItem> laws = new();

    public ObservableCollection<string> LawTypes { get; }

    [ObservableProperty] private string selectedLawType;

    [ObservableProperty] private string searchText = string.Empty;

    private CancellationTokenSource? _searchCts;

    public LawBankVM(LawImporter importer, LawService database)
    {
        _importer = importer;
        _database = database;

        // نقشه فایل‌ها
        _fileMap = new Dictionary<string, string>
        {
            ["قانون اساسی"] = "Asasi_Law.json",
            ["قانون مدنی"] = "Madani_Law.json",
            ["قانون تجارت"] = "Tejarat_Law.json",
            ["آیین دادرسی مدنی"] = "AenDadrasiDadgahomomi&enghlab(Madani)_Law.json",
            ["قانون اجرای احکام مدنی"] = "EjrayeAhkamMadani_Law.json",
            ["آیین دادرسی کیفری"] = "AenDadrasiKeyfari_Law.json",
            ["قانون اصلاح آیین دادرسی کیفری"] = "EslahAenDadrasiKeyfari_Law.json",
            ["قانون کار"] = "Work_Law.json",
            ["اصلاح قانون نظام صنفی"] = "EslahGhanoonNezamSenfi_Law.json",
            ["قانون اصلاح مبارزه با پولشویی"] = "EslahMobarezeBaPoolShoe_Law.json",
            ["قانون اصلاح صدور چک"] = "EslahSodorCheck_Law.json",
            ["قانون حمایت از خانواده"] = "HemayatFamily_Law.json",
            ["قانون جرم سیاسی"] = "JormSeyasi_Law.json",
            ["قانون بیمه"] = "Bime_Law.json",
            ["قانون اموال غیر منقول اتباع خارجه"] = "AmvalGheyrManghoolAtbaKhareje_Law.json",
            ["قانون مجازات اسلامی 1375"] = "MojazatEslami1375_Law.json",
            ["قانون مجازات اسلامی 1392"] = "MojazatEslami1392_Law.json",
            ["قانون چک تضمین شده"] = "CheckTazminShode_Law.json",
            ["قانون دفتر اسناد"] = "DaftarAsnad_Law.json",
            ["قانون نظارت بر رفتار قضات"] = "NezaratBarRaftarGhozat_Law.json",
            ["قانون امور گمرکی"] = "OmorGomroki_Law.json",
            ["قانون پیش فروش ساختمان"] = "PreSealSakhteman_Law.json",
            ["قانون ثبت احوال"] = "SabtAhval_Law.json",
            ["قانون ثبت شرکت"] = "SabtSherkat_Law.json",
            ["قانون صادرات و واردات"] = "Saderat&Varedat_Law.json",
            ["قانون دریایی"] = "Sea_Law.json",
            ["قانون صید و شکار"] = "Seyd&Shekar_Law.json",
            ["قانون شوراهای حل اختلاف"] = "ShorahayeHalEkhtelaf_Law.json",
            ["قانون صدور چک"] = "SodorCheck_Law.json",
            ["قانون الزام به ثبت رسمی معاملات اموال غیر منقول"] = "ElzamBeSabtRasmiMoamelatAmvalGheyrManghool_Law.json",
            ["قانون انتقال قضات"] = "TransferJudge_Law.json",
            ["قانون وکالت"] = "Vekalat_Law.json",
            ["اصلاح قانون مجازات قاچاق اسلحه"] = "EslahGhanonMojazatghachaghAslahe_Law.json",
            ["قانون کاهش محازات حبس تعزیری"] = "KaheshMojazatHabsTaaziri_Law.json",
            ["قانون مأجر و مستأجر"] = "Moajer&Mostaajer_Law.json",
            ["قانون مبارزه با قاچاق کالا و ارز"] = "MobarezeBaGhachaghKala&Arz_Law.json",
            ["قانون مجازات اسید پاشی"] = "MojazatAsidPashi_Law.json",
            ["قانون مجازات انتقال مال غیر"] = "MojazatEnteghalMaalGheyr_Law.json",
            ["قانون مجازات استفاده غیر مجاز از آب و برق"] = "MojazatEstefadeGheyrmojazAAbBargh_Law.json",
            ["قانون تملک آپارتمان"] = "TamalokAparteman_Law.json",
            ["قانون تصدیق انحصار وراثت"] = "TasdighEnhesarVerasat_Law.json",
            ["قانون ورود و اقامت اتباع خارجه"] = "Vorod&EghamatAtbaKhareje_Law.json"
        };

        LawTypes = new ObservableCollection<string>(_fileMap.Keys);
        SelectedLawType = "قانون اساسی"; // پیش‌فرض

        // ثبت پیام
        WeakReferenceMessenger.Default.Register<BookmarkChangedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var lawItem = Laws.FirstOrDefault(x => x.Id == m.LawId);
                if (lawItem != null)
                {
                    lawItem.IsBookmarked = m.IsBookmarked;
                }
            });
        });

    }

    partial void OnSelectedLawTypeChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
            _ = LoadSelectedLawAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        // وقتی متن سرچ تغییر کرد، سرچ را با debounce اجرا کن
        SearchAsync().SafeFireAndForget();
    }

    [RelayCommand]
    private async Task LoadSelectedLawAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedLawType))
            return;

        IsLoading = true;
        Laws.Clear();
        _allLaws.Clear();

        try
        {
            if (!_fileMap.TryGetValue(SelectedLawType, out var fileName))
            {
                Debug.WriteLine($"[VM] No file mapping for '{SelectedLawType}'");
                return;
            }

            Debug.WriteLine($"[VM] Loading file '{fileName}' for type '{SelectedLawType}'");

            // فقط اگه دیتابیس خالی باشه ایمپورت می‌کنیم
            await foreach (var _ in _importer.ImportIfEmptyWithProgressAsync(fileName, SelectedLawType))
            {
                // اینجا چیزی اضافه نکن
            }

            // بعد از ایمپورت (یا اگر قبلاً ایمپورت شده) از DB بخون
            var existing = await _database.GetLawsByTypeAsync(SelectedLawType);
            _allLaws = existing.ToList();

            await ApplyFilterAsync(string.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM] LoadSelectedLawAsync exception: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var ct = _searchCts.Token;

        try
        {
            var query = NormalizeNumbers(SearchText?.Trim() ?? string.Empty);

            if (string.IsNullOrEmpty(query))
            {
                await ApplyFilterAsync(string.Empty);
                return;
            }

            await Task.Delay(150, ct); // debounce

            var filtered = _allLaws.Where(l =>
            {
                var title = NormalizeNumbers(l.Title ?? string.Empty);
                var text = NormalizeNumbers(l.Text ?? string.Empty);
                var article = NormalizeNumbers(l.ArticleNumber.ToString());
                return title.Contains(query, StringComparison.OrdinalIgnoreCase)
                       || text.Contains(query, StringComparison.OrdinalIgnoreCase)
                       || article.Contains(query);
            }).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Laws.Clear();
                foreach (var law in filtered)
                    Laws.Add(law);
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VM] SearchAsync exception: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ApplyFilterAsync(string query)
    {
        if (_allLaws == null || _allLaws.Count == 0) return;

        Laws.Clear();

        query = NormalizeNumbers(query?.Trim() ?? string.Empty);

        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allLaws
            : _allLaws.Where(l =>
            {
                var title = NormalizeNumbers(l.Title ?? string.Empty);
                var text = NormalizeNumbers(l.Text ?? string.Empty);
                var article = NormalizeNumbers(l.ArticleNumber.ToString());
                return title.Contains(query, StringComparison.OrdinalIgnoreCase)
                       || text.Contains(query, StringComparison.OrdinalIgnoreCase)
                       || article.Contains(query);
            });

        foreach (var law in filtered)
            Laws.Add(law);
    }

    [RelayCommand]
    private async Task ToggleBookmarkAsync(LawItem law)
    {
        if (law == null) return;
        law.IsBookmarked = !law.IsBookmarked;
        await _database.UpdateLawAsync(law);

        WeakReferenceMessenger.Default.Send(new BookmarkChangedMessage(law.Id, law.IsBookmarked));
    }

    [RelayCommand]
    private async Task OpenArticleAsync(LawItem law)
    {
        if (law == null) return;
        await App.Current.MainPage.DisplayAlert($"تبصره: {law.Title}", law.NotesText, "برگشت", FlowDirection.RightToLeft);
    }

    private string NormalizeNumbers(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        input = input
            .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2')
            .Replace('۳', '3').Replace('۴', '4').Replace('۵', '5')
            .Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9')
            .Replace('٠', '0').Replace('١', '1').Replace('٢', '2')
            .Replace('٣', '3').Replace('٤', '4').Replace('٥', '5')
            .Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9');

        return input;
    }  
}
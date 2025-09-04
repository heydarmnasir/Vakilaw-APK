using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vakilaw.Models;
using Vakilaw.Services;
using Microsoft.Maui.Dispatching;

namespace Vakilaw.ViewModels;

public partial class LawBankVM : ObservableObject
{
    private readonly LawImporter _importer;
    private readonly LawDatabase _database;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private ObservableCollection<LawItem> laws = new();

    public ObservableCollection<string> LawTypes { get; }

    [ObservableProperty] private string selectedLawType;
    [ObservableProperty] private string searchText;

    public LawBankVM(LawImporter importer, LawDatabase database)
    {
        _importer = importer;
        _database = database;

        LawTypes = new ObservableCollection<string>
        {
            "همه",
            "قانون مدنی",
            "قانون تجارت",
            "قانون دریایی"
        };
        SelectedLawType = "همه";

        // بارگذاری تدریجی قوانین بعد از آماده شدن UI
        MainThread.BeginInvokeOnMainThread(async () => await LoadDataAsync());
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            await foreach (var law in _importer.ImportIfEmptyWithProgressAsync())
            {
                MainThread.BeginInvokeOnMainThread(() => Laws.Add(law));
            }

            if (Laws.Count == 0)
            {
                var existing = await _importer.GetAllAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var item in existing)
                        Laws.Add(item);
                });
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleBookmarkAsync(LawItem law)
    {
        if (law == null) return;
        law.IsBookmarked = !law.IsBookmarked;
        await _database.UpdateLawAsync(law);
    }

    [RelayCommand]
    private async Task OpenArticleAsync(LawItem law)
    {
        if (law == null) return;
        await App.Current.MainPage.DisplayAlert($"ماده {law.ArticleNumber}", law.Content, "باشه");
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        SelectedLawType = "همه";
        await LoadDataAsync();
    }
}
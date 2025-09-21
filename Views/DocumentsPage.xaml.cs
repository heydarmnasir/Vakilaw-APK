using Vakilaw.Services;
using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class DocumentsPage : ContentPage
{
    public DocumentsPage(IPrinterService printerService)
    {
        InitializeComponent();
        BindingContext = new DocumentsViewModel(printerService);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ContractsPanel.IsVisible)
            await ContractsPanel.FadeTo(1, 400, Easing.CubicIn);
        if (BillsPanel.IsVisible)
            await BillsPanel.FadeTo(1, 400, Easing.CubicIn);
        if (WelcomePanel.IsVisible)
            await WelcomePanel.FadeTo(1, 400, Easing.CubicIn);
    }
}
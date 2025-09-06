using Vakilaw.Services;
using Vakilaw.ViewModels;

namespace Vakilaw.Views
{
    public partial class LawBankPage : ContentPage
    {
        public LawBankVM ViewModel { get; }

        public LawBankPage()
        {
            InitializeComponent();

            var database = new LawDatabase();
            var importer = new LawImporter(database);
            ViewModel = new LawBankVM(importer, database);
            BindingContext = ViewModel;
        }

        private async void OnSearchIconClicked(object sender, EventArgs e)
        {
            if (!PageSearchBar.IsVisible)
            {
                PageSearchBar.Opacity = 0;
                PageSearchBar.IsVisible = true;
                await PageSearchBar.FadeTo(1, 250);
                PageSearchBar.Focus();
            }
            else
            {
                await PageSearchBar.FadeTo(0, 250);
                PageSearchBar.IsVisible = false;
            }
        }

    }
}
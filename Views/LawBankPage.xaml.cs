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
    }
}
using Vakilaw.Services;
using Vakilaw.ViewModels;

namespace Vakilaw.Views
{
    public partial class LawBankPage : ContentPage
    {
        public LawBankVM ViewModel { get; }

        public LawBankPage(LawBankVM viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel; // ViewModel از DI تزریق شده
        }    
    }
}

using Vakilaw.Views;

namespace Vakilaw
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("LawBankPage", typeof(LawBankPage));
            Routing.RegisterRoute("ClientsAndCasesPage", typeof(ClientsAndCasesPage));
            Routing.RegisterRoute("DocumentsPage", typeof(DocumentsPage));
        }
    }
}
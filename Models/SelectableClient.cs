using CommunityToolkit.Mvvm.ComponentModel;

namespace Vakilaw.Models
{
    public partial class SelectableClient : ObservableObject
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        [ObservableProperty]
        private bool isSelected;
    }
}
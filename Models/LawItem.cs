using CommunityToolkit.Mvvm.ComponentModel;

namespace Vakilaw.Models
{
    public partial class LawItem : ObservableObject
    {
        // کلید اصلی
        public int Id { get; set; }

        [ObservableProperty]
        private int articleNumber;

        [ObservableProperty]
        private string lawType;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string text;

        [ObservableProperty]
        private bool isBookmarked;

        [ObservableProperty]
        private bool isExpanded;
    }
}
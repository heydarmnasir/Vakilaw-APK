using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using Vakilaw.Converters;

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
        [JsonConverter(typeof(StringOrArrayToListConverter))]
        private List<string> notes = new();

        [ObservableProperty]
        private bool isBookmarked;

        [ObservableProperty]
        private bool isExpanded;

        // ← اینجا پراپرتی NotesText رو اضافه کن
        public string NotesText =>
            (Notes != null && Notes.Any())
                ? string.Join(Environment.NewLine, Notes)
                : "یادداشتی ثبت نشده است.";
    }
}
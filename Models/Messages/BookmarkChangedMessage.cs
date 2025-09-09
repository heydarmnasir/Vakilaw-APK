namespace Vakilaw.Models;
public class BookmarkChangedMessage
{
    public int LawId { get; }
    public bool IsBookmarked { get; }

    public BookmarkChangedMessage(int lawId, bool isBookmarked)
    {
        LawId = lawId;
        IsBookmarked = isBookmarked;
    }
}
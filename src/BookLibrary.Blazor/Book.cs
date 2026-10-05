namespace BookLibrary.Blazor;

public sealed class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Isbn { get; set; } = "";
    public string Genre { get; set; } = "";
    public int PublishedYear { get; set; } = DateTime.Now.Year;
    public bool IsAvailable { get; set; } = true;
}

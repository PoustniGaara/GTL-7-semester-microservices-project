namespace SearchService.Infrastructure.Data;

public class SearchDocument
{
    public int Id { get; set; }           // Local ID
    public int BookId { get; set; }       // Reference to Catalog Book
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string Isbn { get; set; } = default!;
}
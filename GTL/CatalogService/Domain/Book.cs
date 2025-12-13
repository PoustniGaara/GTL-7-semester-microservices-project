namespace CatalogService.Domain;

public class Book
{
    public int Id { get; set; }
    public string Isbn { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
}
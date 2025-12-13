using CatalogService.Domain;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb")));

builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

var app = builder.Build();

app.MapPost("/catalog/books", async (Book book, CatalogDbContext db, IEventPublisher publisher) =>
{
    db.Books.Add(book);
    await db.SaveChangesAsync();

    var evt = new
    {
        book.Id,
        book.Isbn,
        book.Title,
        book.Author,
        CreatedAt = DateTime.UtcNow
    };

    await publisher.PublishAsync("book.created", evt);

    return Results.Created($"/catalog/books/{book.Id}", book);
});

app.MapGet("/catalog/books/{id:int}", async (int id, CatalogDbContext db) =>
{
    var book = await db.Books.FindAsync(id);
    return book is null ? Results.NotFound() : Results.Ok(book);
});

app.Run();
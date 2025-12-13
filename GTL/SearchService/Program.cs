using Microsoft.EntityFrameworkCore;
using SearchService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SearchDb")));

builder.Services.AddHostedService<RabbitMqCatalogConsumer>();

var app = builder.Build();

app.MapGet("/search", async (string? query, SearchDbContext db) =>
{
    var q = query ?? "";
    var results = await db.SearchDocuments
        .Where(d => d.Title.Contains(q) || d.Author.Contains(q))
        .ToListAsync();

    return Results.Ok(results);
});

app.Run();
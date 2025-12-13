using Microsoft.EntityFrameworkCore;
using SearchService.Infrastructure.Data;

namespace SearchService.Infrastructure.Messaging;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options) { }

    public DbSet<SearchDocument> SearchDocuments => Set<SearchDocument>();
}



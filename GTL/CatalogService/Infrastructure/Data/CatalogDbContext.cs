using Microsoft.EntityFrameworkCore;
using CatalogService.Domain;

namespace CatalogService.Infrastructure.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
}
using CheckoutService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CheckoutService.Infrastructure.Data;

public class CheckoutDbContext : DbContext
{
    public CheckoutDbContext(DbContextOptions<CheckoutDbContext> options) : base(options) { }

    public DbSet<Loan> Loans => Set<Loan>();
}
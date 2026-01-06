using CheckoutService.Domain;
using CheckoutService.Infrastructure.Data;
using CheckoutService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CheckoutDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CheckoutDb")));

builder.Services.AddSingleton<IEventPublisher>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    // blocking here is fine for demo startup; for production you'd do a hosted service
    return RabbitMqEventPublisher.CreateAsync(config).GetAwaiter().GetResult();
});

var app = builder.Build();

app.MapPost("/checkout/borrow", async (
    BorrowRequest request,
    CheckoutDbContext db,
    IEventPublisher publisher) =>
{
    // Local ACID transaction
    var loan = new Loan
    {
        UserId = request.UserId,
        ItemId = request.ItemId,
        CreatedAt = DateTime.UtcNow,
        DueDate = DateTime.UtcNow.AddDays(30),
        Status = LoanStatus.Active   // simplified: we skip Pending/Rejected for v1
    };

    db.Loans.Add(loan);
    await db.SaveChangesAsync();

    // Publish LoanCreated event – start of SAGA chain
    var evt = new
    {
        loan.Id,
        loan.UserId,
        loan.ItemId,
        loan.DueDate,
        loan.Status,
        CreatedAt = loan.CreatedAt
    };

    await publisher.PublishAsync("loan.created", evt);

    return Results.Ok(loan);
});

app.Run();

public record BorrowRequest(int UserId, int ItemId);
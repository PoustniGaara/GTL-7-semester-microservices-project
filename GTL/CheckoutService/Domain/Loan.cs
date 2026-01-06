namespace CheckoutService.Domain;

public enum LoanStatus
{
    Pending,
    Active,
    Rejected
}

public class Loan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public LoanStatus Status { get; set; }
}
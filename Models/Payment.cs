namespace ShopEasy.Models;

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    Cash,
    BankTransfer,
    Wallet
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

public class Payment
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public decimal Amount { get; set; }

    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
}

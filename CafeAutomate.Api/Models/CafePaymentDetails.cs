namespace CafeAutomate.Api.Models;

public class CafePaymentDetails
{
    public int Id { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string IBANOrCardNumber { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

using CafeAutomate.Api.Data;
using CafeAutomate.Api.Models;
using Microsoft.Extensions.Configuration;

namespace CafeAutomate.Api.Services;

public class SeederService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<SeederService> _logger;

    public SeederService(AppDbContext db, IConfiguration config, ILogger<SeederService> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (!_db.Users.Any(u => u.Role == UserRole.WebsiteAdmin))
        {
            var email    = _config["Seed:WebsiteAdmin:Email"]
                           ?? throw new InvalidOperationException("Seed__WebsiteAdmin__Email missing from .env");
            var password = _config["Seed:WebsiteAdmin:Password"]
                           ?? throw new InvalidOperationException("Seed__WebsiteAdmin__Password missing from .env");
            var fullName = _config["Seed:WebsiteAdmin:FullName"] ?? "Website Admin";

            _db.Users.Add(new User
            {
                FullName     = fullName,
                Email        = email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role         = UserRole.WebsiteAdmin,
                IsActive     = true
            });
            _logger.LogInformation("Seeded Website Admin account ({Email}).", email);
        }

        if (!_db.Users.Any(u => u.Role == UserRole.CafeAdmin))
        {
            var email    = _config["Seed:CafeAdmin:Email"]
                           ?? throw new InvalidOperationException("Seed__CafeAdmin__Email missing from .env");
            var password = _config["Seed:CafeAdmin:Password"]
                           ?? throw new InvalidOperationException("Seed__CafeAdmin__Password missing from .env");
            var fullName = _config["Seed:CafeAdmin:FullName"] ?? "Cafe Admin";

            _db.Users.Add(new User
            {
                FullName     = fullName,
                Email        = email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role         = UserRole.CafeAdmin,
                IsActive     = true
            });
            _logger.LogInformation("Seeded Cafe Admin account ({Email}).", email);
        }

        if (!_db.CafePaymentDetails.Any())
        {
            _db.CafePaymentDetails.Add(new CafePaymentDetails
            {
                AccountHolderName = "Cafe Automate",
                BankName          = "HBL Bank",
                AccountNumber     = "1234-5678-9012",
                IBANOrCardNumber  = "PK36HABB0000000001234567",
                Instructions      = "Transfer the exact amount and send a screenshot to WhatsApp. Your order will be confirmed once payment is received."
            });
        }

        await _db.SaveChangesAsync();
    }
}

using CafeAutomate.Api.Data;
using CafeAutomate.Api.DTOs;
using CafeAutomate.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeAutomate.Api.Controllers;

[ApiController]
[Route("api/cafe-payment-details")]
public class CafePaymentController : ControllerBase
{
    private readonly AppDbContext _db;

    public CafePaymentController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Get()
    {
        var details = _db.CafePaymentDetails.FirstOrDefault();
        if (details == null) return NotFound(new { error = "Payment details not configured yet." });

        return Ok(new CafePaymentDetailsResponse(
            details.AccountHolderName,
            details.BankName,
            details.AccountNumber,
            details.IBANOrCardNumber,
            details.Instructions,
            details.UpdatedAt
        ));
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] CafePaymentDetailsRequest req)
    {
        if (!User.IsCafeAdmin()) return Forbid();

        var details = _db.CafePaymentDetails.FirstOrDefault();
        if (details == null)
        {
            _db.CafePaymentDetails.Add(new Models.CafePaymentDetails
            {
                AccountHolderName = req.AccountHolderName,
                BankName = req.BankName,
                AccountNumber = req.AccountNumber,
                IBANOrCardNumber = req.IBANOrCardNumber,
                Instructions = req.Instructions,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            details.AccountHolderName = req.AccountHolderName;
            details.BankName = req.BankName;
            details.AccountNumber = req.AccountNumber;
            details.IBANOrCardNumber = req.IBANOrCardNumber;
            details.Instructions = req.Instructions;
            details.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Payment details updated." });
    }
}

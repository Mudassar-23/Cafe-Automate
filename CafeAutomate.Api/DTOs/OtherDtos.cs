namespace CafeAutomate.Api.DTOs;

public record CafePaymentDetailsResponse(
    string AccountHolderName,
    string BankName,
    string AccountNumber,
    string IBANOrCardNumber,
    string Instructions,
    DateTime UpdatedAt
);

public record CafePaymentDetailsRequest(
    string AccountHolderName,
    string BankName,
    string AccountNumber,
    string IBANOrCardNumber,
    string Instructions
);

public record ContactMessageRequest(string Name, string Email, string Message);

public record ContactMessageResponse(
    int Id,
    string Name,
    string Email,
    string Message,
    DateTime SubmittedAt,
    bool IsRead
);

public record UserResponse(
    int Id,
    string FullName,
    string Email,
    int Role,
    string RoleName,
    bool IsActive,
    DateTime CreatedAt
);

public record UpdateUserStatusRequest(bool IsActive);

public record ChangeUserPasswordRequest(string NewPassword);

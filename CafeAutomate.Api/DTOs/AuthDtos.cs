namespace CafeAutomate.Api.DTOs;

public record SignupRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    int UserId,
    string FullName,
    string Email,
    int Role,
    string RoleName
);

public record MeResponse(
    int UserId,
    string FullName,
    string Email,
    int Role,
    string RoleName,
    bool IsActive
);

using CafeAutomate.Api.Models;

namespace CafeAutomate.Api.Middleware;

public static class RoleGuardExtensions
{
    public static bool IsWebsiteAdmin(this System.Security.Claims.ClaimsPrincipal user)
        => user.FindFirst("role")?.Value == "1";

    public static bool IsCafeAdmin(this System.Security.Claims.ClaimsPrincipal user)
        => user.FindFirst("role")?.Value == "2";

    public static bool IsUser(this System.Security.Claims.ClaimsPrincipal user)
        => user.FindFirst("role")?.Value == "3";

    public static int GetUserId(this System.Security.Claims.ClaimsPrincipal user)
    {
        var sub = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(sub, out var id) ? id : 0;
    }
}

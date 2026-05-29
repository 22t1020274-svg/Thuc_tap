using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace SV22T1020274.Shop.Security;

/// <summary>Thông tin khách đăng nhập lưu trong cookie (Shop).</summary>
public class ShopUserData
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Photo { get; set; }
    public List<string>? Roles { get; set; }

    private List<Claim> Claims
    {
        get
        {
            var claims = new List<Claim>
            {
                new(nameof(UserId), UserId ?? ""),
                new(nameof(UserName), UserName ?? ""),
                new(nameof(DisplayName), DisplayName ?? ""),
                new(nameof(Email), Email ?? ""),
                new(nameof(Photo), Photo ?? "")
            };
            if (Roles != null)
                foreach (var role in Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            return claims;
        }
    }

    public ClaimsPrincipal CreatePrincipal()
    {
        var id = new ClaimsIdentity(Claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(id);
    }
}

public static class ShopPrincipalExtensions
{
    public static ShopUserData? GetShopUser(this ClaimsPrincipal principal)
    {
        try
        {
            if (principal.Identity?.IsAuthenticated != true)
                return null;

            var userData = new ShopUserData
            {
                UserId = principal.FindFirstValue(nameof(ShopUserData.UserId)),
                UserName = principal.FindFirstValue(nameof(ShopUserData.UserName)),
                DisplayName = principal.FindFirstValue(nameof(ShopUserData.DisplayName)),
                Email = principal.FindFirstValue(nameof(ShopUserData.Email)),
                Photo = principal.FindFirstValue(nameof(ShopUserData.Photo)),
                Roles = new List<string>()
            };
            foreach (var claim in principal.FindAll(ClaimTypes.Role))
                userData.Roles.Add(claim.Value);
            return userData;
        }
        catch
        {
            return null;
        }
    }

    public static int? GetCustomerId(this ClaimsPrincipal principal)
    {
        var u = principal.GetShopUser();
        if (u?.UserId == null || !int.TryParse(u.UserId, out var id) || id <= 0)
            return null;
        return id;
    }
}

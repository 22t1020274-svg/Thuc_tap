using System.Text.Json;
using SV22T1020274.Shop.Models;

namespace SV22T1020274.Shop.Helpers;

public static class ShoppingCartSession
{
    private const string SessionKey = "SV22T1020274.Shop.Cart";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static List<CartLine> GetCart(ISession session)
    {
        var json = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json))
            return new List<CartLine>();
        try
        {
            return JsonSerializer.Deserialize<List<CartLine>>(json, JsonOptions) ?? new List<CartLine>();
        }
        catch
        {
            return new List<CartLine>();
        }
    }

    public static void SaveCart(ISession session, List<CartLine> cart)
    {
        session.SetString(SessionKey, JsonSerializer.Serialize(cart ?? new List<CartLine>(), JsonOptions));
    }

    public static void Clear(ISession session) => session.Remove(SessionKey);
}

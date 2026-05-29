using Microsoft.AspNetCore.Mvc;
using SV22T1020274.Application;
using SV22T1020274.Shop.Helpers;
using SV22T1020274.Shop.Models;

namespace SV22T1020274.Shop.Controllers;

public class CartController : Controller
{
    private const int MaxQtyPerLine = 999;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = ShoppingCartSession.GetCart(HttpContext.Session);
        var lines = new List<CartDisplayLine>();
        decimal total = 0;
        foreach (var line in cart)
        {
            var p = await CatalogDataService.GetProductAsync(line.ProductID);
            if (p == null || !p.IsSelling)
                continue;
            var sub = p.Price * line.Quantity;
            total += sub;
            lines.Add(new CartDisplayLine
            {
                ProductID = p.ProductID,
                ProductName = p.ProductName,
                Unit = p.Unit,
                UnitPrice = p.Price,
                Quantity = line.Quantity,
                LineTotal = sub,
                Photo = p.Photo
            });
        }

        ViewBag.Total = total;
        return View(lines);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        if (productId <= 0 || quantity <= 0)
        {
            TempData["Error"] = "Sản phẩm hoặc số lượng không hợp lệ.";
            return RedirectToLocal(returnUrl);
        }

        var product = await CatalogDataService.GetProductAsync(productId);
        if (product == null || !product.IsSelling)
        {
            TempData["Error"] = "Sản phẩm không tồn tại hoặc ngừng bán.";
            return RedirectToLocal(returnUrl);
        }

        quantity = Math.Min(quantity, MaxQtyPerLine);
        var cart = ShoppingCartSession.GetCart(HttpContext.Session);
        var existing = cart.FirstOrDefault(x => x.ProductID == productId);
        if (existing != null)
        {
            existing.Quantity = Math.Min(existing.Quantity + quantity, MaxQtyPerLine);
        }
        else
        {
            cart.Add(new CartLine { ProductID = productId, Quantity = quantity });
        }

        ShoppingCartSession.SaveCart(HttpContext.Session, cart);
        TempData["Message"] = "Đã thêm vào giỏ hàng.";
        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int quantity)
    {
        var cart = ShoppingCartSession.GetCart(HttpContext.Session);
        var line = cart.FirstOrDefault(x => x.ProductID == productId);
        if (line == null)
            return RedirectToAction(nameof(Index));

        if (quantity <= 0)
            cart.Remove(line);
        else
            line.Quantity = Math.Min(quantity, MaxQtyPerLine);

        ShoppingCartSession.SaveCart(HttpContext.Session, cart);
        TempData["Message"] = "Đã cập nhật giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var cart = ShoppingCartSession.GetCart(HttpContext.Session);
        cart.RemoveAll(x => x.ProductID == productId);
        ShoppingCartSession.SaveCart(HttpContext.Session, cart);
        TempData["Message"] = "Đã xóa khỏi giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        ShoppingCartSession.Clear(HttpContext.Session);
        TempData["Message"] = "Đã xóa toàn bộ giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }
}

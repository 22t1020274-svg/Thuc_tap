using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020274.Application;
using SV22T1020274.Domain.Sales;
using SV22T1020274.Shop.Helpers;
using SV22T1020274.Shop.Models;
using SV22T1020274.Shop.Security;

namespace SV22T1020274.Shop.Controllers;

[Authorize(Roles = SecurityConstants.ShopCustomerRole)]
public class OrderController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = ShoppingCartSession.GetCart(HttpContext.Session);
        if (cart.Count == 0)
        {
            TempData["Error"] = "Giỏ hàng đang trống.";
            return RedirectToAction("Index", "Cart");
        }

        var id = User.GetCustomerId();
        if (id == null)
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Checkout)) });

        var customer = await PartnerDataService.GetCustomerAsync(id.Value);
        if (customer == null)
            return RedirectToAction("Login", "Account");

        var vm = new CheckoutViewModel
        {
            DeliveryProvince = string.IsNullOrWhiteSpace(customer.Province) ? null : customer.Province.Trim(),
            DeliveryAddress = customer.Address?.Trim() ?? "",
            DeliveryPhone = customer.Phone?.Trim() ?? ""
        };

        ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var id = User.GetCustomerId();
        if (id == null)
            return RedirectToAction("Login", "Account");

        var cart = ShoppingCartSession.GetCart(HttpContext.Session);
        if (cart.Count == 0)
        {
            TempData["Error"] = "Giỏ hàng đang trống.";
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(model);
        }

        var details = new List<OrderDetail>();
        foreach (var line in cart)
        {
            var p = await CatalogDataService.GetProductAsync(line.ProductID);
            if (p == null || !p.IsSelling)
                continue;
            if (line.Quantity <= 0)
                continue;
            details.Add(new OrderDetail
            {
                ProductID = p.ProductID,
                Quantity = line.Quantity,
                SalePrice = p.Price
            });
        }

        if (details.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Không còn sản phẩm hợp lệ trong giỏ.");
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(model);
        }

        var orderId = await SalesDataService.CreateOrderWithDetailsAsync(
            id.Value,
            model.DeliveryProvince?.Trim() ?? "",
            model.DeliveryAddress?.Trim() ?? "",
            details,
            model.DeliveryPhone?.Trim());

        if (orderId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Không tạo được đơn hàng. Kiểm tra địa chỉ / tỉnh thành.");
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(model);
        }

        ShoppingCartSession.Clear(HttpContext.Session);
        TempData["Message"] = $"Đặt hàng thành công. Mã đơn: {orderId}.";
        return RedirectToAction(nameof(Detail), new { id = orderId });
    }

    [HttpGet]
    public async Task<IActionResult> History(int page = 1, int status = 0)
    {
        var id = User.GetCustomerId();
        if (id == null)
            return RedirectToAction("Login", "Account");

        var input = new OrderSearchInput
        {
            Page = page < 1 ? 1 : page,
            PageSize = 10,
            Status = (OrderStatusEnum)status,
            SearchValue = ""
        };

        var result = await SalesDataService.ListCustomerOrdersAsync(id.Value, input);
        ViewBag.StatusCode = status;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var customerId = User.GetCustomerId();
        if (customerId == null)
            return RedirectToAction("Login", "Account");

        var order = await SalesDataService.GetOrderForCustomerAsync(id, customerId.Value);
        if (order == null)
            return NotFound();

        var details = await SalesDataService.ListDetailsAsync(id);
        ViewBag.Details = details;
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var customerId = User.GetCustomerId();
        if (customerId == null)
            return RedirectToAction("Login", "Account");

        var order = await SalesDataService.GetOrderForCustomerAsync(id, customerId.Value);
        if (order == null)
        {
            TempData["Error"] = "Không tìm thấy đơn hàng.";
            return RedirectToAction(nameof(History));
        }

        if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
        {
            TempData["Error"] = "Đơn này không thể hủy ở trạng thái hiện tại.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var ok = await SalesDataService.CancelOrderAsync(id);
        TempData[ok ? "Message" : "Error"] = ok ? "Đã gửi yêu cầu hủy đơn." : "Không hủy được đơn hàng.";
        return RedirectToAction(nameof(Detail), new { id });
    }
}

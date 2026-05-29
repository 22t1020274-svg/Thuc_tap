using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020274.Application;
using SV22T1020274.Domain.Partner;
using SV22T1020274.Shop.Models;
using SV22T1020274.Shop.Security;

namespace SV22T1020274.Shop.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (ok, _, err) = await SecurityDataSerer.RegisterCustomerWithAccountAsync(model.Email, model.Password);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, err ?? "Đăng ký không thành công.");
            return View(model);
        }

        TempData["Message"] = "Đăng ký thành công. Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var hash = HashHelper.HashMD5(model.Password);
        var acc = await SecurityDataSerer.CustomerAuthorizeAsync(model.Email, hash);
        if (acc == null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng, hoặc tài khoản bị khóa.");
            return View(model);
        }

        var userData = new ShopUserData
        {
            UserId = acc.UserId,
            UserName = acc.UserName,
            DisplayName = acc.DisplayName,
            Email = acc.Email,
            Photo = acc.Photo,
            Roles = (acc.RoleNames ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
        };

        var props = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            userData.CreatePrincipal(),
            props);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize(Roles = SecurityConstants.ShopCustomerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Message"] = "Bạn đã đăng xuất.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    [Authorize(Roles = SecurityConstants.ShopCustomerRole)]
    public async Task<IActionResult> Profile()
    {
        var id = User.GetCustomerId();
        if (id == null)
            return RedirectToAction(nameof(Login));

        var c = await PartnerDataService.GetCustomerAsync(id.Value);
        if (c == null)
            return RedirectToAction(nameof(Login));

        var vm = new ProfileEditViewModel
        {
            CustomerName = CustomerProfileHelper.ForEditableDisplay(c.CustomerName),
            ContactName = CustomerProfileHelper.ForEditableDisplay(c.ContactName),
            Province = string.IsNullOrWhiteSpace(c.Province) ? null : c.Province.Trim(),
            Address = c.Address?.Trim() ?? "",
            Phone = c.Phone?.Trim() ?? "",
            Email = c.Email
        };

        ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = SecurityConstants.ShopCustomerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileEditViewModel model)
    {
        var id = User.GetCustomerId();
        if (id == null)
            return RedirectToAction(nameof(Login));

        var existing = await PartnerDataService.GetCustomerAsync(id.Value);
        if (existing == null)
            return RedirectToAction(nameof(Login));

        model.Email = existing.Email;
        if (!ModelState.IsValid)
        {
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(model);
        }

        if (!await PartnerDataService.ValidatelCustomerEmailAsync(existing.Email, id.Value))
        {
            ModelState.AddModelError(nameof(model.Email), "Email không hợp lệ.");
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(model);
        }

        var data = new Customer
        {
            CustomerID = id.Value,
            CustomerName = model.CustomerName.Trim(),
            ContactName = string.IsNullOrWhiteSpace(model.ContactName) ? model.CustomerName.Trim() : model.ContactName.Trim(),
            Province = model.Province?.Trim(),
            Address = model.Address?.Trim(),
            Phone = model.Phone?.Trim(),
            Email = existing.Email.Trim(),
            IsLocked = existing.IsLocked
        };

        if (!await PartnerDataService.UpdateCustomerAsync(data))
        {
            ModelState.AddModelError(string.Empty, "Không lưu được hồ sơ. Kiểm tra dữ liệu (tỉnh/thành phải có trong danh mục).");
            ViewBag.Provinces = await DictionaryDataService.ListProvincesAsync();
            return View(model);
        }

        TempData["Message"] = "Đã cập nhật hồ sơ.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize(Roles = SecurityConstants.ShopCustomerRole)]
    public IActionResult ChangePassword() => View(new CustomerChangePasswordViewModel());

    [HttpPost]
    [Authorize(Roles = SecurityConstants.ShopCustomerRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(CustomerChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var u = User.GetShopUser();
        var email = u?.UserName?.Trim();
        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(Login));

        var oldHash = HashHelper.HashMD5(model.OldPassword);
        var newHash = HashHelper.HashMD5(model.NewPassword);
        var ok = await SecurityDataSerer.ChangeCustomerPasswordAsync(email, oldHash, newHash);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
            return View(model);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Message"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
        return RedirectToAction(nameof(Login));
    }
}

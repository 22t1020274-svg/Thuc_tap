using SV22T1020274.Application.Abstractions;
using SV22T1020274.Domain.Partner;
using SV22T1020274.Domain.Security;

namespace SV22T1020274.Application;

/// <summary>
/// Use case: xác thực và đăng ký tài khoản.
/// </summary>
public static class SecurityDataSerer
{
    private static ISecurityRepository securityDB = null!;

    public static void Configure(ISecurityRepository securityRepository)
    {
        securityDB = securityRepository;
    }

    public static async Task<UserAccount?> EmployeeAuthorizeAsync(string userName, string password)
    {
        return await securityDB.EmployeeAuthorizeAsync(userName, password);
    }

    public static async Task<UserAccount?> CustomerAuthorizeAsync(string userName, string passwordMd5)
    {
        userName = (userName ?? "").Trim();
        if (userName.Length == 0 || string.IsNullOrEmpty(passwordMd5))
            return null;

        var acc = await securityDB.CustomerAuthorizeAsync(userName, passwordMd5);
        if (acc != null && CustomerProfileHelper.IsPendingDisplayName(acc.DisplayName))
            acc.DisplayName = acc.Email ?? acc.UserName;
        return acc;
    }

    public static async Task<(bool ok, int customerId, string? error)> RegisterCustomerWithAccountAsync(string email, string plainPassword)
    {
        email = (email ?? "").Trim();
        if (string.IsNullOrWhiteSpace(email))
            return (false, 0, "Vui lòng nhập email.");
        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6)
            return (false, 0, "Mật khẩu tối thiểu 6 ký tự.");

        if (!await PartnerDataService.ValidatelCustomerEmailAsync(email, 0))
            return (false, 0, "Email đã được sử dụng.");

        var pending = CustomerProfileHelper.PendingCustomerDisplayName;
        var customer = new Customer
        {
            CustomerName = pending,
            ContactName = pending,
            Province = null,
            Address = null,
            Phone = null,
            Email = email,
            IsLocked = false,
        };

        var hash = HashHelper.HashMD5(plainPassword);
        return await securityDB.RegisterCustomerWithAccountAsync(customer, hash);
    }

    public static async Task<bool> ChangeCustomerPasswordAsync(string userName, string oldPasswordMd5, string newPasswordMd5)
    {
        return await securityDB.ChangeCustomerPasswordAsync(userName, oldPasswordMd5, newPasswordMd5);
    }
}

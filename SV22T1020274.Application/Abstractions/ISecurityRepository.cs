using SV22T1020274.Domain.Partner;
using SV22T1020274.Domain.Security;

namespace SV22T1020274.Application.Abstractions;

/// <summary>
/// Cổng (port) xác thực và quản lý tài khoản khách/nhân viên.
/// </summary>
public interface ISecurityRepository
{
    Task<UserAccount?> EmployeeAuthorizeAsync(string userName, string password);

    Task<UserAccount?> CustomerAuthorizeAsync(string userName, string passwordMd5);

    Task<(bool ok, int customerId, string? error)> RegisterCustomerWithAccountAsync(Customer customer, string passwordHash);

    Task<bool> ChangeCustomerPasswordAsync(string userName, string oldPasswordMd5, string newPasswordMd5);
}

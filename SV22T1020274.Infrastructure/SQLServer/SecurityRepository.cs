using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020274.Application;
using SV22T1020274.Application.Abstractions;
using SV22T1020274.Domain.Partner;
using SV22T1020274.Domain.Security;

namespace SV22T1020274.Infrastructure.SQLServer;

public class SecurityRepository : ISecurityRepository
{
    private readonly string _connectionString;

    public SecurityRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private static string DescribeRegisterSqlError(SqlException ex)
    {
        return ex.Number switch
        {
            208 => "Không tìm thấy bảng trên database (kiểm tra tên bảng Customers và connection string).",
            207 => "Bảng Customers không khớp code (thiếu/sai tên cột Email, Password, …).",
            2627 or 2601 => "Email này đã được dùng cho một khách hàng khác.",
            515 => "Không thể lưu: có cột NOT NULL chưa được gán (ví dụ Customers.Password). Cho phép NULL hoặc cập nhật code để gửi giá trị mặc định.",
            547 => "Vi phạm ràng buộc khóa ngoại hoặc check trên database. Chi tiết: " + ex.Message,
            8152 => "Chuỗi quá dài so với cột SQL (thường gặp: Customers.Email chỉ nvarchar(50)). Rút ngắn email hoặc ALTER COLUMN Email.",
            _ => $"Lỗi SQL ({ex.Number}): {ex.Message}",
        };
    }

    public async Task<UserAccount?> EmployeeAuthorizeAsync(string userName, string password)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
                SELECT TOP (1)
                    CAST(EmployeeID AS varchar(20)) AS UserId,
                    Email AS UserName,
                    FullName AS DisplayName,
                    Email,
                    Photo,
                    RoleNames
                FROM Employees
                WHERE Email = @userName
                  AND [Password] = @password
                  AND IsWorking = 1;";

        return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql, new { userName, password });
    }

    public async Task<UserAccount?> CustomerAuthorizeAsync(string userName, string passwordMd5)
    {
        userName = (userName ?? "").Trim();
        if (userName.Length == 0 || string.IsNullOrEmpty(passwordMd5))
            return null;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
                SELECT TOP (1)
                    CAST(c.CustomerID AS varchar(20)) AS UserId,
                    LTRIM(RTRIM(c.Email)) AS UserName,
                    LTRIM(RTRIM(c.CustomerName)) AS DisplayName,
                    LTRIM(RTRIM(c.Email)) AS Email,
                    CAST(N'' AS nvarchar(500)) AS Photo,
                    @shopRole AS RoleNames
                FROM Customers c
                WHERE LTRIM(RTRIM(c.Email)) = @userName
                  AND c.[Password] = @password
                  AND ISNULL(c.IsLocked, 0) = 0;";

        return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql,
            new { userName, password = passwordMd5, shopRole = SecurityConstants.ShopCustomerRole });
    }

    public async Task<(bool ok, int customerId, string? error)> RegisterCustomerWithAccountAsync(Customer customer, string passwordHash)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var tran = await connection.BeginTransactionAsync();

        try
        {
            const string insertCustomer = @"
                    INSERT INTO Customers (CustomerName, ContactName, Province, Address, Phone, Email, [Password], IsLocked)
                    VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var customerId = await connection.ExecuteScalarAsync<int>(insertCustomer, new
            {
                customer.CustomerName,
                customer.ContactName,
                customer.Province,
                customer.Address,
                customer.Phone,
                customer.Email,
                Password = passwordHash,
                customer.IsLocked
            }, tran);

            if (customerId <= 0)
            {
                await tran.RollbackAsync();
                return (false, 0, "Không tạo được khách hàng.");
            }

            await tran.CommitAsync();
            return (true, customerId, null);
        }
        catch (SqlException ex)
        {
            await tran.RollbackAsync();
            return (false, 0, DescribeRegisterSqlError(ex));
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            return (false, 0, $"Đăng ký thất bại: {ex.Message}");
        }
    }

    public async Task<bool> ChangeCustomerPasswordAsync(string userName, string oldPasswordMd5, string newPasswordMd5)
    {
        userName = (userName ?? "").Trim();
        if (userName.Length == 0 || string.IsNullOrEmpty(oldPasswordMd5) || string.IsNullOrEmpty(newPasswordMd5))
            return false;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var n = await connection.ExecuteAsync(@"
                UPDATE Customers
                SET [Password] = @newPassword
                WHERE LTRIM(RTRIM(Email)) = @userName
                  AND [Password] = @oldPassword
                  AND ISNULL(IsLocked, 0) = 0;",
            new
            {
                userName,
                oldPassword = oldPasswordMd5,
                newPassword = newPasswordMd5
            });
        return n > 0;
    }
}

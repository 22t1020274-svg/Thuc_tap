using SV22T1020274.Domain.Partner;

namespace SV22T1020274.Application.Abstractions;

public interface ISupplierRepository : IGenericRepository<Supplier>
{
    Task<bool> ValidateEmailAsync(string email, int supplierID = 0);
}

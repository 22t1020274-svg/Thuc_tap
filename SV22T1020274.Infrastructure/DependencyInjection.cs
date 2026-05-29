using Microsoft.Extensions.DependencyInjection;
using SV22T1020274.Application;
using SV22T1020274.Application.Abstractions;
using SV22T1020274.Domain.Catalog;
using SV22T1020274.Domain.DataDictionary;
using SV22T1020274.Domain.Partner;
using SV22T1020274.Infrastructure.SQLServer;

namespace SV22T1020274.Infrastructure;

/// <summary>
/// Composition root: đăng ký repository (Infrastructure) và khởi tạo application services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddLiteCommerceInfrastructure(this IServiceCollection services, string connectionString)
    {
        Configuration.Initialize(connectionString);

        var categoryRepo = new CategoryRepository(connectionString);
        var productRepo = new ProductRepository(connectionString);
        var orderRepo = new OrderRepository(connectionString);
        var employeeRepo = new EmployeeRepository(connectionString);
        var customerRepo = new CustomerRepository(connectionString);
        var supplierRepo = new SupplierRepository(connectionString);
        var shipperRepo = new ShipperRepository(connectionString);
        var provinceRepo = new ProvinceRepository(connectionString);
        var securityRepo = new SecurityRepository(connectionString);

        CatalogDataService.Configure(categoryRepo, productRepo);
        SalesDataService.Configure(orderRepo);
        HRDataService.Configure(employeeRepo);
        PartnerDataService.Configure(supplierRepo, customerRepo, shipperRepo);
        DictionaryDataService.Configure(provinceRepo);
        SecurityDataSerer.Configure(securityRepo);

        services.AddSingleton<IGenericRepository<Category>>(categoryRepo);
        services.AddSingleton<IProductRepository>(productRepo);
        services.AddSingleton<IOrderRepository>(orderRepo);
        services.AddSingleton<IEmployeeRepository>(employeeRepo);
        services.AddSingleton<ICustomerRepository>(customerRepo);
        services.AddSingleton<ISupplierRepository>(supplierRepo);
        services.AddSingleton<IGenericRepository<Shipper>>(shipperRepo);
        services.AddSingleton<IDataDictionaryRepository<Province>>(provinceRepo);
        services.AddSingleton<ISecurityRepository>(securityRepo);

        return services;
    }
}

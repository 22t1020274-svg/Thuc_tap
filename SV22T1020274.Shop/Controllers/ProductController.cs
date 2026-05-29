using Microsoft.AspNetCore.Mvc;
using SV22T1020274.Application;
using SV22T1020274.Domain.Catalog;
using SV22T1020274.Domain.Common;

namespace SV22T1020274.Shop.Controllers;

public class ProductController : Controller
{
    private readonly IConfiguration _config;

    public ProductController(IConfiguration config) => _config = config;

    [HttpGet]
    public async Task<IActionResult> Index(
        int categoryId = 0,
        string? search = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1)
    {
        var pageSize = 12;
        if (int.TryParse(_config["PageSize"], out var ps) && ps > 0 && ps <= 100)
            pageSize = ps;

        var input = new ProductSearchInput
        {
            Page = page < 1 ? 1 : page,
            PageSize = pageSize,
            SearchValue = search ?? "",
            CategoryID = categoryId,
            SupplierID = 0,
            MinPrice = minPrice ?? 0,
            MaxPrice = maxPrice ?? 0,
            OnlySelling = true
        };

        var categoriesResult = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput
        {
            Page = 1,
            PageSize = 200,
            SearchValue = ""
        });

        var products = await CatalogDataService.ListProductsAsync(input);
        ViewBag.Categories = categoriesResult.DataItems;
        ViewBag.Input = input;
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        if (id <= 0)
            return NotFound();

        var product = await CatalogDataService.GetProductAsync(id);
        if (product == null || !product.IsSelling)
            return NotFound();

        ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
        ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
        Category? cat = null;
        if (product.CategoryID is > 0)
            cat = await CatalogDataService.GetCategoryAsync(product.CategoryID.Value);
        ViewBag.Category = cat;
        return View(product);
    }
}

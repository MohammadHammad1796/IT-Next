using AutoMapper;
using IT_Next.Controllers.UIs.Base;
using IT_Next.Controllers.UIs.ViewModels;
using IT_Next.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs.Customer;

[Route("products")]
public class ShopController : ClientUIController
{
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ShopController(ISubCategoryRepository subCategoryRepository,
    IProductRepository productRepository,
    IMapper mapper)
    {
        _subCategoryRepository = subCategoryRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    [HttpGet("{categoryName}/{subCategoryName}")]
    public async Task<IActionResult> ListProducts(string categoryName, string subCategoryName)
    {
        var subCategory = await _subCategoryRepository.GetByNameAsync(subCategoryName, sc => sc.Category);
        if (subCategory == null)
            return NotFound();

        var category = subCategory.Category;
        if (!category.Name.Equals(categoryName, StringComparison.CurrentCultureIgnoreCase))
            return NotFound();

        ViewData["subCategoryName"] = subCategory.Name;
        ViewData["categoryName"] = category.Name;
        return View("ListProducts");
    }

    [HttpGet("{categoryName}/{subCategoryName}/{productName}")]
    public async Task<IActionResult> GetProduct(string categoryName, string subCategoryName, string productName)
    {
        var product = await _productRepository
            .GetByNameAsync(productName, p => p.Brand, p => p.SubCategory.Category);
        if (product == null)
            return NotFound();

        var subCategory = product.SubCategory;
        if (!subCategory.Name.Equals(subCategoryName, StringComparison.CurrentCultureIgnoreCase))
            return NotFound();

        var category = product.SubCategory.Category;
        if (!category.Name.Equals(categoryName, StringComparison.CurrentCultureIgnoreCase))
            return NotFound();

        ViewData["productName"] = product.Name;
        ViewData["brandName"] = product.Brand.Name;
        ViewData["subCategoryName"] = subCategory.Name;
        ViewData["categoryName"] = category.Name;

        var viewModel = _mapper.Map<ProductDetailsViewModel>(product);
        return View("ProductDetails", viewModel);
    }
}
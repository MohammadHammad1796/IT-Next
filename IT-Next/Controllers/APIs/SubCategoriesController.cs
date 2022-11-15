using AutoMapper;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using IT_Next.Custom.RouteAttributes;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.APIs;

[ApiRoute("subCategories")]
[ApiController]
public class SubCategoriesController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubCategoryService _subCategoryService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IBrandRepository _brandRepository;

    public SubCategoriesController(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ISubCategoryRepository subCategoryRepository,
        ISubCategoryService subCategoryService,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IBrandRepository brandRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _subCategoryRepository = subCategoryRepository;
        _subCategoryService = subCategoryService;
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _brandRepository = brandRepository;
    }

    [HttpGet("{withTotal:bool?}")]
    public async Task<IActionResult> Get([FromQuery] SubCategoryQueryResource queryResource, bool withTotal = true)
    {
        var query = _mapper.Map<Query<SubCategory>>(queryResource);

        if (queryResource.IncludeCategory)
            query.AddIncludeProperty(sc => sc.Category);

        var subCategories = await _subCategoryRepository.GetAsync(query);
        var subCategoryResources = _mapper
            .Map<IEnumerable<SubCategoryResource>>(subCategories).ToList();

        if (!withTotal)
            return Ok(subCategoryResources);

        var count = await _subCategoryRepository.GetCountAsync(query.Conditions);
        var resource = new ItemsWithCountResource<SubCategoryResource>(count, subCategoryResources);
        return Ok(resource);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var subCategory = await _subCategoryRepository.GetByIdAsync(id, sc => sc.Category);

        return subCategory == null ? NotFound() : Ok(_mapper.Map<SubCategoryResource>(subCategory));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveSubCategoryResource saveSubCategoryResource)
    {
        SubCategory subCategory = new();
        if (saveSubCategoryResource.Id != 0)
        {
            subCategory = await _subCategoryRepository.GetByIdAsync(saveSubCategoryResource.Id);
            if (subCategory == null)
                return NotFound();
        }

        if (await _categoryRepository.GetByIdAsync(saveSubCategoryResource.CategoryId) is null)
        {
            ModelState.AddModelError(nameof(saveSubCategoryResource.CategoryId), "Category not found.");
            return BadRequest(ModelState);
        }

        if (!await _subCategoryService.IsUniqueAsync(
                saveSubCategoryResource.Name,
                saveSubCategoryResource.Id))
        {
            ModelState.AddModelError(nameof(saveSubCategoryResource.Name),
                "Sub category name duplicated.");
            return BadRequest(ModelState);
        }

        _mapper.Map(saveSubCategoryResource, subCategory);

        if (saveSubCategoryResource.Id == 0)
            await _subCategoryRepository.AddAsync(subCategory);

        await _unitOfWork.CompleteAsync();

        return Ok(_mapper.Map<SubCategoryResource>(subCategory));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var subCategory = await _subCategoryRepository.GetByIdAsync(id);
        if (subCategory == null)
            return NotFound();

        await _subCategoryRepository.DeleteAsync(subCategory);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    [HttpGet("{name}/products")]
    public async Task<IActionResult> GetProducts(string name, [FromQuery] SubCategoryProductsQueryResource queryResource)
    {
        var subCategory = await _subCategoryRepository.GetByNameAsync(name);
        if (subCategory == null)
            return NotFound();

        var query = new Query<Product>
        {
            Conditions = p => p.SubCategoryId == subCategory.Id
        };
        _mapper.Map(queryResource, query);

        var products = await _productRepository.GetAsync(query);
        var count = await _productRepository.GetCountAsync(query.Conditions);

        var productResources = _mapper.Map<IEnumerable<ProductInSubCategoryProductsListResource>>(products).ToList();
        var resource = new ItemsWithCountResource<ProductInSubCategoryProductsListResource>(count, productResources);
        return Ok(resource);
    }

    [HttpGet("{name}/products/brands")]
    public async Task<IActionResult> GetBrands(string name)
    {
        var subCategory = await _subCategoryRepository.GetByNameAsync(name);
        if (subCategory == null)
            return NotFound();

        var brandsQuery = new Query<Brand>
        {
            Conditions = b => b.Products.Any(p => p.SubCategoryId == subCategory.Id && p.Quantity > 0)
        };
        var subCategoryBrands = await _brandRepository.GetAsync(brandsQuery);

        var resources = _mapper.Map<IEnumerable<BrandResource>>(subCategoryBrands);
        return Ok(resources);
    }

    [HttpGet("{name}/products/minimumAndMaximumPrice")]
    public async Task<IActionResult> GetMinimumAndMaximumPrice(string name)
    {
        var subCategory = await _subCategoryRepository.GetByNameAsync(name);
        if (subCategory == null)
            return NotFound();

        var productQuery = new Query<Product>
        {
            Paging = new Paging(1, 1),
            Conditions = p => p.SubCategoryId == subCategory.Id &&
                p.Quantity > 0
        };
        productQuery.AddOrder(new(p => p.Price * (1 - p.Discount), true));
        var minimumPriceProduct = (await _productRepository.GetAsync(productQuery)).SingleOrDefault();
        if (minimumPriceProduct == null)
            return Ok(new MinimumMaximumPriceResource());

        var minimumPrice = minimumPriceProduct.Price * (1 - minimumPriceProduct.Discount);
        productQuery.Orders.Clear();
        productQuery.AddOrder(new(p => p.Price * (1 - p.Discount), false));
        var maximumPriceProduct = (await _productRepository.GetAsync(productQuery)).Single();
        var maximumPrice = maximumPriceProduct.Price * (1 - maximumPriceProduct.Discount);
        return Ok(new MinimumMaximumPriceResource(minimumPrice, maximumPrice));
    }

    [HttpGet("{name}/related")]
    public async Task<IActionResult> GetRelatedSubCategories(string name)
    {
        var subCategory = await _subCategoryRepository.GetByNameAsync(name);
        if (subCategory == null)
            return NotFound();

        var subCategoriesQuery = new Query<SubCategory>(paging: new Paging(1, 5))
        {
            Conditions = s =>
                s.Id != subCategory.Id &&
                s.CategoryId == subCategory.CategoryId &&
                s.Products.Any(p => p.Quantity > 0)
        };
        var relatedSubCategories = await _subCategoryRepository.GetAsync(subCategoriesQuery);
        return Ok(relatedSubCategories.Select(s => s.Name));
    }
}
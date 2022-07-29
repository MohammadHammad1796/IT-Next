using AutoMapper;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.APIs;

[Route("api/products")]
[ApiController]
public class ProductsController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;
    private readonly IProductRepository _productRepository;

    public ProductsController(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ISubCategoryRepository subCategoryRepository,
        IBrandRepository brandRepository,
        IProductService productService,
        IProductRepository productRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _subCategoryRepository = subCategoryRepository;
        _brandRepository = brandRepository;
        _productService = productService;
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ProductQueryResource queryResource)
    {
        var query = _mapper.Map<Query<Product>>(queryResource);
        query.AddIncludeProperty(p => p.Brand);
        query.AddIncludeProperty(p => p.SubCategory.Category);

        var products = await _productRepository.GetAsync(query);
        var count = await _productRepository.GetCountAsync(query.Conditions);

        var productResources = _mapper.Map<IEnumerable<ProductResource>>(products).ToList();
        var resource = new ItemsWithCountResource<ProductResource>(count, productResources);
        return Ok(resource);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productRepository
            .GetByIdAsync(id, p =>
                    p.Brand, p => p.SubCategory.Category);

        return product == null ? NotFound() : Ok(_mapper.Map<ProductResource>(product));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromForm] SaveProductResource saveResource)
    {
        if (!await SaveCheckBehavior(saveResource))
            return BadRequest(ModelState);

        var product = _mapper.Map<Product>(saveResource);
        var successfullyAdded = await _productService.AddAsync(product, saveResource.Image!);
        if (!successfullyAdded)
            return InternalServerError();

        try
        {
            await _unitOfWork.CompleteAsync();
            await _productService.IncludeCategoryAsync(product);
            return Ok(_mapper.Map<ProductResource>(product));
        }
        catch (Exception)
        {
            _productService.DeleteImage(product);
            return InternalServerError();
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromForm] SaveProductResource saveResource, int id)
    {
        var productInDb = await _productRepository.GetByIdAsync(id);
        if (productInDb == null)
            return NotFound();

        if (!await SaveCheckBehavior(saveResource))
            return BadRequest(ModelState);

        var productAfterUpdate = new Product(productInDb);
        _mapper.Map(saveResource, productAfterUpdate);

        var haveImage = saveResource.Image != null;
        if (haveImage)
        {
            var imageUpdated = await _productService.UpdateImageAsync(productAfterUpdate, saveResource.Image!);
            if (!imageUpdated)
                return InternalServerError();

            _mapper.Map(productAfterUpdate, productInDb);
        }
        else if (productInDb.Equals(productAfterUpdate))
            return NoContent();
        else
        {
            _mapper.Map(productAfterUpdate, productInDb);
            _productService.UpdateTime(productInDb);
        }

        try
        {
            await _unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            _productService.DeleteImage(productInDb);
            return InternalServerError();
        }

        if (haveImage)
            _productService.DeleteOldImage();

        await _productService.IncludeCategoryAsync(productInDb);
        return Ok(_mapper.Map<ProductResource>(productInDb));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        await _productRepository.DeleteAsync(product);
        await _unitOfWork.CompleteAsync();
        _productService.DeleteImage(product);

        return NoContent();
    }

    private IActionResult InternalServerError()
    {
        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    [NonAction]
    public async Task<bool> SaveCheckBehavior(SaveProductResource saveResource)
    {
        if (await _subCategoryRepository.GetByIdAsync(saveResource.SubCategoryId) is null)
        {
            ModelState.AddModelError(nameof(saveResource.SubCategoryId), "Sub category not found.");
            return false;
        }

        if (await _brandRepository.GetByIdAsync(saveResource.BrandId) is null)
        {
            ModelState.AddModelError(nameof(saveResource.BrandId), "Brand not found.");
            return false;
        }

        if (await _productService.IsUniqueAsync(saveResource.Name, saveResource.Id))
            return true;

        ModelState.AddModelError(nameof(saveResource.Name),
            "Product name duplicated.");
        return false;
    }
}
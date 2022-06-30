using AutoMapper;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.APIs;

[Route("api/categories")]
[ApiController]
public class CategoriesController : Controller
{
    private readonly IMapper _mapper;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICategoryRepository categoryRepository,
        ICategoryService categoryService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _categoryService = categoryService;
    }

    [HttpGet("seed")]
    public async Task<IActionResult> SeedFakeData()
    {
        var categories = new List<Category>
        {
            new()
            {
                Name = "Mobiles & Tab",
                /*SubCategories = new List<SubCategory>
                {
                    new()
                    {
                        Name = "Mobiles"
                    },
                    new()
                    {
                        Name = "Tab"
                    },
                    new()
                    {
                        Name = "Spare parts"
                    }
                }*/
            },
            new()
            {
                Name = "Storage Solution",
                /*SubCategories = new List<SubCategory>
                {
                    new()
                    {
                        Name = "Blank CDs & DVDs"
                    },
                    new()
                    {
                        Name = "External Hard Drives"
                    },
                    new()
                    {
                        Name = "Flash Cards"
                    },
                    new()
                    {
                        Name = "Internal Hard Drives"
                    },
                    new()
                    {
                        Name = "Optical Disk Drives"
                    },
                    new()
                    {
                        Name = "USB Flash"
                    }
                }*/
            },
            new()
            {
                Name = "Toys",
                /*SubCategories = new List<SubCategory>
                {
                    new()
                    {
                        Name = "Fun Toys"
                    },
                    new()
                    {
                        Name = "Pool Accessories"
                    },
                    new()
                    {
                        Name = "Pool Toys"
                    },
                    new()
                    {
                        Name = "Scooters"
                    },
                    new()
                    {
                        Name = "Swimming Pool"
                    }
                }*/
            },
            new()
            {
                Name = "Empty"
            }
        };

        foreach (var category in categories)
            await _categoryRepository.AddAsync(category);

        await _unitOfWork.CompleteAsync();
        return Ok($"{categories.Count} category added.");
    }

    [HttpGet("clear")]
    public async Task<IActionResult> DeleteFakeData()
    {
        var categories = await _categoryRepository.GetAsync();

        foreach (var category in categories)
            await _categoryRepository.DeleteAsync(category);

        await _unitOfWork.CompleteAsync();
        return Ok($"{categories.Count()} category deleted.");
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] CategoryQueryResource queryResource)
    {
        var query = _mapper.Map<Query<Category>>(queryResource);

        var categories = await _categoryRepository.GetAsync(query);
        var count = await _categoryRepository.GetCountAsync(query);

        var categoryResources = _mapper.Map<IEnumerable<CategoryResource>>(categories).ToList();
        var resource = new CategoryWithCountResources(count, categoryResources);
        return Ok(resource);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        return category == null ? NotFound() : Ok(_mapper.Map<CategoryResource>(category));
    }

    [HttpPost]
    public async Task<IActionResult> SaveCategory([FromBody] CategoryResource categoryResource)
    {
        Category category = new();
        if (categoryResource.Id != 0)
        {
            category = await _categoryRepository.GetByIdAsync(categoryResource.Id);
            if (category == null)
                return NotFound();
        }

        if (!await ArePoliciesValid(categoryResource))
            return BadRequest(ModelState);

        _mapper.Map(categoryResource, category);

        if (categoryResource.Id == 0)
            await _categoryRepository.AddAsync(category);

        await _unitOfWork.CompleteAsync();

        return Ok(_mapper.Map(category, categoryResource));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        await _categoryRepository.DeleteAsync(category);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    private async Task<bool> ArePoliciesValid(CategoryResource categoryResource)
    {
        if (await _categoryService.IsUnique(categoryResource.Name, categoryResource.Id))
            return true;

        ModelState.AddModelError(nameof(categoryResource.Name), "Category name duplicated.");
        return false;
    }
}
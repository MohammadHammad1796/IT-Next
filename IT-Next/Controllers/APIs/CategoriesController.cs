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

    [HttpGet("{withTotal:bool?}")]
    public async Task<IActionResult> Get([FromQuery] CategoryQueryResource queryResource, bool withTotal = true)
    {
        var query = _mapper.Map<Query<Category>>(queryResource);

        var categories = await _categoryRepository.GetAsync(query);
        var categoryResources = _mapper
            .Map<IEnumerable<CategoryResource>>(categories).ToList();

        if (!withTotal)
            return Ok(categoryResources);

        var count = await _categoryRepository.GetCountAsync(query.Conditions);
        var resource = new ItemsWithCountResource<CategoryResource>(count, categoryResources);
        return Ok(resource);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        return category == null ? NotFound() : Ok(_mapper.Map<CategoryResource>(category));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] CategoryResource categoryResource)
    {
        Category category = new();
        if (categoryResource.Id != 0)
        {
            category = await _categoryRepository.GetByIdAsync(categoryResource.Id);
            if (category == null)
                return NotFound();
        }

        if (!await _categoryService.IsUnique(categoryResource.Name, categoryResource.Id))
        {
            ModelState.AddModelError(nameof(categoryResource.Name), "Category name duplicated.");
            return BadRequest(ModelState);
        }

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
}
using AutoMapper;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.APIs;

[Route("api/subCategories")]
[ApiController]
public class SubCategoriesController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubCategoryService _subCategoryService;
    private readonly ICategoryRepository _categoryRepository;

    public SubCategoriesController(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ISubCategoryRepository subCategoryRepository,
        ISubCategoryService subCategoryService,
        ICategoryRepository categoryRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _subCategoryRepository = subCategoryRepository;
        _subCategoryService = subCategoryService;
        _categoryRepository = categoryRepository;
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
}
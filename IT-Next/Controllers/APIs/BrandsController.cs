using AutoMapper;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.APIs;

[Route("api/brands")]
[ApiController]
public class BrandsController : Controller
{
    private readonly IMapper _mapper;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBrandService _brandService;

    public BrandsController(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IBrandRepository brandRepository, IBrandService brandService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _brandRepository = brandRepository;
        _brandService = brandService;
    }

    [HttpGet("{withTotal:bool?}")]
    public async Task<IActionResult> Get([FromQuery] BrandQueryResource queryResource, bool withTotal = true)
    {
        var query = _mapper.Map<Query<Brand>>(queryResource);

        var brands = await _brandRepository.GetAsync(query);
        var brandResources = _mapper.Map<IEnumerable<BrandResource>>(brands).ToList();

        if (!withTotal)
            return Ok(brandResources);

        var count = await _brandRepository.GetCountAsync(query.Conditions);
        var resource = new ItemsWithCountResource<BrandResource>(count, brandResources);
        return Ok(resource);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);

        return brand == null ? NotFound() : Ok(_mapper.Map<BrandResource>(brand));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] BrandResource brandResource)
    {
        Brand brand = new();
        if (brandResource.Id != 0)
        {
            brand = await _brandRepository.GetByIdAsync(brandResource.Id);
            if (brand == null)
                return NotFound();
        }

        if (!await _brandService.IsUniqueAsync(brandResource.Name, brandResource.Id))
        {
            ModelState.AddModelError(nameof(brandResource.Name), "Brand name duplicated.");
            return BadRequest(ModelState);
        }

        _mapper.Map(brandResource, brand);

        if (brandResource.Id == 0)
            await _brandRepository.AddAsync(brand);

        await _unitOfWork.CompleteAsync();

        return Ok(_mapper.Map(brand, brandResource));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
            return NotFound();

        await _brandRepository.DeleteAsync(brand);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }
}
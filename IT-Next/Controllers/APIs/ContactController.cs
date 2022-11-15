using AutoMapper;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using IT_Next.Custom.RouteAttributes;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.APIs;

[ApiRoute("contact")]
[ApiController]
public class ContactController : Controller
{
    private readonly IMapper _mapper;
    private readonly IContactMessageRepository _contactMessageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ContactController(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IContactMessageRepository contactMessageRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _contactMessageRepository = contactMessageRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ContactMessageQueryResource queryResource)
    {
        var query = _mapper.Map<Query<ContactMessage>>(queryResource);

        var messages = await _contactMessageRepository.GetAsync(query);

        var count = await _contactMessageRepository.GetCountAsync(query.Conditions);
        var resource = new ItemsWithCountResource<ContactMessage>(count, messages.ToList());
        return Ok(resource);
    }

    [HttpPut("toggleStatus/{id:int}")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var contactMessage = await _contactMessageRepository.GetByIdAsync(id);
        if (contactMessage == null)
            return NotFound();

        contactMessage.IsRead = !contactMessage.IsRead;
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SendContactMessageResource resource)
    {
        var message = _mapper.Map<ContactMessage>(resource);
        message.Time = DateTime.UtcNow;

        await _contactMessageRepository.AddAsync(message);
        await _unitOfWork.CompleteAsync();

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var contactMessage = await _contactMessageRepository.GetByIdAsync(id);
        if (contactMessage == null)
            return NotFound();

        await _contactMessageRepository.DeleteAsync(contactMessage);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }
}
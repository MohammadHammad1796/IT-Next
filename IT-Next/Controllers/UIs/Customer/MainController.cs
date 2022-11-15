using AutoMapper;
using IT_Next.Controllers.UIs.Base;
using IT_Next.Controllers.UIs.ViewModels;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs.Customer;

[Route("")]
public class MainController : ClientUIController
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public MainController(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    [Route("")]
    [Route("home")]
    public async Task<IActionResult> Home()
    {
        var query = new Query<Product>(paging: new Paging(1, 12));
        query.AddOrder(ordering: new Ordering<Product>(p => p.Id, isAscending: false));
        query.AddIncludeProperty(p => p.Brand);
        query.AddIncludeProperty(p => p.SubCategory.Category);

        var latestTwelveProducts = await _productRepository.GetAsync(query);
        var viewModel = _mapper.Map<HomeViewModel>(latestTwelveProducts);
        return View("Home", viewModel);
    }

    [Route("about")]
    public IActionResult About() => View("About");

    [Route("privacy")]
    public IActionResult Privacy() => View("Privacy");

    [HttpGet("contact")]
    public IActionResult Contact() => View("Contact");

    [Route("notFound")]
    public IActionResult NotFoundAction() => NotFound();
}
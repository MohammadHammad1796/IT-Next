using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Services;
using IT_Next.Custom.RouteAttributes;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.APIs;

[ApiRoute("hosting")]
[ApiController]
public class HostingController : Controller
{
    private readonly ICertificateService _certificateService;
    private readonly IStorageService _storageService;

    public HostingController(ICertificateService certificateService, IStorageService storageService)
    {
        _certificateService = certificateService;
        _storageService = storageService;
    }

    [HttpGet("certificateInfo")]
    public async Task<IActionResult> GetCertificateInfo()
    {
        var url = $"{Request.Scheme}://{Request.Host.Value}";
        var expirationTime = await _certificateService.GetExpirationTimeAsync(url);
        if (expirationTime is null)
            return NotFound();
        
        var issueTime = await _certificateService.GetIssueTimeAsync(url);
        if (issueTime is null)
            return NotFound();
        
        return Ok(new CertificateInfoResource(issueTime.Value, expirationTime.Value));
    }

    [HttpGet("storageInfo")]
    public IActionResult GetStorageInfo()
    {
        var allSpace = _storageService.GetAllStorageInMB();
        var usedSpace = _storageService.GetUsedStorageInMB();
        return Ok(new StorageInfoResource(allSpace, usedSpace));
    }
}
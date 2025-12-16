using Microsoft.AspNetCore.Mvc;
using OSInstaller.Models;
using OSInstaller.Services;

namespace OSInstaller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecController : ControllerBase
{
    private readonly SpecLoaderService _loaderService;
    private readonly SpecMergerService _mergerService;
    private readonly ILogger<SpecController> _logger;
    private static UnifiedSpec? _cachedUnifiedSpec;

    public SpecController(
        SpecLoaderService loaderService,
        SpecMergerService mergerService,
        ILogger<SpecController> logger)
    {
        _loaderService = loaderService;
        _mergerService = mergerService;
        _logger = logger;
    }

    [HttpGet("unified")]
    public async Task<ActionResult<UnifiedSpec>> GetUnifiedSpec()
    {
        try
        {
            if (_cachedUnifiedSpec == null)
            {
                var specs = await _loaderService.LoadAllSpecsAsync();
                _cachedUnifiedSpec = _mergerService.MergeSpecs(specs);
            }

            return Ok(_cachedUnifiedSpec);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unified spec");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("diagnostics")]
    public async Task<ActionResult<object>> GetDiagnostics()
    {
        try
        {
            var specs = await _loaderService.LoadAllSpecsAsync();
            var unified = _mergerService.MergeSpecs(specs);

            return Ok(new
            {
                specCount = specs.Count,
                pageCount = unified.Pages.Count,
                diagnostics = unified.Diagnostics,
                contributions = specs.Select(s => new
                {
                    s.ContribId,
                    s.Priority,
                    pageCount = s.Pages.Count,
                    patchCount = s.PagePatches.Count
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get diagnostics");
            return StatusCode(500, new { error = ex.Message, diagnostics = new[] { ex.Message } });
        }
    }

    [HttpPost("reload")]
    public ActionResult Reload()
    {
        _cachedUnifiedSpec = null;
        return Ok(new { message = "Spec cache cleared" });
    }
}

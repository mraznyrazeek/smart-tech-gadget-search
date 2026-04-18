using backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IElasticIndexService _elasticIndexService;

    public SearchController(IElasticIndexService elasticIndexService)
    {
        _elasticIndexService = elasticIndexService;
    }

    [HttpPost("reindex")]
    public async Task<IActionResult> Reindex()
    {
        await _elasticIndexService.ReindexAllProductsAsync();

        return Ok(new { message = "Reindex completed successfully" });
    }
}
using backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("dummyjson")]
    public async Task<IActionResult> ImportFromDummyJson()
    {
        var count = await _importService.ImportProductsFromDummyJsonAsync();

        return Ok(new
        {
            message = "Import completed successfully",
            importedCount = count
        });
    }
}
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _context.Products
            .OrderBy(p => p.Id)
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        return Ok(product);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string? category,
        [FromQuery] string? brand)
    {
        var productsQuery = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var loweredQuery = query.ToLower();

            productsQuery = productsQuery.Where(p =>
                p.Name.ToLower().Contains(loweredQuery) ||
                p.Brand.ToLower().Contains(loweredQuery) ||
                p.Category.ToLower().Contains(loweredQuery) ||
                p.Description.ToLower().Contains(loweredQuery));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var loweredCategory = category.ToLower();
            productsQuery = productsQuery.Where(p => p.Category.ToLower() == loweredCategory);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            var loweredBrand = brand.ToLower();
            productsQuery = productsQuery.Where(p => p.Brand.ToLower() == loweredBrand);
        }

        var results = await productsQuery
            .OrderBy(p => p.Id)
            .ToListAsync();

        return Ok(results);
    }
}
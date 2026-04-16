using System.Text.Json;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ImportService : IImportService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public ImportService(AppDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public async Task<int> ImportProductsFromDummyJsonAsync()
    {
        var url = "https://dummyjson.com/products?limit=100";
        var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<DummyJsonProductsResponseDto>(json, options);

        if (result == null || result.products.Count == 0)
        {
            return 0;
        }

        int importedCount = 0;

        foreach (var raw in result.products)
        {
            var exists = await _context.Products.AnyAsync(p =>
                p.ExternalId == raw.id.ToString() && p.SourceName == "DummyJSON");

            if (exists)
            {
                continue;
            }

            var product = new Product
            {
                ExternalId = raw.id.ToString(),
                Name = raw.title?.Trim() ?? string.Empty,
                Description = raw.description?.Trim() ?? string.Empty,
                Brand = raw.brand?.Trim() ?? "Unknown",
                Category = raw.category?.Trim() ?? "Uncategorized",
                Price = raw.price,
                Rating = raw.rating,
                StockQuantity = raw.stock,
                ThumbnailUrl = raw.thumbnail,
                SourceName = "DummyJSON",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            importedCount++;
        }

        await _context.SaveChangesAsync();
        return importedCount;
    }
}
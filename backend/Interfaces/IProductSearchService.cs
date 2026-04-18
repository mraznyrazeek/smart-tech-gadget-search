using backend.DTOs;

namespace backend.Interfaces;

public interface IProductSearchService
{
    Task<List<ProductSearchDocument>> SearchAsync(string? query, string? category, string? brand);
}
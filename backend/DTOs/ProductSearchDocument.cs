namespace backend.DTOs;

public class ProductSearchDocument
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Rating { get; set; }
    public int StockQuantity { get; set; }
    public string? ThumbnailUrl { get; set; }
}
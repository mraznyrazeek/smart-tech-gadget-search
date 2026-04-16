namespace backend.DTOs;

public class RawProductImportDto
{
    public int id { get; set; }
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string category { get; set; } = string.Empty;
    public decimal price { get; set; }
    public decimal rating { get; set; }
    public int stock { get; set; }
    public string brand { get; set; } = string.Empty;
    public string thumbnail { get; set; } = string.Empty;
}
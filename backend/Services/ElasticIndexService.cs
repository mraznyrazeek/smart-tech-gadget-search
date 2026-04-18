using backend.Data;
using backend.DTOs;
using backend.Interfaces;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ElasticIndexService : IElasticIndexService
{
    private readonly AppDbContext _context;
    private readonly ElasticsearchClient _elasticClient;

    public ElasticIndexService(AppDbContext context, ElasticsearchClient elasticClient)
    {
        _context = context;
        _elasticClient = elasticClient;
    }

    public async Task IndexProductAsync(int productId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return;

        var doc = new ProductSearchDocument
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            Category = product.Category,
            Price = product.Price,
            Rating = product.Rating
        };

        await _elasticClient.IndexAsync(doc, idx => idx
            .Index("products")
            .Id(product.Id));
    }

    public async Task ReindexAllProductsAsync()
    {
        var products = await _context.Products.ToListAsync();

        foreach (var product in products)
        {
            var doc = new ProductSearchDocument
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Rating = product.Rating,

                StockQuantity = product.StockQuantity,
                ThumbnailUrl = product.ThumbnailUrl
            };

            await _elasticClient.IndexAsync(doc, idx => idx
                .Index("products")
                .Id(product.Id));
        }
    }

    public async Task DeleteProductAsync(int productId)
    {
        await _elasticClient.DeleteAsync<ProductSearchDocument>(productId, d => d.Index("products"));
    }
}
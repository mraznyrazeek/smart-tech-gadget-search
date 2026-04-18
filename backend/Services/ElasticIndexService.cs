using backend.Data;
using backend.DTOs;
using backend.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.Analysis;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ElasticIndexService : IElasticIndexService
{
    private readonly AppDbContext _context;
    private readonly ElasticsearchClient _elasticClient;
    private const string IndexName = "products";

    public ElasticIndexService(AppDbContext context, ElasticsearchClient elasticClient)
    {
        _context = context;
        _elasticClient = elasticClient;
    }

    public async Task IndexProductAsync(int productId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return;

        var existsResponse = await _elasticClient.Indices.ExistsAsync(IndexName);
        if (!existsResponse.Exists)
        {
            await CreateIndexAsync();
        }

        var doc = MapToSearchDocument(product);

        await _elasticClient.IndexAsync(doc, idx => idx
            .Index(IndexName)
            .Id(product.Id));
    }

    public async Task ReindexAllProductsAsync()
    {
        var existsResponse = await _elasticClient.Indices.ExistsAsync(IndexName);

        if (existsResponse.Exists)
        {
            await _elasticClient.Indices.DeleteAsync(IndexName);
        }

        await CreateIndexAsync();

        var products = await _context.Products.ToListAsync();

        foreach (var product in products)
        {
            var doc = MapToSearchDocument(product);

            await _elasticClient.IndexAsync(doc, idx => idx
                .Index(IndexName)
                .Id(product.Id));
        }

        await _elasticClient.Indices.RefreshAsync(IndexName);
    }

    public async Task DeleteProductAsync(int productId)
    {
        await _elasticClient.DeleteAsync<ProductSearchDocument>(productId, d => d.Index(IndexName));
    }

    private static ProductSearchDocument MapToSearchDocument(backend.Entities.Product product)
    {
        return new ProductSearchDocument
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
    }

    private async Task CreateIndexAsync()
    {
        var createIndexResponse = await _elasticClient.Indices.CreateAsync(IndexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Tokenizers(t => t
                        .EdgeNGram("edge_ngram_tokenizer", eg => eg
                            .MinGram(1)
                            .MaxGram(20)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit)
                        )
                    )
                    .Analyzers(an => an
                        .Custom("edge_ngram_analyzer", ca => ca
                            .Tokenizer("edge_ngram_tokenizer")
                            .Filter(new[] { "lowercase" })
                        )
                        .Custom("search_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase" })
                        )
                    )
                )
            )
            .Mappings(m => m
                .Properties(new Properties
                {
                    {
                        "id",
                        new IntegerNumberProperty()
                    },
                    {
                        "name",
                        new TextProperty
                        {
                            Analyzer = "edge_ngram_analyzer",
                            SearchAnalyzer = "search_analyzer",
                            Fields = new Properties
                            {
                                {
                                    "keyword",
                                    new KeywordProperty()
                                }
                            }
                        }
                    },
                    {
                        "description",
                        new TextProperty
                        {
                            Analyzer = "search_analyzer"
                        }
                    },
                    {
                        "brand",
                        new TextProperty
                        {
                            Analyzer = "edge_ngram_analyzer",
                            SearchAnalyzer = "search_analyzer",
                            Fields = new Properties
                            {
                                {
                                    "keyword",
                                    new KeywordProperty()
                                }
                            }
                        }
                    },
                    {
                        "category",
                        new TextProperty
                        {
                            Analyzer = "edge_ngram_analyzer",
                            SearchAnalyzer = "search_analyzer",
                            Fields = new Properties
                            {
                                {
                                    "keyword",
                                    new KeywordProperty()
                                }
                            }
                        }
                    },
                    {
                        "price",
                        new DoubleNumberProperty()
                    },
                    {
                        "rating",
                        new DoubleNumberProperty()
                    },
                    {
                        "stockQuantity",
                        new IntegerNumberProperty()
                    },
                    {
                        "thumbnailUrl",
                        new KeywordProperty()
                    }
                })
            )
        );

        if (!createIndexResponse.IsValidResponse)
        {
            throw new Exception("Failed to create Elasticsearch index.");
        }
    }
}
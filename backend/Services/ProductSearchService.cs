using backend.DTOs;
using backend.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace backend.Services;

public class ProductSearchService : IProductSearchService
{
    private readonly ElasticsearchClient _elasticClient;

    public ProductSearchService(ElasticsearchClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public async Task<List<ProductSearchDocument>> SearchAsync(string? query, string? category, string? brand)
    {
        var filterQueries = new List<Query>();

        if (!string.IsNullOrWhiteSpace(category))
        {
            filterQueries.Add(new TermQuery
            {
                Field = "category.keyword",
                Value = category
            });
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            filterQueries.Add(new TermQuery
            {
                Field = "brand.keyword",
                Value = brand
            });
        }

        Query finalQuery;

        if (string.IsNullOrWhiteSpace(query))
        {
            finalQuery = new BoolQuery
            {
                Filter = filterQueries
            };
        }
        else
        {
            finalQuery = new BoolQuery
            {
                Should = new List<Query>
                {
                    new MultiMatchQuery
                    {
                        Query = query,
                        Fields = new[] { "name", "description", "brand", "category" },
                        Fuzziness = "AUTO"
                    },
                    new MultiMatchQuery
                    {
                        Query = query,
                        Fields = new[] { "name", "brand", "category" },
                        Type = TextQueryType.BoolPrefix
                    }
                },
                MinimumShouldMatch = 1,
                Filter = filterQueries
            };
        }

        var request = new SearchRequest("products")
        {
            Query = finalQuery
        };

        var response = await _elasticClient.SearchAsync<ProductSearchDocument>(request);

        if (!response.IsValidResponse)
        {
            throw new Exception(
                $"Elasticsearch search failed: {response.ElasticsearchServerError?.Error?.Reason ?? "Unknown error"}");
        }

        return response.Documents.ToList();
    }
}
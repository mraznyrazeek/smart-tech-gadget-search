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
        var mustQueries = new List<Query>();
        var filterQueries = new List<Query>();

        if (!string.IsNullOrWhiteSpace(query))
        {
            //mustQueries.Add(new MultiMatchQuery
            //{
            //    Query = query,
            //    Fields = new[] { "name", "description", "brand", "category" },
            //    Fuzziness = "AUTO"
            //});

            if (!string.IsNullOrWhiteSpace(query))
            {
                mustQueries.Add(new BoolQuery
                {
                    Should = new List<Query>
        {
            new MultiMatchQuery
            {
                Query = query,
                Fields = new[]
                {
                    "name^3",
                    "brand^2",
                    "category",
                    "description"
                },
                Fuzziness = "AUTO"
            },

            new MultiMatchQuery
            {
                Query = query,
                Fields = new[] { "name", "brand" },
                Type = TextQueryType.BoolPrefix
            }
        },

                    MinimumShouldMatch = 1
                });
            }

        }

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

        if (mustQueries.Count == 0 && filterQueries.Count == 0)
        {
            finalQuery = new MatchAllQuery();
        }
        else
        {
            finalQuery = new BoolQuery
            {
                Must = mustQueries,
                Filter = filterQueries
            };
        }

        var request = new SearchRequest("products")
        {
            Query = finalQuery,
            Size = 50
        };

        var response = await _elasticClient.SearchAsync<ProductSearchDocument>(request);

        if (!response.IsValidResponse)
        {
            throw new Exception("Elasticsearch search failed.");
        }

        return response.Documents.ToList();
    }
}
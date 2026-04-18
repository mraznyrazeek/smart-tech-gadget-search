namespace backend.Interfaces;

public interface IElasticIndexService
{
    Task IndexProductAsync(int productId);
    Task ReindexAllProductsAsync();
    Task DeleteProductAsync(int productId);
}
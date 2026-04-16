namespace backend.Interfaces;

public interface IImportService
{
    Task<int> ImportProductsFromDummyJsonAsync();
}
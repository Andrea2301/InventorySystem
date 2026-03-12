using InventorySystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<IEnumerable<Product>> GetLowStockAsync(int threshold);
        Task AddRangeAsync(List<Product> productsToImport);
    }
}

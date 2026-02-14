using InventorySystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public interface ISaleService
    {
        Task<IEnumerable<Sale>> GetAllSalesAsync(string? searchText = null);
        Task<Sale?> GetSaleByIdAsync(int id);
        Task CreateSaleAsync(Sale sale);
    }
}

using InventorySystem.Models;
using System.Threading.Tasks;

namespace InventorySystem.Services.Export
{
    public interface IPdfService
    {
        Task GenerateInvoiceAsync(Sale sale, string filePath);
    }
}

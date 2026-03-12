using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold)
        {
            return await _context.Products
                .Where(p => p.Quantity < threshold && p.IsActive)
                .OrderBy(p => p.Quantity)
                .ToListAsync();
        }

        public async Task AddRangeAsync(List<Product> productsToImport)
        {
            await _context.Products.AddRangeAsync(productsToImport);
            await _context.SaveChangesAsync();
        }
    }
}

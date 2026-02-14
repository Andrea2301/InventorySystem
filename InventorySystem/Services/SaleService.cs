using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public class SaleService : ISaleService
    {
        private readonly AppDbContext _context;

        public SaleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sale>> GetAllSalesAsync(string? searchText = null)
        {
            var query = _context.Sales
                .Include(s => s.Client)
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string searchLower = searchText.ToLower();
                if (int.TryParse(searchText, out int id))
                {
                    query = query.Where(s => s.Id == id || 
                        (s.Client != null && (s.Client.FirstName.ToLower().Contains(searchLower) || s.Client.LastName.ToLower().Contains(searchLower))));
                }
                else
                {
                    query = query.Where(s => s.Client != null && (s.Client.FirstName.ToLower().Contains(searchLower) || s.Client.LastName.ToLower().Contains(searchLower)));
                }
            }

            return await query.AsNoTracking().OrderByDescending(s => s.SaleDate).ToListAsync();
        }

        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            return await _context.Sales
                .Include(s => s.Client)
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task CreateSaleAsync(Sale sale)
        {
            // Ensure we have a valid client if one wasn't provided (as a failsafe for the current POS state)
            if (sale.ClientId <= 0)
            {
                var defaultClient = await _context.Clients.FirstOrDefaultAsync();
                if (defaultClient == null)
                {
                    // If no clients exist, create a generic one
                    defaultClient = new Client { FirstName = "Consumidor", LastName = "Final", DocumentNumber = "00000000" };
                    _context.Clients.Add(defaultClient);
                    await _context.SaveChangesAsync();
                }
                sale.ClientId = defaultClient.Id;
            }

            // Start transaction or use same context to ensure atomicity
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.Sales.AddAsync(sale);
                    
                    // Update Stock for each product
                    foreach (var detail in sale.SaleDetails)
                    {
                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            if (product.Quantity < detail.Quantity)
                            {
                                throw new Exception($"Stock insuficiente para el producto: {product.Name}");
                            }
                            product.Quantity -= detail.Quantity;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}

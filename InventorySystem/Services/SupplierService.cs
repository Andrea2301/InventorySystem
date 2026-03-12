using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly AppDbContext _context;

        public SupplierService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllAsync();

            string searchLower = searchText.ToLower();

            return await _context.Suppliers
                .Where(s => s.CompanyName.ToLower().Contains(searchLower) ||
                            s.FirstName.ToLower().Contains(searchLower) ||
                            s.LastName.ToLower().Contains(searchLower) ||
                            (s.Category != null && s.Category.ToLower().Contains(searchLower)))
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task AddAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            var existing = await _context.Suppliers.FindAsync(supplier.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(supplier);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(List<Supplier> suppliersToImport)
        {
            await _context.Suppliers.AddRangeAsync(suppliersToImport);
            await _context.SaveChangesAsync();
        }
    }
}

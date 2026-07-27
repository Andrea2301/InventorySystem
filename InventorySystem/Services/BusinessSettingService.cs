using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public class BusinessSettingService : IBusinessSettingService
    {
        private readonly AppDbContext _context;

        public BusinessSettingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BusinessSetting> GetSettingsAsync()
        {
            var settings = await _context.BusinessSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                // Create a default instance if none exists
                settings = new BusinessSetting
                {
                    CompanyName = "INVENTORY SYSTEM",
                    TaxId = "RUT: 12345678-9",
                    Address = "Calle Ficticia 123",
                    Phone = "+57 300 123 4567",
                    Email = "contacto@misistema.com",
                    TaxPercentage = 19.00m,
                    CurrencySymbol = "$"
                };
                await _context.BusinessSettings.AddAsync(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        public async Task<bool> SaveSettingsAsync(BusinessSetting settings)
        {
            var existing = await _context.BusinessSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                await _context.BusinessSettings.AddAsync(settings);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(settings);
            }
            
            int affected = await _context.SaveChangesAsync();
            return affected > 0;
        }
    }
}

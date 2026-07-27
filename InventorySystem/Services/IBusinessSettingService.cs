using InventorySystem.Models;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public interface IBusinessSettingService
    {
        Task<BusinessSetting> GetSettingsAsync();
        Task<bool> SaveSettingsAsync(BusinessSetting settings);
    }
}

using System;

namespace InventorySystem.ViewModel.Base
{
    public interface ICloseableViewModel
    {
        event EventHandler RequestClose;
    }
}

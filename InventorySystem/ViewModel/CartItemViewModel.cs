using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using System;

namespace InventorySystem.ViewModel
{
    public class CartItemViewModel : ViewModelBase
    {
        private int _quantity;
        public Product Product { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value > 0)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        public decimal Subtotal => Product.Price * Quantity;

        public CartItemViewModel(Product product)
        {
            Product = product;
            Quantity = 1;
        }
    }
}

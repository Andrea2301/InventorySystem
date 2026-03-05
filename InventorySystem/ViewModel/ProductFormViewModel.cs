using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using InventorySystem.Services;

namespace InventorySystem.ViewModel
{
    public class ProductFormViewModel : ViewModelBase, ICloseableViewModel
    {
        private readonly IProductService _productService;
        private Product _originalProduct;
        private int _id;
        private string _name;
        private string _category;
        private decimal _price;
        private int _quantity;
        private string _description;
        private DateTime _createdAt;
        private string _imagePath;
        private byte[] _imageData;

        private BitmapImage _previewImage;
        private string _formTitle;

        public string FormTitle { get => _formTitle; set { _formTitle = value; OnPropertyChanged(nameof(FormTitle)); } }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); ValidateProperty(nameof(Name), value); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); ValidateProperty(nameof(Category), value); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(nameof(Price)); ValidateProperty(nameof(Price), value); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(nameof(Quantity)); ValidateProperty(nameof(Quantity), value); }
        }

        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }

        public BitmapImage PreviewImage { get => _previewImage; set { _previewImage = value; OnPropertyChanged(nameof(PreviewImage)); } }

        public ICommand SelectImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler RequestClose;

        public ProductFormViewModel(IProductService productService, Product? product = null)
        {
            _productService = productService;
            _originalProduct = product;

            if (product == null)
            {
                FormTitle = "New Product";
                _category = "General";
                _isActive = true;
                _createdAt = DateTime.UtcNow;
            }
            else
            {
                _id = product.Id;
                _name = product.Name;
                _category = product.Category;
                _price = product.Price;
                _quantity = product.Quantity;
                _description = product.Description;
                _isActive = product.IsActive;
                _createdAt = product.CreatedAt;
                _imagePath = product.ImagePath;
                _imageData = product.ImageData;
                
                FormTitle = "Edit Product";
                LoadPreviewImage();
            }

            SelectImageCommand = new ViewModelCommand(ExecuteSelectImageCommand);
            SaveCommand = new ViewModelCommand(ExecuteSaveCommand);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);
            
            // Initial validation for Edit mode
            if (product != null)
            {
                ValidateAll();
            }
        }

        private bool _isActive;
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }

        private void LoadPreviewImage()
        {
            if (!string.IsNullOrEmpty(_imagePath))
            {
                try
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _imagePath);
                    if (File.Exists(fullPath))
                    {
                        PreviewImage = new BitmapImage(new Uri(fullPath));
                    }
                }
                catch { }
            }
        }

        private void ValidateProperty(string propertyName, object value)
        {
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(Name):
                    if (string.IsNullOrWhiteSpace(value?.ToString()))
                        AddError(propertyName, "Name is required.");
                    break;
                case nameof(Category):
                    if (string.IsNullOrWhiteSpace(value?.ToString()))
                        AddError(propertyName, "Category is required.");
                    break;
                case nameof(Price):
                    if (value is decimal price && price <= 0)
                        AddError(propertyName, "Price must be greater than 0.");
                    break;
                case nameof(Quantity):
                    if (value is int qty && qty < 0)
                        AddError(propertyName, "Quantity cannot be negative.");
                    break;
            }
        }

        private void ValidateAll()
        {
            ValidateProperty(nameof(Name), Name);
            ValidateProperty(nameof(Category), Category);
            ValidateProperty(nameof(Price), Price);
            ValidateProperty(nameof(Quantity), Quantity);
        }

        private void ExecuteSelectImageCommand(object obj)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg" };
            if (openFileDialog.ShowDialog() == true)
            {
                _imagePath = openFileDialog.FileName;
                PreviewImage = new BitmapImage(new Uri(_imagePath));
            }
        }

        private async void ExecuteSaveCommand(object obj)
        {
            ValidateAll();
            if (HasErrors) return;

            try
            {
                var product = new Product
                {
                    Id = _id,
                    Name = Name,
                    Category = Category,
                    Price = Price,
                    Quantity = Quantity,
                    Description = Description,
                    IsActive = IsActive,
                    CreatedAt = _createdAt,
                    ImagePath = _imagePath,
                    ImageData = _imageData
                };

                if (product.Id == 0) await _productService.AddAsync(product);
                else await _productService.UpdateAsync(product);

                MessageBox.Show("Product Saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelCommand(object obj) => RequestClose?.Invoke(this, EventArgs.Empty);
    }
}

using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace InventorySystem.ViewModel
{
    public class ProductFormViewModel : ViewModelBase
    {
        private Product _product;
        private string _selectedImagePath;
        private BitmapImage _previewImage;
        private string _formTitle;

        public string FormTitle
        {
            get => _formTitle;
            set
            {
                _formTitle = value;
                OnPropertyChanged(nameof(FormTitle));
            }
        }

        public Product Product
        {
            get { return _product; }
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
            }
        }

        public BitmapImage PreviewImage
        {
            get { return _previewImage; }
            set
            {
                _previewImage = value;
                OnPropertyChanged(nameof(PreviewImage));
            }
        }

        public ICommand SelectImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler RequestClose;

        public ProductFormViewModel(Product? product = null)
        {
            if (product == null)
            {
                Product = new Product();
                FormTitle = "New Product";
            }
            else
            {
                // Create a shallow copy to avoid modifying the original list item before saving
                Product = new Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    Description = product.Description,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt,
                    ImagePath = product.ImagePath,
                    ImageData = product.ImageData
                };
                FormTitle = "Edit Product";
                
                // Set preview image if path exists
                if (!string.IsNullOrEmpty(Product.ImagePath))
                {
                    try
                    {
                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Product.ImagePath);
                        if (File.Exists(fullPath))
                        {
                            PreviewImage = new BitmapImage(new Uri(fullPath));
                        }
                    }
                    catch { /* Ignore image loading errors in preview */ }
                }
            }

            SelectImageCommand = new ViewModelCommand(ExecuteSelectImageCommand);
            SaveCommand = new ViewModelCommand(ExecuteSaveCommand);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);
        }

        private void ExecuteSelectImageCommand(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePath = openFileDialog.FileName;
                
                // Show preview
                PreviewImage = new BitmapImage(new Uri(_selectedImagePath));
            }
        }

        private void ExecuteSaveCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(Product.Name))
            {
                MessageBox.Show("Please enter a product name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Product.Category))
            {
                MessageBox.Show("Please enter a category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Product.Price <= 0)
            {
                MessageBox.Show("Price must be greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Product.Quantity < 0)
            {
                MessageBox.Show("Quantity cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Image Handling Logic - COMMENTED OUT FOR NOW AS REQUESTED
            /*
            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                try
                {
                    // 1. Define destination folder (Assets/Products)
                    string projectPath = AppDomain.CurrentDomain.BaseDirectory;
                    string assetsPath = Path.Combine(projectPath, "Assets", "Products");

                    // Create directory if not exists
                    if (!Directory.Exists(assetsPath))
                    {
                        Directory.CreateDirectory(assetsPath);
                    }

                    // 2. Generate unique filename
                    string extension = Path.GetExtension(_selectedImagePath);
                    string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    string destinationPath = Path.Combine(assetsPath, uniqueFileName);

                    // 3. Copy file
                    File.Copy(_selectedImagePath, destinationPath, true);

                    // 4. Save RELATIVE path to database
                    Product.ImagePath = Path.Combine("Assets", "Products", uniqueFileName);
                    
                    // Optional: If you still need byte[] for legacy reasons, you can keep it or remove it.
                    // Product.ImageData = File.ReadAllBytes(destinationPath); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                    return;
                }
            }
            */

            try
            {
                using (var context = new Data.AppDbContext())
                {
                    if (Product.Id == 0)
                    {
                        context.Products.Add(Product);
                    }
                    else
                    {
                        context.Products.Update(Product);
                    }
                    context.SaveChanges();
                }

                MessageBox.Show($"Product Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelCommand(object obj)
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using InventorySystem.Models;
using InventorySystem.Services;
using InventorySystem.ViewModel.Base;
using InventorySystem.Views;

namespace InventorySystem.ViewModel
{
    public class SupplierViewModel : ViewModelBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private ObservableCollection<Supplier> _suppliers;
        private string _searchText;

        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set
            {
                _suppliers = value;
                OnPropertyChanged(nameof(Suppliers));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _ = SearchSuppliersAsync();
            }
        }

        public ICommand ShowAddSupplierCommand { get; }
        public ICommand ShowEditSupplierCommand { get; }
        public ICommand DeleteSupplierCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand OpenImportExcelCommand { get; }

        public SupplierViewModel(ISupplierService supplierService, IDialogService dialogService, IMessageService messageService)
        {
            _supplierService = supplierService;
            _dialogService = dialogService;
            _messageService = messageService;

            ShowAddSupplierCommand = new ViewModelCommand(ExecuteShowAddSupplierCommand);
            ShowEditSupplierCommand = new ViewModelCommand(ExecuteShowEditSupplierCommand);
            DeleteSupplierCommand = new ViewModelCommand(ExecuteDeleteSupplierCommand);
            SearchCommand = new ViewModelCommand(async _ => await SearchSuppliersAsync());
            OpenImportExcelCommand = new ViewModelCommand(ExecuteOpenImportExcelCommand);

            _ = LoadSuppliersAsync();
        }

        private void ExecuteOpenImportExcelCommand(object obj)
        {
            var viewModel = new ImportFileViewModel();
            var view = new ImportFileView { DataContext = viewModel };

            if (view.ShowDialog() == true)
            {
                _ = ImportSuppliers(viewModel.SelectedFilePath);
            }
        }

        private async Task ImportSuppliers(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _messageService.ShowWarning("No file selected.");
                    return;
                }

                IsLoading = true;
                var rows = MiniExcelLibs.MiniExcel.Query(filePath, useHeaderRow: true)
                            .Cast<IDictionary<string, object>>()
                            .ToList();

                if (rows == null || rows.Count == 0)
                {
                    _messageService.ShowWarning("The file is empty or could not be read.");
                    return;
                }

                var suppliersToImport = new List<Supplier>();
                foreach (var row in rows)
                {
                    string GetValue(params string[] possibleAliases)
                    {
                        foreach (var alias in possibleAliases)
                        {
                            var matchingKey = row.Keys.FirstOrDefault(k => k.Trim().Equals(alias, StringComparison.OrdinalIgnoreCase));
                            if (matchingKey != null) return row[matchingKey]?.ToString() ?? "";
                        }
                        return "";
                    }

                    var companyName = GetValue("Company", "CompanyName", "Empresa", "Proveedor");
                    if (string.IsNullOrWhiteSpace(companyName)) continue;

                    suppliersToImport.Add(new Supplier
                    {
                        CompanyName = companyName,
                        FirstName = GetValue("ContactName", "Contact", "FirstName", "Nombre"),
                        LastName = GetValue("LastName", "Surname", "Apellido"),
                        Email = GetValue("Email", "Correo", "E-mail"),
                        PhoneNumber = GetValue("Phone", "PhoneNumber", "Telefono", "Teléfono", "Celular"),
                        Category = GetValue("Category", "Categoría", "Rubro"),
                        DocumentNumber = GetValue("Document", "DNI", "Cedula", "Cédula", "Documento", "IdNumber") ?? "0",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                }

                if (suppliersToImport.Count > 0)
                {
                    await _supplierService.AddRangeAsync(suppliersToImport);
                    await LoadSuppliersAsync();
                    _messageService.ShowInfo($"{suppliersToImport.Count} suppliers imported successfully.");
                }
                else
                {
                    _messageService.ShowWarning("No valid data found in the file to import.");
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error importing file: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSuppliersAsync()
        {
            try
            {
                IsLoading = true;
                var list = await _supplierService.GetAllAsync();
                Suppliers = new ObservableCollection<Supplier>(list);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error loading suppliers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchSuppliersAsync()
        {
            try
            {
                IsLoading = true;
                var list = await _supplierService.SearchAsync(SearchText);
                Suppliers = new ObservableCollection<Supplier>(list);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error searching suppliers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteShowAddSupplierCommand(object obj)
        {
            var viewModel = new SupplierFormViewModel(_supplierService);
            viewModel.RequestClose += (s, e) => _ = LoadSuppliersAsync();
            _dialogService.ShowDialog(viewModel);
        }

        private void ExecuteShowEditSupplierCommand(object obj)
        {
            if (obj is Supplier supplier)
            {
                var viewModel = new SupplierFormViewModel(_supplierService, supplier);
                viewModel.RequestClose += (s, e) => _ = LoadSuppliersAsync();
                _dialogService.ShowDialog(viewModel);
            }
        }

        private async void ExecuteDeleteSupplierCommand(object obj)
        {
            if (obj is Supplier supplier)
            {
                if (_messageService.ShowConfirmation($"Are you sure you want to delete {supplier.CompanyName}?", "Confirm Delete"))
                {
                    try
                    {
                        IsLoading = true;
                        await _supplierService.DeleteAsync(supplier.Id);
                        await LoadSuppliersAsync();
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError($"Error deleting supplier: {ex.Message}");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }
    }
}

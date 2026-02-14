using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using InventorySystem.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    class ClientViewModel : ViewModelBase
    {
        private ObservableCollection<Client> _clients;
        private ICollectionView _clientsView;
        private string _searchText;
        private Client _selectedClient;

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(nameof(Clients)); }
        }

        public ICollectionView ClientsView
        {
            get => _clientsView;
            set { _clientsView = value; OnPropertyChanged(nameof(ClientsView)); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ClientsView.Refresh();
            }
        }

        public Client SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(nameof(SelectedClient)); }
        }

        public ICommand OpenClientFormCommand { get; }
        public ICommand EditClientCommand { get; }
        public ICommand DeleteClientCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand OpenImportExcelCommand { get; }

        public ClientViewModel()
        {
            Clients = new ObservableCollection<Client>();
            ClientsView = CollectionViewSource.GetDefaultView(Clients);
            ClientsView.Filter = FilterClients;

            OpenClientFormCommand = new ViewModelCommand(ExecuteOpenClientFormCommand);
            EditClientCommand = new ViewModelCommand(ExecuteEditClientCommand, CanEditOrDelete);
            DeleteClientCommand = new ViewModelCommand(ExecuteDeleteClientCommand, CanEditOrDelete);
            SortCommand = new ViewModelCommand(ExecuteSortCommand);
            OpenImportExcelCommand = new ViewModelCommand(ExecuteOpenImportExcelCommand);

            LoadClients();
        }

        private bool FilterClients(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            if (obj is Client client)
            {
                return client.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       client.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       client.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private void LoadClients()
        {
            using (var db = new AppDbContext())
            {
                var list = db.Clients.ToList();
                Clients.Clear();
                foreach (var client in list)
                {
                    Clients.Add(client);
                }
            }
        }

        private bool CanEditOrDelete(object obj) => obj is Client || SelectedClient != null;

        private void ExecuteSortCommand(object obj)
        {
            var sortDescription = ClientsView.SortDescriptions.FirstOrDefault(sd => sd.PropertyName == "FirstName");
            ListSortDirection direction = ListSortDirection.Ascending;

            if (sortDescription != default && sortDescription.Direction == ListSortDirection.Ascending)
            {
                direction = ListSortDirection.Descending;
            }

            ClientsView.SortDescriptions.Clear();
            ClientsView.SortDescriptions.Add(new SortDescription("FirstName", direction));
        }

        private void ExecuteOpenClientFormCommand(object obj)
        {
            var viewModel = new ClientFormViewModel();
            var view = new ClientFormView { DataContext = viewModel };
            
            if (view.ShowDialog() == true)
            {
                LoadClients(); // Simple refresh for now
            }
        }

        private void ExecuteEditClientCommand(object obj)
        {
            var clientToEdit = obj as Client ?? SelectedClient;
            if (clientToEdit == null) return;

            var viewModel = new ClientFormViewModel(clientToEdit);
            var view = new ClientFormView { DataContext = viewModel };

            if (view.ShowDialog() == true)
            {
                LoadClients();
            }
        }

        private void ExecuteDeleteClientCommand(object obj)
        {
            var clientToDelete = obj as Client ?? SelectedClient;
            if (clientToDelete == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {clientToDelete.FullName}?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (var db = new AppDbContext())
                {
                    db.Clients.Remove(clientToDelete);
                    db.SaveChanges();
                }
                LoadClients();
            }
        }

        private void ExecuteOpenImportExcelCommand(object obj)
        {
            var viewModel = new ImportFileViewModel();
            var view = new ImportFileView { DataContext = viewModel };
            
            if (view.ShowDialog() == true)
            {
                try
                {
                    string filePath = viewModel.SelectedFilePath;
                    if (string.IsNullOrEmpty(filePath)) 
                    {
                        MessageBox.Show("No file selected.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Use explicit useHeaderRow: true to ensure MiniExcel uses the first row for keys
                    var rows = MiniExcelLibs.MiniExcel.Query(filePath, useHeaderRow: true)
                                .Cast<IDictionary<string, object>>()
                                .ToList();
                    
                    if (rows == null || rows.Count == 0)
                    {
                        MessageBox.Show("The file is empty or could not be read. Please ensure it has a header row.", "Empty File", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    using (var db = new AppDbContext())
                    {
                        int successCount = 0;
                        foreach (var row in rows)
                        {
                            // Robust mapping (case-insensitive and handling potential spaces)
                            var keys = row.Keys.Select(k => k.Trim()).ToList();
                            
                            string GetValue(params string[] possibleAliases)
                            {
                                foreach (var alias in possibleAliases)
                                {
                                    var matchingKey = row.Keys.FirstOrDefault(k => k.Trim().Equals(alias, StringComparison.OrdinalIgnoreCase));
                                    if (matchingKey != null) return row[matchingKey]?.ToString() ?? "";
                                }
                                return "";
                            }

                            var firstName = GetValue("FirstName", "First Name", "Nombre");
                            var lastName = GetValue("LastName", "Last Name", "Apellido");

                            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName)) continue;

                            db.Clients.Add(new Client
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                Email = GetValue("Email", "Correo", "E-mail"),
                                PhoneNumber = GetValue("Phone", "PhoneNumber", "Telefono", "Teléfono", "Celular"),
                                DocumentNumber = GetValue("Document", "DNI", "Cedula", "Cédula", "Documento", "IdNumber"),
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            });
                            successCount++;
                        }
                        
                        if (successCount > 0)
                        {
                            db.SaveChanges();
                            LoadClients();
                            MessageBox.Show($"{successCount} clients imported successfully.", "Import Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No valid data found in the file to import.", "Import Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing file: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

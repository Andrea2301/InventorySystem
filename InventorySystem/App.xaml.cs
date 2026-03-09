using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using InventorySystem.Data;
using InventorySystem.Services;
using InventorySystem.ViewModel;
using InventorySystem.Shell;
using System;
using InventorySystem.Services.Export;

namespace InventorySystem
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Initialize Database
            using (var scope = ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    context.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error initializing database: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Show Splash Screen first
            var splashScreen = new InventorySystem.Shell.SplashScreen();
            splashScreen.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Data
            services.AddDbContext<AppDbContext>();

            // Services
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ISeedDataService, SeedDataService>();
            services.AddScoped<IDialogService, DialogService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<ProductFormViewModel>();
            services.AddTransient<ClientViewModel>();
            services.AddTransient<ClientFormViewModel>();
            services.AddTransient<SalesHistoryViewModel>();
            services.AddTransient<SaleViewModel>();
            services.AddTransient<SupplierViewModel>();
            services.AddTransient<SupplierFormViewModel>();
            services.AddTransient<MaintenanceViewModel>();
            services.AddTransient<ReportsViewModel>();

            // Views
            services.AddTransient<MainWindow>();
        }
    }
}

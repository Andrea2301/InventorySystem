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

            // Initialize Database & Ensure Default Admin
            using (var scope = ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    context.Database.EnsureCreated();
                    
                    // Initialize Default Admin User
                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    authService.EnsureDefaultAdminAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error initializing database or admin user: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Show Login Window first
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
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
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

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
            services.AddTransient<LoginViewModel>();

            // Views / Windows
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<Shell.SplashScreen>();
        }
    }
}

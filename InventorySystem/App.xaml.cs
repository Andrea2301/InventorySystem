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
        public static string CurrentLanguage { get; private set; } = "es";

        public static void ChangeLanguage(string cultureCode)
        {
            var dict = new ResourceDictionary();
            switch (cultureCode.ToLower())
            {
                case "en":
                    dict.Source = new Uri("Assets/Languages/Strings.en.xaml", UriKind.Relative);
                    CurrentLanguage = "en";
                    break;
                case "es":
                default:
                    dict.Source = new Uri("Assets/Languages/Strings.es.xaml", UriKind.Relative);
                    CurrentLanguage = "es";
                    break;
            }

            var mergedDicts = Current.Resources.MergedDictionaries;
            ResourceDictionary? existingLangDict = null;

            foreach (var d in mergedDicts)
            {
                if (d.Source != null && d.Source.OriginalString.Contains("Assets/Languages/Strings."))
                {
                    existingLangDict = d;
                    break;
                }
            }

            if (existingLangDict != null)
            {
                int index = mergedDicts.IndexOf(existingLangDict);
                mergedDicts[index] = dict;
            }
            else
            {
                mergedDicts.Add(dict);
            }
        }

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

            // Show Splash Screen first
            var splashScreen = ServiceProvider.GetRequiredService<Shell.SplashScreen>();
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
            services.AddTransient<CheckoutViewModel>();
 
            // Views / Windows
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<Shell.SplashScreen>();
            services.AddTransient<Views.CheckoutWindow>();
        }
    }
}

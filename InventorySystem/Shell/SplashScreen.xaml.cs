using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace InventorySystem.Shell
{
    /// <summary>
    /// Lógica de interacción para SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                LoadingText.Text = "Iniciando sistema...";
                Progressbar.IsIndeterminate = false;
                Progressbar.Value = 15;

                await Task.Delay(150);

                LoadingText.Text = "Verificando base de datos y esquema...";
                Progressbar.Value = 40;

                // Ejecutar la inicialización completa y autorreparación de la base de datos
                await Task.Run(async () =>
                {
                    using var scope = App.ServiceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<InventorySystem.Data.AppDbContext>();
                    var auth = scope.ServiceProvider.GetRequiredService<InventorySystem.Services.IAuthService>();
                    await InventorySystem.Helpers.DatabaseInitializer.InitializeAsync(db, auth);
                });

                Progressbar.Value = 85;
                LoadingText.Text = "Todo listo.";
                await Task.Delay(250);

                Progressbar.Value = 100;

                var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
                App.Current.MainWindow = loginWindow;
                loginWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Advertencia al inicializar base de datos:\n{ex.Message}", "Inicialización", MessageBoxButton.OK, MessageBoxImage.Warning);
                try
                {
                    var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
                    App.Current.MainWindow = loginWindow;
                    loginWindow.Show();
                    Close();
                }
                catch { }
            }
        }

  

     
    }
}

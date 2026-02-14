using System.Windows;

namespace InventorySystem.Views
{
    public partial class SaleDetailView : Window
    {
        public SaleDetailView()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

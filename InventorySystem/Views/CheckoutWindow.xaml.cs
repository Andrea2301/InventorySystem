using System.Windows;

namespace InventorySystem.Views
{
    public partial class CheckoutWindow : Window
    {
        public CheckoutWindow()
        {
            InitializeComponent();
            
            Loaded += (s, e) => {
                if (TxtCashReceived != null && TxtCashReceived.Visibility == Visibility.Visible)
                {
                    TxtCashReceived.Focus();
                }
            };
        }
    }
}

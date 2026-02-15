using System;
using System.Windows;
using System.Windows.Controls;

namespace InventorySystem.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            
            // REMOVED: DataContext = this; 
            // This was preventing the ViewModel from being set by the DataTemplate system
            // The DataContext should be set automatically to HomeViewModel by MainWindow's DataTemplate
        }
    }
}

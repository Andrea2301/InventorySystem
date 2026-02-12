using InventorySystem.ViewModel;
using System.Windows;

namespace InventorySystem.Views
{
    public partial class ImportFileView : Window
    {
        public ImportFileView()
        {
            InitializeComponent();
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                if (DataContext is ImportFileViewModel vm)
                {
                    vm.IsDragging = true;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            if (DataContext is ImportFileViewModel vm)
            {
                vm.IsDragging = false;
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is ImportFileViewModel vm)
            {
                vm.IsDragging = false;
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    vm.HandleDroppedFile(files);
                }
            }
        }
    }
}

using HandyControl.Tools.Extension;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
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

namespace InventorySystem.Views
{
    /// <summary>
    /// Lógica de interacción para ClientView.xaml
    /// </summary>
    public partial class ClientView : UserControl
    {
        public ClientView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            int MaxLength = 100; // Limite de caracteres

            if (textBox.Text.Length > MaxLength)

            {

                // Colocamos el cursor al final del texto
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

          
        }
    }
}


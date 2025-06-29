using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _iconChar;

        public ViewModelBase CurrentChildView
        {
            get { return _currentChildView; }
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }

        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        public IconChar IconChar
        {
            get { return _iconChar; }
            set
            {
                _iconChar = value;
                OnPropertyChanged(nameof(Icon)); // Corregido aquí
            }
        }

        //--> Commands
        public ICommand ShowHomeViewCommand { get;}
        public ICommand ShowClientViewCommand { get;}
        public ICommand ShowProductsViewCommand { get;}

        public MainViewModel()
        {

            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowClientViewCommand = new ViewModelCommand(ExecuteShowClientViewCommand);
            ShowProductsViewCommand = new ViewModelCommand(ExecuteShowProductsViewCommand); 
            //Default view
            ExecuteShowHomeViewCommand(null);
           


        }


        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = new HomeViewModel();
            Caption = "Dashboard";
            IconChar = IconChar.Home;
        }

        private void ExecuteShowClientViewCommand(object obj)
        {
            CurrentChildView = new ClientViewModel();
            Caption = "Clients";
            IconChar = IconChar.UserGroup; 
        }


        private void ExecuteShowProductsViewCommand(object obj)
        {
            CurrentChildView = new ProductsViewModel();
            Caption = "Products";
            IconChar = IconChar.Boxes;
        }

    }
}


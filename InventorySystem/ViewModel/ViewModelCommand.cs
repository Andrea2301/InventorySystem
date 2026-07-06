using System;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    // Clase que implementa la interfaz ICommand para representar comandos en ViewModels
    class ViewModelCommand : ICommand
    {
        // Acción que se ejecutará cuando se invoque el comando
        private readonly Action<object> _executeAction;
        // Predicado que determina si el comando puede ejecutarse en un momento dado
        private readonly Predicate<object>? _canExecuteAction;

        // Constructor que recibe solo la acción a ejecutar
        public ViewModelCommand(Action<object> executeAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = null;
        }

        // Constructor que recibe tanto la acción a ejecutar como el predicado de ejecución
        public ViewModelCommand(Action<object> executeAction, Predicate<object> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        // Evento que se dispara cuando cambia la capacidad de ejecución del comando
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }  // Fix: was += causing memory leak
        }

        // Método que determina si el comando puede ejecutarse en un momento dado
        public bool CanExecute(object? parameter)
        {
            return _canExecuteAction == null || _canExecuteAction(parameter!);
        }

        // Método que ejecuta la acción del comando
        public void Execute(object? parameter)
        {
            _executeAction(parameter!);
        }

        // Fuerza la re-evaluación del CanExecute en el UI thread (de forma asíncrona para evitar deadlocks)
        public static void RaiseCanExecuteChanged()
        {
            if (Application.Current?.Dispatcher != null)
                Application.Current.Dispatcher.BeginInvoke(CommandManager.InvalidateRequerySuggested);
        }
    }
}

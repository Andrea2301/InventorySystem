using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using System;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    // Clase que implementa la interfaz ICommand para representar comandos en ViewModels
    class ViewModelCommand : ICommand
    {
        // Acción que se ejecutará cuando se invoque el comando
        private readonly Action<object> _executeAction;
        // Predicado que determina si el comando puede ejecutarse en un momento dado
        private readonly Predicate<object> _canExecuteAction;

        // Constructor que recibe solo la acción a ejecutar
        public ViewModelCommand(Action<object> executeAction)
        {
            _executeAction = executeAction;
            // No se proporciona un predicado de ejecución (siempre se puede ejecutar)
            _canExecuteAction = null;
        }

        // Constructor que recibe tanto la acción a ejecutar como el predicado de ejecución
        public ViewModelCommand(Action<object> executeAction, Predicate<object> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        // Evento que se dispara cuando cambia la capacidad de ejecución del comando
        public event EventHandler CanExecuteChanged
        {
            // Agrega y elimina un controlador de eventos a CommandManager.RequerySuggested
            // Este evento se dispara cuando el sistema sugiere que se vuelva a evaluar si el comando puede ejecutarse
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested += value; }
        }

        // Método que determina si el comando puede ejecutarse en un momento dado
        public bool CanExecute(object? parameter)
        {
            // Si no se proporciona un predicado de ejecución, se asume que siempre se puede ejecutar
            return _canExecuteAction == null ? true : _canExecuteAction(parameter);
        }

        // Método que ejecuta la acción del comando
        public void Execute(object? parameter)
        {
            _executeAction(parameter);
        }
    }
}

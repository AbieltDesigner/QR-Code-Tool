using System.Windows.Input;

namespace QR_Code_Tool_App.VievModels
{
    public class CommandHandler : ICommand
    {
        private Action execute;
        private Func<object, bool> canExecute;

        public CommandHandler(Action execute, Func<object, bool> canExecute = null!)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute(parameter!);
        }

        public void Execute(object? parameter)
        {
            execute();
        }
    }
}

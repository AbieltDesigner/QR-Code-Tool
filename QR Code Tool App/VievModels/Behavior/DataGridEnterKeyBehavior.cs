using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace QR_Code_Tool_App.VievModels.Behavior
{
    public class DataGridEnterKeyBehavior : Behavior<DataGrid>
    {
        // DependencyProperty для команды
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(DataGridEnterKeyBehavior));

        // Стандартный интерфейс ICommand из System.Windows.Input
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += HandlePreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= HandlePreviewKeyDown;
            base.OnDetaching();
        }

        private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var dataGrid = (DataGrid)sender;

                // Фиксируем изменения перед выполнением команды
                dataGrid.CommitEdit(DataGridEditingUnit.Row, true);

                // Создаем параметр для команды
                var parameter = new EnterKeyArgs(
                    dataGrid.SelectedItem,
                    e
                );

                // Проверяем и выполняем команду
                if (Command?.CanExecute(parameter) == true)
                {
                    Command.Execute(parameter);
                    e.Handled = true; // Блокируем стандартное поведение
                }
            }
        }
    }
}

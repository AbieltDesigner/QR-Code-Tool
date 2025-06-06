using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace QR_Code_Tool_App.VievModels.Behavior
{
    public class DataGridRowDoubleClickBehavior : Behavior<DataGrid>
    {
        // DependencyProperty для команды
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(DataGridRowDoubleClickBehavior),
                new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDoubleClick += OnDataGridDoubleClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDoubleClick -= OnDataGridDoubleClick;
            base.OnDetaching();
        }

        private void OnDataGridDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Command == null) return;

            // Определяем, была ли нажата строка
            var source = e.OriginalSource as DependencyObject;

            // Ищем родительскую строку
            while (source != null && source is not DataGridRow)
            {
                source = VisualTreeHelper.GetParent(source);
            }

            if (source is DataGridRow row)
            {
                var item = row.Item;
                if (Command.CanExecute(item))
                {
                    Command.Execute(item);
                }
            }
        }
    }
}

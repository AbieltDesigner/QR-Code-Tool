using System.Windows;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool_App.VievModels.Behavior
{
    using System.Collections;
    using System.Windows.Controls;
    using Microsoft.Xaml.Behaviors;

    public class DataGridSelectedItemsBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                "SelectedItems",
                typeof(IList),
                typeof(DataGridSelectedItemsBehavior),
                new PropertyMetadata(null));

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += DataGrid_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= DataGrid_SelectionChanged;
            base.OnDetaching();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItems == null) return;

            // Очищаем коллекцию и добавляем элементы с проверкой типа
            SelectedItems.Clear();
            foreach (var item in AssociatedObject.SelectedItems)
            {
                // Важно: проверяем тип перед добавлением!
                if (item is Resource resource)
                {
                    SelectedItems.Add(resource);
                }
            }
        }
    }
}

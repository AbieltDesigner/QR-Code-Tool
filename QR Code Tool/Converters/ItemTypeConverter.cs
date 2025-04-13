using System;
using System.Globalization;
using System.Windows.Data;

namespace QR_Code_Tool.Converters
{
    public class ItemTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {        
            if (value is YandexDisk.Client.Protocol.ResourceType.Dir)
            {
                return "Папка";
            }

            if (value is YandexDisk.Client.Protocol.ResourceType.File)
            {
                return "Файл";
            }
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
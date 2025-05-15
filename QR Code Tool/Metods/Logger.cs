using System;
using System.IO;

namespace QR_Code_Tool.Metods
{
    public sealed class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        private static readonly object _lock = new object();
        private readonly string _logFilePath;

        // Приватный конструктор
        private Logger()
        {
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
        }

        // Доступ к экземпляру синглтона
        public static Logger Instance => _instance.Value;

        // Метод для записи исключений
        public void Log(Exception ex)
        {
            lock (_lock)
            {
                try
                {
                    using (var writer = new StreamWriter(_logFilePath, true))
                    {
                        WriteExceptionDetails(writer, ex);
                    }
                }
                catch
                {
                    // Ошибка записи в лог (не выбрасываем исключение)
                }
            }
        }

        // Рекурсивная запись деталей исключения
        private void WriteExceptionDetails(StreamWriter writer, Exception ex, int depth = 0)
        {
            writer.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}]");
            writer.WriteLine($"{(depth == 0 ? "EXCEPTION" : $"INNER EXCEPTION ({depth})")}: {ex.GetType()}");
            writer.WriteLine($"Message: {ex.Message}");
            writer.WriteLine($"Stack Trace:\n{ex.StackTrace}");
            writer.WriteLine(new string('-', 60));

            if (ex.InnerException != null)
            {
                WriteExceptionDetails(writer, ex.InnerException, depth + 1);
            }
        }
    }
}

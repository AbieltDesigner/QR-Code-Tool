using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QR_Code_Tool.Serializable.Entity;

namespace Serializator
{
    class Program
    {
        static void Main(string[] args)
        {
            var appSettings = new AppSettings(ClientSettings(), PrintSettings(), FolderSettings());

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var sw = new StreamWriter("appSettings.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.Indented })
                {
                    serializer.Serialize(writer, appSettings);
                }
            }

            Console.WriteLine(@"Файл json успешно создан!");
            Console.ReadKey();
        }

        private static ClientSettings ClientSettings()
        {
            string сlientId = "e44807741c564ca9bcbf3cb14ef20be5";
            string returnUrl = "127.0.0.1:130/test";
            string clientSecret = "35090a02b8b3485cb632c57d8f337e8b";
            return new ClientSettings(сlientId, returnUrl, clientSecret);
        }



        private static PrintSettings PrintSettings()
        {
            string comPort = "COM3";
            int sizeFont = 11;

            return new PrintSettings(comPort, sizeFont);
        }

        private static FolderSettings FolderSettings()
        {
            string folderLocation = "Информация для заказчиков и объектов";
            return new FolderSettings(folderLocation);
        }
    }
}
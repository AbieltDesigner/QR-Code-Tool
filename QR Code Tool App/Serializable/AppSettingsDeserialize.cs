using System.IO;
using Newtonsoft.Json;
using QR_Code_Tool_App.Serializable.Entity;

namespace QR_Code_Tool_App.Serializable
{
    public class AppSettingsDeserialize(string jsonPatch)
    {
        private readonly string jsonPatch = jsonPatch;

        public AppSettings GetSettingsModels()
        {
            JsonSerializer serializer = new();
            using StreamReader sw = new(jsonPatch);
            using JsonReader reader = new JsonTextReader(sw);
            return serializer.Deserialize<AppSettings>(reader)!;
        }
    }
}

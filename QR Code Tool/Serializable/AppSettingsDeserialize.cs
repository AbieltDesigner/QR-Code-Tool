﻿using System.IO;
using Newtonsoft.Json;
using QR_Code_Tool.Serializable.Entity;

namespace QR_Code_Tool.Serializable
{
    public class AppSettingsDeserialize
    {
        private readonly string jsonPatch;

        public AppSettingsDeserialize(string jsonPatch)
        {
            this.jsonPatch = jsonPatch;
        }

        public AppSettings GetSettingsModels()
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader sw = new StreamReader(jsonPatch))
            {
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    return serializer.Deserialize<AppSettings>(reader);
                }
            }
        }
    }
}

namespace QR_Code_Tool_App.Serializable.Entity
{
    public class ClientSettings(string clientId, string returnUrl, string clientSecret) : IClientSettings
    {
        public string clientId { get; set; } = clientId;
        public string returnUrl { get; set; } = returnUrl;
        public string clientSecret { get; set; } = clientSecret;
    }
}

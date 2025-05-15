namespace QR_Code_Tool.Serializable.Entity
{
    public class ClientSettings : IClientSettings
    {
        public string clientId { get; set; }
        public string returnUrl { get; set; }
        public string clientSecret { get; set; }

        public ClientSettings(string clientId, string returnId, string clientSecret)
        {
            this.clientId = clientId;
            this.returnUrl = returnUrl;
            this.clientSecret = clientSecret;
        }
    }
}

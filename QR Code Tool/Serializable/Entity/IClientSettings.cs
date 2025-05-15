namespace QR_Code_Tool.Serializable.Entity
{
    public interface IClientSettings
    {
        string clientId { get; set; }
        string returnUrl { get; set; }
        string clientSecret { get; set; }
    }
}

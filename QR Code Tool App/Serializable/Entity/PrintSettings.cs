namespace QR_Code_Tool_App.Serializable.Entity
{
    public class PrintSettings(string printName, int sizeFont) : IPrintSettings
    {
        public string PrintName { get; set; } = printName;
        public int SizeFont { get; set; } = sizeFont;
    }
}

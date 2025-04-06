namespace QR_Code_Tool.Serializable.Entity
{
    public class PrintSettings : IPrintSettings
    {
        public string ComPort { get; set; }
        public int SizeFont { get; set; }

        public PrintSettings(string comPort, int sizeFont)
        {
            this.ComPort = comPort;
            this.SizeFont = sizeFont;
        }
    }
}

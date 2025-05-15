namespace QR_Code_Tool.Serializable.Entity
{
    public class PrintSettings : IPrintSettings
    {
        public string PrintName { get; set; }
        public int SizeFont { get; set; }

        public PrintSettings(string printName, int sizeFont)
        {
            this.PrintName = printName;
            this.SizeFont = sizeFont;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using QR_Code_Tool.Serializable.Entity;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool.Metods
{
    public class PrintZPL
    {
        private readonly IEnumerable<Resource> items;
        private readonly IPrintSettings printSettings;
        public PrintZPL(IEnumerable<Resource> items, IPrintSettings printSettings)
        {
            this.items = items;
            this.printSettings = printSettings;
        }
        public void Print()
        {
            foreach (Resource item in items)
            {
                var sampleText = item.Name;
                var font = new ZplFont(fontWidth: printSettings.SizeFont, fontHeight: printSettings.SizeFont);
                var elements = new List<ZplElementBase>();
                elements.Add(new ZplTextField(sampleText, 50, 100, font));
                elements.Add(new ZplQrCode(item.PublicUrl, 100, 200, 2, 12));
                var renderEngine = new ZplEngine(elements);
                var output = renderEngine.ToZplString(new ZplRenderOptions { AddEmptyLineBeforeElementStart = true });
                System.Windows.Clipboard.SetText(output);
                //SendPrinter(output);
                Debug.WriteLine(output);
            }
        }

        private void SendPrinter(string zplData)
        {
            try
            {
                using (SerialPort printerPort = new SerialPort(printSettings.ComPort, 9600, Parity.None, 8, StopBits.One))
                {
                    printerPort.Open();
                    printerPort.Write(zplData);
                }

                Debug.WriteLine("Data sent to the printer.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error sending data to the printer: " + ex.Message);
            }
        }
    }
}

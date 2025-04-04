using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool.Metods
{
    public class PrintZPL
    {
        private string printerPortName = "COM3";
        private readonly IEnumerable<Resource> items;
        public PrintZPL(IEnumerable<Resource> items)
        {
            this.items = items;
        }
        public void Print()
        {
            foreach (Resource item in items)
            {
                var sampleText = item.Name;
                var font = new ZplFont(fontWidth: 50, fontHeight: 50);
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
                using (SerialPort printerPort = new SerialPort(printerPortName, 9600, Parity.None, 8, StopBits.One))
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

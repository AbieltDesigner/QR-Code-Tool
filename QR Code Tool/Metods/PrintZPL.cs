﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using QR_Code_Tool.Serializable.Entity;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool.Metods
{
    public class PrintZPL
    {
        // WinAPI методы для работы с принтером
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool OpenPrinter(string printerName, out IntPtr phPrinter, IntPtr pd);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOCINFO di);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        // Структура для документа
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DOCINFO
        {
            public string pDocName;
            public string pOutputFile;
            public string pDataType;
        }

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
                if (item.PublicUrl != null)
                {
                    var sampleText = item.Name;
                    var font = new ZplFont(fontWidth: printSettings.SizeFont, fontHeight: printSettings.SizeFont);
                    var elements = new List<ZplElementBase>();
                    elements.Add(new ZplTextField(sampleText, StartTextPosition(sampleText), 30, font));
                    elements.Add(new ZplQrCode(item.PublicUrl, 190, 100, 2, 12));
                    var renderEngine = new ZplEngine(elements);
                    var zplContent = renderEngine.ToZplString(new ZplRenderOptions { AddEmptyLineBeforeElementStart = true });
                    SendToPrinter(printSettings.PrintName, zplContent);
                }
                else
                {
                    MessageBox.Show($"Попытка распечатать без публичной ссылки.{Environment.NewLine} Публичная ссылка на каталог или файл {item.Name} не сформированна.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
        }

        private static int StartTextPosition(string sampleText)
        {
            int realLength = sampleText.Length * 3;
            double pole = (100 - realLength) / 2;
            if (pole > 0)
            {
                return (int)pole * (740 / 100);
            }
            return 0;
        }

        private static void SendToPrinter(string printerName, string zplData)
        {
            IntPtr hPrinter = IntPtr.Zero;
            DOCINFO di = new DOCINFO
            {
                pDocName = "ZPL Print Job",
                pDataType = "RAW"
            };

            try
            {
                // Открываем принтер
                if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                    try
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    catch (Win32Exception ex)
                    {
                        MessageBox.Show($"Принтер {printerName} не установлен. Печать невозможна.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                        Logger.Instance.Log(ex);
                        return;
                    }

                // Начинаем документ
                if (!StartDocPrinter(hPrinter, 1, ref di))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // Начинаем страницу
                if (!StartPagePrinter(hPrinter))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // Конвертируем данные в байты
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(zplData);

                // Выделяем память и копируем данные
                IntPtr pBytes = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, pBytes, bytes.Length);

                // Отправляем данные
                if (!WritePrinter(hPrinter, pBytes, bytes.Length, out int dwWritten))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // Завершаем печать
                Marshal.FreeCoTaskMem(pBytes);
                EndPagePrinter(hPrinter);
                EndDocPrinter(hPrinter);
            }
            finally
            {
                if (hPrinter != IntPtr.Zero)
                    ClosePrinter(hPrinter);
            }
        }
    }
}

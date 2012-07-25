using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using SecretLabs.NETMF.IO;


namespace SDCardHelloWorld
{
    class Logger
    {
        private FileStream filestream;


        //constructor
        public Logger()
        {
            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
            filestream = new FileStream(@"\SD\DeckerKicksNetduinoAss.txt", FileMode.Append);
        }

        public void Initialize()
        {
            //StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
            //FileStream filestream = new FileStream(@"\SD\DeckerKicksNetduinoAss.txt", FileMode.Append);
        }

        public void Log()
        {
            //opening the filestream in here was ~35ms/write when called in a loop as a Logger class; both mounting the SD card and creating a filestream take a lot of time.
            //writing 1 byte to the SD card in a loop (without opening a filestream or mounting the card) takes ~1.5ms.

            //StreamWriter streamWriter = new StreamWriter(filestream);
            //long longToWrite = 12345678;
            //int intToWrite = 1234567;
            //byte[] byteArraryToWrite = { 0x68, 0x65, 0x79, 0x20, 0x66, 0x75, 0x63, 0x6b, 0x20, 0x79, 0x6f, 0x75, 0x2c, 0x20, 0x79, 0x6f, 0x75, 0x20, 0x66, 0x75, 0x63, 0x6b, 0x69, 0x6e, 0x67, 0x20, 0x66, 0x75, 0x63, 0x6b, 0x21 };
            byte byteToWrite = 0x68;

            //streamWriter.WriteLine("\t" + stopwatch.ElapsedMilliseconds.ToString()); // this combined line is 2519ms/1k-line, 2274ms/1k-line
            //filestream.Write(byteArraryToWrite, 0, byteArraryToWrite.Length); //byte[] = 1776ms/1k-line
            filestream.WriteByte(byteToWrite);  //byte = 804ms/1k-line
            
        }

        public void Close()
        {
            //streamWriter.Close();
            filestream.Close();
            StorageDevice.Unmount("SD");
        }

    }
}

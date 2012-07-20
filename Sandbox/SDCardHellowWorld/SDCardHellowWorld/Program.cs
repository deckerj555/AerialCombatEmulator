using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using SecretLabs.NETMF.IO;

namespace SDCardHellowWorld
{
    public class Program
    {
        

        public static void Main()
        {
           
            
            
            try
            {
                StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);

                FileStream filestream = new FileStream(@"\SD\DeckerKicksNetduinoAss.txt", FileMode.Append);
                StreamWriter streamWriter = new StreamWriter(filestream);
                long longToWrite = 12345678;
                int intToWrite = 1234567;
                byte[] byteArraryToWrite = { 0x68, 0x65, 0x79, 0x20, 0x66, 0x75, 0x63, 0x6b, 0x20, 0x79, 0x6f, 0x75, 0x2c, 0x20, 0x79, 0x6f, 0x75, 0x20, 0x66, 0x75, 0x63, 0x6b, 0x69, 0x6e, 0x67, 0x20, 0x66, 0x75, 0x63, 0x6b, 0x21 };
                byte byteToWrite =  0x68 ;

                Stopwatch stopwatch = Stopwatch.StartNew();  //baseline with no writes in the for-loop to the SD card is 240ms.

                for (short i = 0; i < 1000; i++)
                {
                    //streamWriter.WriteLine(intToWrite);     // int =  8878ms/1k-line
                    //streamWriter.WriteLine(longToWrite);  // long  = 9753ms/1k-line, 9982ms/1k-line
                    //streamWriter.WriteLine("string data");  // string "some data" = ~1407/1k-line;
                    //streamWriter.Write(i);
                    streamWriter.WriteLine("\t" + stopwatch.ElapsedMilliseconds.ToString()); // this combined line is 2519ms/1k-line, 2274ms/1k-line
                    //filestream.Write(byteArraryToWrite, 0, byteArraryToWrite.Length); //byte[] = 1776ms/1k-line
                    //filestream.WriteByte(byteToWrite);  //byte = 804ms/1k-line


                    //if (i % 100 == 0)
                    //{
                    //    Debug.Print(i.ToString());
                    //}
                }
                Debug.Print(stopwatch.ElapsedMilliseconds.ToString());
                Debug.Print("================all done!======================");
                streamWriter.Close();
                filestream.Close();
                StorageDevice.Unmount("SD");



            }

            catch (Exception ex)
            {

                Debug.Print(ex.ToString());
            }

        }

    }
}

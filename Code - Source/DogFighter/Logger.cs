using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using SecretLabs.NETMF.IO;


namespace DogFighter
{
    class Logger
    {
        private FileStream filestream;
        private StreamWriter streamWriter;

        //constructor
        public Logger()
        {
            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
            filestream = new FileStream(@"\SD\DeckerRox.txt", FileMode.Append, FileAccess.Write, FileShare.None, 8);
            streamWriter = new StreamWriter(filestream);
            streamWriter.WriteLine("#GPSTime_csec\tLat_e7\tLon_e7\tAlt_cm\tPDOP_e2\ttSA_rad\tYaw_mrad\tyawWithMagCal_mrad\ttSE_rad\tPitch_mrad\tDistance_m\tEnemyLat_e7\tEnemyLon_e7\tEnemyAlt_cm\tEnemyPDOP_e2\tDLC\tMagCalCounter");
            streamWriter.Flush();
        }

        public void Initialize()
        {
            //StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
            filestream = new FileStream(@"\SD\DeckerIsFuckingAmazing.txt", FileMode.Append, FileAccess.Write, FileShare.None, 8);
            streamWriter = new StreamWriter(filestream);
        }

        public void Log(string stringToLog)
        {
            //StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
            //FileStream filestream = new FileStream(@"\SD\DeckerIsFuckingAmazing.txt", FileMode.Append, FileAccess.Write, FileShare.None, 8);
            //streamWriter = new StreamWriter(filestream);
            
            //long longToWrite = 12345678;
            //int intToWrite = 1234567;
            //byte[] byteArraryToWrite = { 0x68, 0x65, 0x79, 0x20, 0x66, 0x75, 0x63, 0x6b, 0x20, 0x79, 0x6f, 0x75, 0x2c, 0x20, 0x79, 0x6f, 0x75, 0x20, 0x66, 0x75, 0x63, 0x6b, 0x69, 0x6e, 0x67, 0x20, 0x66, 0x75, 0x63, 0x6b, 0x21 };
            //byte byteToWrite = 0x69;

            streamWriter.WriteLine(stringToLog); 
            //filestream.Write(byteArraryToWrite, 0, byteArraryToWrite.Length); 
            //filestream.WriteByte(byteToWrite);  
            //filestream.Close();
        }

        public void Close()
        {
            streamWriter.Close();
            filestream.Close();
            //StorageDevice.Unmount("SD");
        }

        public void Flush()
        {
            streamWriter.Flush();
            filestream.Flush();
        }

    }
}

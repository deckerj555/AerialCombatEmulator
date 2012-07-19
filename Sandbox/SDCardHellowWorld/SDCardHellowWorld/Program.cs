using System;
using System.IO;
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

                for (int i = 0; i < 11; i++)
                {

                    streamWriter.Write("Here's a line muthafucko: ");
                    streamWriter.WriteLine(i);
                }

                streamWriter.Close();
                filestream.Close();
                StorageDevice.Unmount("SD");
                Debug.Print("all done!");

            }

            catch (Exception ex)
            {

                Debug.Print(ex.ToString());
            }

        }

    }
}

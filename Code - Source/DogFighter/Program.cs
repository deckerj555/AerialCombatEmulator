using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace DogFighter
{
    // Declination is HARD CODED AT 16 DEGREES!!!!
    public class Program
    {
        public static void Main()
        {
            // Nothing's going to happen here playa, head to Main's Run() Method, that's where the action is.
            Main Mane = new Main();
            //Mane.LedTester();
            Mane.Run();
        }
    }
}
using System;
using Microsoft.SPOT;

namespace DogFighter
{
    class DeadLedControl
    {
        // Declarations
        int controllingInt;

        // Constructor
        public DeadLedControl(int controllingInt)
        {
            this.controllingInt = controllingInt;
        }

        // Overloaded constructor soley for the purpose of providing Radio with the size of a DLC
        public DeadLedControl()
        {
            // Nothing ever needs to go in here.
        }

        public static byte[] Serialize(DeadLedControl dlc)
        {
            byte[] buffer = new byte[4]; // Size must be increased if DLC is changed.

            Serialize(buffer, 0, dlc.controllingInt);

            return buffer;
        }

        public static DeadLedControl Deserialize(byte[] line)
        {
            int a = (line[0] << 24) | (line[1] << 16) | (line[2] << 8) | (line[3] << 0);

            DeadLedControl deserialized = new DeadLedControl(a);
            return deserialized;
        }
        
        // LTN 4/1/12: This is copy past from AttNav.  Since we're using the same code more than once, we could wrap it up into a class of it's own to handle serialization.  But at least for now, simplicity votes to just copy past it as I have here.
        private static void Serialize(byte[] buffer, int offset, int input)
        {
            buffer[offset + 0] = (byte)((input >> 24) & 0xFF); //take the top 8bits, AND them with 0xFF
            buffer[offset + 1] = (byte)((input >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((input >> 8) & 0xFF);
            buffer[offset + 3] = (byte)((input >> 0) & 0xFF);

        }

        public int ControllingInt
        {
            get
            {
                return controllingInt;
            }
        }
    }
}

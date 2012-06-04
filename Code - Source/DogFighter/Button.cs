using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;

namespace DogFighter
{
    class Button
    {
        // Declarations
        private InterruptPort physicalButton;
        private Led controlledLed;

        // Constructor
        public Button(Cpu.Pin ButtonPin, Led controlledLed)
        {
            physicalButton = new InterruptPort(ButtonPin, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
            physicalButton.OnInterrupt += new NativeEventHandler(physicalButton_OnInterrupt);
            this.controlledLed = controlledLed;
        }

        // Event Handler Method
        void physicalButton_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (controlledLed.CurrentState == Led.State.Blinking)
            {
                controlledLed.Off();
            }
            else
            {
                controlledLed.Blink();
            }
        }
    }
}
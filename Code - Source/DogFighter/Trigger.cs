using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;

namespace DogFighter
{
    class Trigger
    {
        // NOTE: ASSUMES trigger is NOT pulled during powerup.
        // For now, when trigger is pressed, triggerState will be changed to Pulled.
        // In future, we can put amunition limits on it, or be clever.

        // Declarations
        private InterruptPort physicalTrigger;
        private object locker;
        private State triggerState;
        public enum State
        {
            Pulled,
            Released
        }


        // Constructor
        public Trigger(Cpu.Pin TriggerPin)
        {
            physicalTrigger = new InterruptPort(TriggerPin, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
            physicalTrigger.OnInterrupt += new NativeEventHandler(physicalTrigger_OnInterrupt);
            locker = new Object();

            lock (locker)
            {
                triggerState = State.Released; // This assumes that the trigger is not pulled upon powerup
            }
         }

        void physicalTrigger_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (triggerState == State.Pulled)
            {
                lock (locker)
                {
                    triggerState = State.Released;
                }
            }
            else
            {
                lock (locker)
                {
                    triggerState = State.Pulled;
                }
            }
        }

        public State TriggerState
        {
            get
            {
                lock (locker)
                {
                    return triggerState;
                }
            }
        }
        
    }
}

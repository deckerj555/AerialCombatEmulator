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
        private Timer timer = new Timer();
        private UInt16 counter = 0;


        //Events
        public delegate void sevenTimesKeyedDelegate(bool state);
        public event sevenTimesKeyedDelegate sevenTimesKeyed;


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

        void physicalTrigger_OnInterrupt(uint data1, uint data2, DateTime time) //attach an interrupt which toggles the trigger state
        {
            if (counter == 0)
            {
                timer.Start();    
            }
            Debug.Print(timer.Stop().ToString());
            Debug.Print("counter: " + counter.ToString());

            if (timer.Stop() >= 4)
            {
                counter = 0;
            }
            else if (counter == 13) //interrupt fires on both rising and falling edges, so the counter needs to accrue 14 ticks to equal 7 trigger pulls--but because we're starting counter with 0 as the first loop, the counter only needs to go to 13.
            {
                counter = 0;
                if (this.sevenTimesKeyed != null)
                {
                    this.sevenTimesKeyed(true);
                }
            }
            else
            {
                counter++;
            }

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

        //properties
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

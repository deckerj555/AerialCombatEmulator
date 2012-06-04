using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;

namespace RazorVenusEmulator
{
    public class Led
    {
        private OutputPort physicalLed;
        private Thread blinkingThread;
        private object lockObject;
        private int blinkRate_msec;

        // Constructor
        public Led(Microsoft.SPOT.Hardware.Cpu.Pin LedPin)
        {
            physicalLed = new OutputPort(LedPin, false);
            lockObject = new Object();
            this.Off();
        }

        public void On()
        {
            lock (lockObject)
            {
                currentState = State.On;
                this.physicalLed.Write(true);
            }
        }

        public void Off()
        {
            lock (lockObject)
            {
                currentState = State.Off;
                this.physicalLed.Write(false);
            }
        }

        public void Blink()
        {
            this.Blink(250);
        }

        public void Blink(int rate_msec)
        {
            this.blinkRate_msec = System.Math.Abs(rate_msec);

            lock (lockObject)
            {
                if (currentState == State.Blinking)
                {
                    return;
                }
                else
                {
                    currentState = State.Blinking;
                }
            }
            blinkingThread = new Thread(
                                            delegate()
                                            {
                                                blink();
                                            }
                                        );
            blinkingThread.Start();
        }

        private void blink()
        {
            while (true)
            {
                lock (lockObject)
                {
                    if (currentState != State.Blinking)
                    {
                        break;
                    }
                    this.physicalLed.Write(true);
                }
                Thread.Sleep(this.blinkRate_msec / 2);
                lock (lockObject)
                {
                    if (currentState != State.Blinking)
                    {
                        break;
                    }
                    this.physicalLed.Write(false);
                }
                Thread.Sleep(this.blinkRate_msec / 2);
            }

        }

        public enum State
        {
            On,
            Off,
            Blinking
        }

        private State currentState;

        public State CurrentState
        {
            get
            {
                lock (lockObject)
                {
                    return currentState;
                }
            }
        }
    }
}

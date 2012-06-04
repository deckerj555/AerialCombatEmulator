using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;

namespace DogFighter
{
    public class Led
    {
        private OutputPort physicalLed;
        private Thread blinkingThread;
        private Thread blinkyblinkingThread;
        private object currentStateLocker;
        private int blinkRate_msec;

        // Constructor
        public Led(Microsoft.SPOT.Hardware.Cpu.Pin LedPin)
        {
            physicalLed = new OutputPort(LedPin, false);
            currentStateLocker = new Object();
            this.Off();
        }

        public void On()
        {
            lock (currentStateLocker)
            {
                currentState = State.On;
                this.physicalLed.Write(true);
            }
        }

        public void Off()
        {
            lock (currentStateLocker)
            {
                currentState = State.Off;
                this.physicalLed.Write(false);
            }
        }

        public void Blink()
        {
            this.Blink(250);
        }

        public void BlinkyBlink()
        {
            lock (currentStateLocker)
            {
                if (currentState == State.BlinkyBlinkin)
                {
                    return;
                }
                else
                {
                    currentState = State.BlinkyBlinkin;
                }
            }
            blinkyblinkingThread = new Thread(
                                                delegate()
                                                {
                                                    blinkyblink();
                                                }
                                              );
            blinkyblinkingThread.Start();
        }

        private void blinkyblink()
        {
            while (true)
            {
                lock (currentStateLocker)
                {
                    if (currentState != State.BlinkyBlinkin)
                    {
                        break;
                    }
                    this.physicalLed.Write(true);           // On
                }
                Thread.Sleep(100);
                lock (currentStateLocker)
                {
                    if (currentState != State.BlinkyBlinkin)
                    {
                        break;
                    }
                    this.physicalLed.Write(false);          // Off 
                }
                Thread.Sleep(50);
                lock (currentStateLocker)
                {
                    if (currentState != State.BlinkyBlinkin)
                    {
                        break;
                    }
                    this.physicalLed.Write(true);           // Onnn
                }
                Thread.Sleep(300);
                lock (currentStateLocker)
                {
                    if (currentState != State.BlinkyBlinkin)
                    {
                        break;
                    }
                    this.physicalLed.Write(false);          // Offfff
                }
                Thread.Sleep(300);
            }
        }

        public void Blink(int rate_msec)
        {
            this.blinkRate_msec = System.Math.Max(System.Math.Abs(rate_msec), 33); // Never need to blink faster than 30hz, cause you can't see that fast

            lock (currentStateLocker)
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
                lock (currentStateLocker)
                {
                    if (currentState != State.Blinking)
                    {
                        break;
                    }
                    this.physicalLed.Write(true);
                }
                Thread.Sleep(this.blinkRate_msec / 2);
                lock (currentStateLocker)
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
            Blinking,
            BlinkyBlinkin
        }

        private State currentState;

        public State CurrentState
        {
            get
            {
                lock (currentStateLocker)
                {
                    return currentState;
                }
            }
        }
    }
}

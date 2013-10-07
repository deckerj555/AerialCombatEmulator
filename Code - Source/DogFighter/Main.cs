using System;
using Microsoft.SPOT;
using System.Threading;
using SecretLabs.NETMF.Hardware.Netduino;

// Input/Setup: No inputs, instance "Mane" is created in Program.cs and called by Program 1 time, then Run() takes over.
// Operation:   Creates Position computer, and runs it at 50hz.
// Output:      Subscribes to the events ReceivedLineGPS and ReceivedLineIMU.

namespace DogFighter
{
    class Main
    {
        // Declarations
        Led statusLed = new Led(Pins.ONBOARD_LED);
        Led imuLed = new Led(Pins.GPIO_PIN_D7);   // green
        Led gpsLed = new Led(Pins.GPIO_PIN_D6);   // yellow
        Led deadLed = new Led(Pins.GPIO_PIN_D5);  // red
        Led killLed = new Led(Pins.GPIO_PIN_D4);  // white super-bright 

        GPSIMU razorVenus = new GPSIMU();
        PositionComputer positionComputer = new PositionComputer();
        Radio xbee = new Radio();
        Trigger trigger = new Trigger(Pins.ONBOARD_SW1);
        FiringSolution firingSolution = new FiringSolution();

        public void Run()
        {
            #region Wire up all Classes
            positionComputer.Initialize(razorVenus, imuLed, gpsLed);
            xbee.Initialize(positionComputer, firingSolution, deadLed, razorVenus);
            firingSolution.Initialize(positionComputer, xbee, trigger, killLed, statusLed, razorVenus, gpsLed);
            #endregion

            #region System Ready Blinks
            statusLed.On();
            Thread.Sleep(300);
            imuLed.On();
            Thread.Sleep(300);
            gpsLed.On();
            Thread.Sleep(300);
            deadLed.On();
            Thread.Sleep(300);
            killLed.On();
            Thread.Sleep(3000); // LTN March 20, 2012: This is a long LED startup sequence, giving the imu time to fully initialize before the imu data is being used to affect imuLed.  Otherwise you get the imuLed blinkyblinking as if it's north because while initializing it outputs a yaw angle of 0mrads.
            statusLed.Off();
            imuLed.Off();
            gpsLed.Off();
            deadLed.Off();
            killLed.Off();
            #endregion

            string lineToPrint = "\n\rSystem Wired and Ready for Action\n\r\n\r";
            razorVenus.TerminalPrintOut(lineToPrint);

            //DLC subscription to test DLC output via serial debug
            xbee.ReceivedImDeadDLC += new Radio.DLCReceivedDelegate(xbee_ReceivedDLCMe);

            // Call Testing here, after all wiring has been completed.
            Testing();


            for (; ; )
            {
                positionComputer.Compute();
                Thread.Sleep(100); // Main loop clock cycle Timer

                // StatusLed Flashes to confirm Loop is still running.
                if (statusLed.CurrentState == Led.State.Off)
                {
                    statusLed.On();
                }
                else
                {
                    statusLed.Off();
                }
            }
        }

        // DLC received debug printout "I JUST DIED"
        void xbee_ReceivedDLCMe(DeadLedControl dlcToMe)
        {
            if (dlcToMe.ControllingInt == 1)
            {
                razorVenus.TerminalPrintOut("\n\r*\n\r*\n\r*\n\r*\n\r*\n\r*\n\r*\n\r*\n\r*\n\r*I JUST DIED\n\r*I JUST DIED\n\r*I JUST DIED\n\r*I JUST DIED\n\r*I JUST DIED");
            }
        }

        #region Testing() - Insert a fake radio stream
        public void Testing()
        {
            AttNav tAttNav = new AttNav(-234847615, -379916876, 453814132, 1600, 3100, 780, 456486502, -121722502, 150000, 1, 10000);
            DeadLedControl tDLC = new DeadLedControl(1);
            byte[] tDlCSerialized = DeadLedControl.Serialize(tDLC);

            byte[] tAttNavSerialized = AttNav.Serialize(tAttNav);
            byte[] fakeRadioStream1 = new byte[51];
            fakeRadioStream1[0] = 0x00; // This is a dummy byte because InsertTestBuffer uses tailindex, which is often 1 because the datareceived event triggers on startup.
            fakeRadioStream1[1] = 0xB0;
            fakeRadioStream1[2] = 0xB1;
            Array.Copy(tAttNavSerialized, 0, fakeRadioStream1, 3, tAttNavSerialized.Length);
            UInt16 checkSum = Radio.CheckSumCalc(tAttNavSerialized);
            fakeRadioStream1[47] = (byte)(checkSum >> 8);
            fakeRadioStream1[48] = (byte)(checkSum);
            fakeRadioStream1[49] = 0x0D;
            fakeRadioStream1[50] = 0x0A;

            byte[] fakeRadioStream2 = new byte[16];
            fakeRadioStream2[0] = 0x44;
            fakeRadioStream2[1] = 0x4C;
            fakeRadioStream2[2] = 0x43;
            Array.Copy(tDlCSerialized, 0, fakeRadioStream2, 3, tDlCSerialized.Length);
            UInt16 checkSumtDLC = Radio.CheckSumCalc(tDlCSerialized);
            fakeRadioStream2[7] = (byte)(checkSumtDLC >> 8);
            fakeRadioStream2[8] = (byte)(checkSumtDLC);
            fakeRadioStream2[9] = 0x0D;
            fakeRadioStream2[10] = 0x0A;
            fakeRadioStream2[11] = 0xAA; // Pretty sure these next bytes (11-15) are just noise, intended to make sure the parsing engine can handle noise showing up with real infomation.
            fakeRadioStream2[12] = 0xAB;
            fakeRadioStream2[13] = 0xAC;
            fakeRadioStream2[14] = 0x0F;
            fakeRadioStream2[15] = 0x01;

            xbee.InsertTestBuffer(fakeRadioStream1);
            // Thread.Sleep(10); // shitty sleep 
            xbee.InsertTestBuffer(fakeRadioStream2);
        }
        #endregion

        public void LedTester()
        {
            // System Ready Blinks
            statusLed.On();
            Thread.Sleep(200);
            imuLed.On();
            Thread.Sleep(200);
            gpsLed.On();
            Thread.Sleep(200);
            deadLed.On();
            Thread.Sleep(200);
            killLed.On();
            Thread.Sleep(2000);
            statusLed.Off();
            imuLed.Off();
            gpsLed.Off();
            deadLed.Off();
            killLed.Off();

            statusLed.BlinkyBlink();
            Thread.Sleep(3000);
            imuLed.BlinkyBlink();
            Thread.Sleep(3000);
            gpsLed.BlinkyBlink();
            Thread.Sleep(3000);
            deadLed.BlinkyBlink();
            Thread.Sleep(3000);
            killLed.BlinkyBlink();
            Thread.Sleep(6000);
            statusLed.Off();
            imuLed.Off();
            gpsLed.Off();
            deadLed.Off();
            killLed.Off();

            statusLed.Blink(250);
            Thread.Sleep(1000);
            imuLed.Blink(250);
            Thread.Sleep(1000);
            gpsLed.Blink(250);
            Thread.Sleep(1000);
            deadLed.Blink(250);
            Thread.Sleep(1000);
            killLed.Blink(250);
            Thread.Sleep(1000);
            statusLed.Off();
            imuLed.Off();
            gpsLed.Off();
            deadLed.Off();
            killLed.Off();


            string lineToPrint = "\n\rLed System Check Run Complete\n\r\n\r";
            razorVenus.TerminalPrintOut(lineToPrint);
        }
    }
}
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
        Led imuLed = new Led(Pins.GPIO_PIN_D7);
        Led gpsLed = new Led(Pins.GPIO_PIN_D6);   
        Led deadLed = new Led(Pins.GPIO_PIN_D5);
        Led killLed = new Led(Pins.GPIO_PIN_D4);   
        int gpsTimeLast_csec;
        int gpsTimeNew_csec;

        GPSIMU razorVenus = new GPSIMU();
        PositionComputer positionComputer = new PositionComputer();
        Radio xbee = new Radio();
        Trigger trigger = new Trigger(Pins.ONBOARD_SW1);
        FiringSolution firingSolution = new FiringSolution();

        public void Run()
        {
            Thread.Sleep(50); // TODO: We think this should be deleted.  Lowell wants to leave it until the DLC problem is solved.  THEN! we shall blow it away.

            // Wire up all of the classes to eachother as necessary
            positionComputer.Initialize(razorVenus, imuLed, gpsLed);
            xbee.Initialize(positionComputer, firingSolution, deadLed, razorVenus);
            firingSolution.Initialize(positionComputer, xbee, trigger, killLed, statusLed, razorVenus);

            // Button is commented out because we're currently using ONBOARD_SW1 for the trigger
            // Button mainButton = new Button(Pins.ONBOARD_SW1, statusLed);

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
            //Testing();

            //Calling xbee.HelloWorld, after commenting out the forloop below this.  This is being used to test the radios, and we should probably roll back after this LTN 6-1-2012
            Thread.Sleep(4000);
            statusLed.On();
            byte[] helloWorld = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            xbee.HelloWorld(helloWorld);

            xbee.TerminalPrintOut2("HelloWorld");
            statusLed.Blink(500);
      
            //JPD and LTN 6-1-2012
            // Commenting out the for loop to use this code as a HelloWorld to test the radios.
            //gpsTimeLast_csec = 0;
            //gpsTimeNew_csec = 0;
            //for (; ; )
            //{
            //    gpsTimeNew_csec = positionComputer.Compute(gpsTimeLast_csec);
            //    Thread.Sleep(100); //main loop clock at 50Hz (firing solution to be feed at 50hz) // LTN: March 6, changed to 20hz instead of 50...no real good justification here, so feel free to change back if desired  LTN: March 11, changed to 10 hz.

            //    // We're doing this check twice..once here, and once in PositionComputer where we decide whether or not to zero out the gpsTime and dop.  Instead of storing the gpsTimeLast and TimeNew in Mane.  Store them in the instance of PositionComputer that we've created, and do the check only once, in that location.  Then don't even bother turning on gpsLed from that location, just create an AttNav that' flagged as stale.  Downstream in firingSolution, do the AttNav stale data check first, and turn the Led on from there.  It's where you're going to have to do an EnemyStale check anyway.
            //    if (gpsTimeLast_csec < gpsTimeNew_csec)
            //    {
            //        gpsTimeLast_csec = gpsTimeNew_csec;
            //    }
            //    else
            //    {
            //        gpsLed.BlinkyBlink();
            //        gpsTimeLast_csec = gpsTimeNew_csec;
            //    }

            //    // StatusLed Flashes to confirm Loop is still running.
            //    if (statusLed.CurrentState == Led.State.Off)
            //    {
            //        statusLed.On();
            //    }
            //    else
            //    {
            //        statusLed.Off();
            //    }
            //}
        }

        void xbee_ReceivedDLCMe(DeadLedControl dlcToMe)
        {
            razorVenus.TerminalPrintOut("\n\rDLC: " + dlcToMe.ControllingInt);
        }

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
            Thread.Sleep(10); // TODO: DELETE THIS, AS SOON AS THE DLC PROBLEM IS FIXED.!!!
            xbee.InsertTestBuffer(fakeRadioStream2);
        }

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
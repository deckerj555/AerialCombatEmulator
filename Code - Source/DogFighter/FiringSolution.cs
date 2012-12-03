using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace DogFighter
{
    class FiringSolution
    {
        #region Delclarations
        private AttNav positionEnemy; // This will be the "To Use" value.
        private AttNav positionEnemyReceived;
        private Object enemyLocker;
        private AttNav positionMe; // This will be the "To Use" value.
        private AttNav positionMeReceived;
        private Object meLocker;
        private Led killLed;
        private Led statusLed;
        private Led gpsLed;
        private GPSIMU forDebugPrint;
        private Logger logger = new Logger();
        private Trigger trigger;

        private UInt16 loggingCounter = 0;
        private double toShootAzimuth_rad;
        private double yawOffsetFromMagCal_mrad = 0;
        private double yawWithMagCal_mrad;
        private UInt16 dlcForLogging = 0;
        #endregion

        #region EventSetup
        public delegate void DLCCreatedDelegate(DeadLedControl dlcToEnemy);
        public event DLCCreatedDelegate DLCCreated;
        #endregion


        // Initializer, used to give the Radio, PositionComputer, and FiringSolution classes each other's events.
        public void Initialize(PositionComputer positionComputer, Radio xbee, Trigger trigger, Led firingLed, Led statusLed, GPSIMU forDebugPrint, Led gpsLed)
        {
            positionComputer.AttNavCreated += new PositionComputer.AttNavDataReadyDelegate(positionComputer_AttNavCreated);
            xbee.ReceivedAttNavEnemy += new Radio.AttNavDelegate(xbee_ReceivedAttNavEnemy);
            trigger.sevenTimesKeyed += new Trigger.sevenTimesKeyedDelegate(trigger_sevenTimesKeyed);
            this.forDebugPrint = forDebugPrint;
            this.killLed = firingLed;
            this.statusLed = statusLed;
            this.gpsLed = gpsLed;
            this.trigger = trigger;


            enemyLocker = new Object();
            meLocker = new Object();
        }

        void trigger_sevenTimesKeyed(bool state)
        {
            magCal();
            forDebugPrint.TerminalPrintOut("\r\n!!!!!!!!!\r\n!!!!!!!!!!!!!!!!\r\n!!!!!!!!!!!!!\r\n!!!!!!!!just did a magCal!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\r\n!!!!!!!!\r\n!!!!!!!!!!\r\n");
            //Debug.Print("magCal()!!");
        }

        void xbee_ReceivedAttNavEnemy(AttNav shitIn)
        {
            lock (enemyLocker)
            {
                positionEnemyReceived = shitIn;
            }

            forDebugPrint.TerminalPrintOut("\r\n\t\t\t\t\t\t\t\t\t\t\t\tDOP_e2: " + positionEnemyReceived.PositionDOP_e2 + " Time: " + positionEnemyReceived.GPSTimeInWeek_csec);
        } // Assigning positionEnemyReceived

        // Insert trigger event handler, that changes a trigger variable true or false on both edges of trigger.

        void positionComputer_AttNavCreated(AttNav shitIn)
        {
            lock (meLocker)
            {
                positionMeReceived = shitIn;
            } // Now this event handler, which is running on clocked cycle, has an uptodate positionMe

            forDebugPrint.TerminalPrintOut("\n\rDOP_e2: " + positionMeReceived.PositionDOP_e2.ToString() + "  Time: " + positionMeReceived.GPSTimeInWeek_csec.ToString() + " Y: " + positionMeReceived.Yaw_deg.ToString() + " P: " + positionMeReceived.Pitch_deg.ToString());
            #region gpsLed Blink(rate DOP dependent
            if (positionMeReceived.PositionDOP_e2 == 0)
            {
                gpsLed.BlinkyBlink();
            }
            else
            {
                if (positionMeReceived.PositionDOP_e2 > 0 && positionMeReceived.PositionDOP_e2 <= 6000) // positionDOP, Ideal = 1000, good = 2000-5000, moderate = 6000-8000is, poor = up by 20,000
                {
                    gpsLed.Off();
                }
                else
                {
                    if (positionMeReceived.PositionDOP_e2 > 6000 && positionMeReceived.PositionDOP_e2 <= 2000)
                    {
                        gpsLed.Blink((30000 + ((20000000 - positionMeReceived.PositionDOP_e2 * 1000000) * 212)) / 1000); // This algorithm creates a scale from 20,000 to 6,000 that maps those end points to 30 to 3000, such that a dop of 20,000 => 30msec blink, and a dop of 6000 => 3000msec blink.  Everything is a huge number because it was all mulitplied by 1000 for integer operations.  0.212 is the ratio of 14000 to 2970 (which is the deltas on each end of the scales begin mapped to each other)
                    }
                    else
                    {
                        gpsLed.On();
                    }
                }
            }
            #endregion

            checkKillShot();
        }

        public void checkKillShot()
        {
            // Must copy the most recent positionMeReceived and positionEnemyReceived into the values that will be used.
            lock (enemyLocker)
            {
                positionEnemy = positionEnemyReceived;
            }
            lock (meLocker)
            {
                positionMe = positionMeReceived;
                // LTN: We should add a check here that checks to see if the positionMe has moved at all, and if it doesn't for a certain number of updates, throw a "stale data" flag
                // LTN: Yes, and because there's only 1 instance of FiringSolution, the memory of previous position Me's can be stored in FiringSolution, overwriting every 5 or so.
                // LTN&JPD: Maybe not, that sounds like a lot of work, when DOP should give us similar information.
            }

            // Guarding against bothering with the math if we have no positionEnemy
            if (positionEnemy == null)
            {
                //statusLed.BlinkyBlink(); //commented out to chase down heartbeat stalling with Main.Testing() commented out 29May12
                return;
            }

            if (positionEnemy.GPSTimeInWeek_csec == 0)
            {
                //statusLed.BlinkyBlink(); //commented out to chase down heartbeat stalling with Main.Testing() commented out 29May12
            }

            dlcForLogging = 0; //i'm lazy and don't want to figure out how to log the deadledcontrol class

            double latitudeMe_rad = (((double)positionMe.Latitude_e7) / 10000000) * exMath.PI / 180;
            double longitudeMe_rad = (((double)positionMe.Longitude_e7) / 10000000) * exMath.PI / 180;

            int delta_ECEF_X_cm = positionEnemy.X_cm - positionMe.X_cm;
            int delta_ECEF_Y_cm = positionEnemy.Y_cm - positionMe.Y_cm;
            int delta_ECEF_Z_cm = positionEnemy.Z_cm - positionMe.Z_cm;

            //see pic of matrix math: http://upload.wikimedia.org/wikipedia/en/math/6/c/5/6c5e10c1708acc1663d618c2f3fecc98.png
            double east_m = -exMath.Sin(longitudeMe_rad) * delta_ECEF_X_cm / 100 + exMath.Cos(longitudeMe_rad) * delta_ECEF_Y_cm / 100;
            double north_m = -exMath.Cos(longitudeMe_rad) * exMath.Cos(latitudeMe_rad) * delta_ECEF_X_cm / 100 - exMath.Sin(latitudeMe_rad) * exMath.Sin(longitudeMe_rad) * delta_ECEF_Y_cm / 100 + exMath.Cos(latitudeMe_rad) * delta_ECEF_Z_cm / 100;
            double up_m = exMath.Cos(latitudeMe_rad) * exMath.Cos(longitudeMe_rad) * delta_ECEF_X_cm / 100 + exMath.Cos(latitudeMe_rad) * exMath.Sin(longitudeMe_rad) * delta_ECEF_Y_cm / 100 + exMath.Sin(latitudeMe_rad) * delta_ECEF_Z_cm / 100;

            //distance_m = sqrt(dx^2 + dy^2 + dz^2)
            double distance_m = System.Math.Pow(System.Math.Pow(east_m, 2) + System.Math.Pow(north_m, 2) + System.Math.Pow(up_m, 2), 0.5);

            double toShootAzimuthOffset_rad;  // offset converts traditional polar math to compass heading.  See jeff's notebook drawings 3 Feb.
            north_m = north_m + 1.1; //for debug only: add a smidge to north_m so you're not dividing by zero

            // Determines how much offset to use:
            if (east_m > 0 && north_m > 0) //pointing in quadrant I
                toShootAzimuthOffset_rad = 0;
            else if (north_m < 0) //pointing in quadrant II or III
                toShootAzimuthOffset_rad = exMath.PI;
            else if (east_m < 0 && north_m > 0)//pointing in quadrant IV
                toShootAzimuthOffset_rad = 2 * exMath.PI;
            else
                toShootAzimuthOffset_rad = 999;

            //double toShootAzimuth_rad = exMath.Atan(east_m / north_m);
            toShootAzimuth_rad = exMath.Atan(east_m / north_m) + toShootAzimuthOffset_rad;
            double toShootAzimuth_deg = toShootAzimuth_rad * 180 / 3.1415926; // To Do: Delete this if we aren't using it for printout anymore.  It's a waste of computation.  LTN 2012_12_02
            forDebugPrint.TerminalPrintOut("\n\r\t\t\t\t\t\tTSA: " + toShootAzimuth_deg.ToString("F04") + "\trng_m: " + distance_m.ToString("F01"));

            double denominator = System.Math.Pow((System.Math.Pow(east_m, 2) + System.Math.Pow(north_m, 2)), 0.5);
            double toShootElevationAngle_rad = exMath.Atan(up_m / denominator);

            //this is a tolernace to be used when comparing caclulated angles for killshot. 
            double deadbandTolerance_mrad = 175; // 50mrad = ~2.5deg   //349mrad = ~20deg

            //we want to add magCal to yaw in order to keep the magnetic quantities together; the other option is to add it to toShootAzimuth (which is a GPS based angle)
            yawWithMagCal_mrad = positionMe.Yaw_mrad + yawOffsetFromMagCal_mrad;

            // warning! Mod in Octave is not the same as % in c#.  
            if (yawWithMagCal_mrad < 0)
            {
                yawWithMagCal_mrad = yawWithMagCal_mrad + (2 * exMath.PI * 1000);
            }
            else
            {
                yawWithMagCal_mrad = yawWithMagCal_mrad % 360;
            }


            // Metal Detector Magic!!!!! (like finding a gold coin on the beach)
            double maxOfAzimuths = exMath.Max(toShootAzimuth_rad * 1000, yawWithMagCal_mrad);
            double minOfAzimuths = exMath.Min(toShootAzimuth_rad * 1000, yawWithMagCal_mrad);
            double azimuthDelta_mrad = exMath.Min((2 * exMath.PI * 1000 - maxOfAzimuths) + minOfAzimuths, maxOfAzimuths - minOfAzimuths); // always pos by nature.
            double elevationDelta_mrad = toShootElevationAngle_rad * 1000 - positionMe.Pitch_mrad;
            double magnitudeOfDeltas_mrad = System.Math.Pow(azimuthDelta_mrad * azimuthDelta_mrad + elevationDelta_mrad * elevationDelta_mrad, 0.5);

            #region Compare magnitudeOfDeltas to deadBandTolerance and DLC Throw
            if (magnitudeOfDeltas_mrad < deadbandTolerance_mrad)
            {
                killLed.On();
                forDebugPrint.TerminalPrintOut("\n\r$\n\r$\n\r$\n\r$\n\r$I be KILLIN a MotherFucker NOW\n\r$\n\r$\n\r$\n\r$");

                DeadLedControl dlcToEnemy = new DeadLedControl(1);
                dlcForLogging = 1;

                if (this.DLCCreated != null)
                {
                    this.DLCCreated(dlcToEnemy);
                }
            }
            else
            {
                killLed.Blink((int)magnitudeOfDeltas_mrad);
                DeadLedControl dlcToEnemy = new DeadLedControl(2);

                if (this.DLCCreated != null)
                {
                    this.DLCCreated(dlcToEnemy);
                }
            }
            #endregion

            loggingCounter++;
            //TODO: add a guard aginst trying to log with no SD card installed
            if (loggingCounter > 10) //ten was just for simplicity. main-loop (mane-loop?) is currently 10Hz, so this was to get 1Hz data.
            {
                loggingCounter = 0;
                //logger.Initialize();
                logger.Log(positionMe.GPSTimeInWeek_csec.ToString() + "\t" + positionMe.Latitude_e7.ToString() + "\t" + positionMe.Longitude_e7.ToString() + "\t" + positionMe.MSLAlt_cm.ToString() + "\t" + positionMe.PositionDOP_e2 + "\t" + toShootAzimuth_rad.ToString("F04") + "\t" + positionMe.Yaw_mrad.ToString() + "\t" + yawWithMagCal_mrad.ToString() + "\t" + toShootElevationAngle_rad.ToString("F04") + "\t" + positionMe.Pitch_mrad.ToString() + "\t" + distance_m.ToString("F04") + "\t" + positionEnemy.Latitude_e7.ToString() + "\t" + positionEnemy.Longitude_e7.ToString() + "\t" + positionEnemy.MSLAlt_cm.ToString() + "\t" + positionEnemy.PositionDOP_e2.ToString() + "\t" + dlcForLogging.ToString() + "\t" + yawOffsetFromMagCal_mrad.ToString());
                //logger.Close();
                logger.Flush();
            }
        }

        public void magCal()
        {
            // Guarding against bothering with the math if we have no data
            if (positionMe == null || positionEnemy == null)
            {
                return;
            }
            //TODO: add a check to gaurd aginst completing the calucaltion if PDOP > distance_m

            // Step 1) Determine the magnitude of the delta between yaw and tsA
            double maxOfYawOrTSA = exMath.Max(toShootAzimuth_rad * 1000, positionMe.Yaw_mrad);
            double minOfYawOrTSA = exMath.Min(toShootAzimuth_rad * 1000, positionMe.Yaw_mrad);
            double splitNOTCrosingNorth = maxOfYawOrTSA - minOfYawOrTSA;
            double splitCrosingNorth = (2 * exMath.PI * 1000 - maxOfYawOrTSA) + minOfYawOrTSA;
            yawOffsetFromMagCal_mrad = exMath.Min(splitCrosingNorth, splitNOTCrosingNorth);

            // Step 2) Determine if yaw needs to move CW, or CCW to get to tsA.  CW = ture, CCW = false
            if (yawOffsetFromMagCal_mrad == splitNOTCrosingNorth)
            {
                if (toShootAzimuth_rad * 1000 < positionMe.Yaw_mrad)
                {
                    // CCW
                    yawOffsetFromMagCal_mrad = yawOffsetFromMagCal_mrad * (-1);
                }
            }
            else // yaw and tsA are on opposite sides of the North line.
            {
                if (toShootAzimuth_rad *1000 > positionMe.Yaw_mrad)
                {
                    // CCW
                    yawOffsetFromMagCal_mrad = yawOffsetFromMagCal_mrad * (-1);
                }
            }
            
            // random check, we should do more with this... but I intend to log yawOffsetFrom... so it will be evident if this becomes a problem
            if (yawOffsetFromMagCal_mrad >= 1570 || yawOffsetFromMagCal_mrad <= -1570)
            {
                forDebugPrint.TerminalPrintOut("\n\n\n\n\rAHHH FUCK! THE MagCal was greater than 90 degrees off... PANIC!");
            }
        }

        // Handler to the trigger interupt, and change TriggerBool to Yes or No.
        // Change Trigger Bool and keep the value here.


    }
}


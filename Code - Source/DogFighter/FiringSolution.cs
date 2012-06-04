using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace DogFighter
{
    class FiringSolution
    {
        // Declarations
        private AttNav positionEnemy; // This will be the "To Use" value.
        private AttNav positionEnemyReceived;
        private Object enemyLocker;
        private AttNav positionMe; // This will be the "To Use" value.
        private AttNav positionMeReceived;
        private Object meLocker;
        private Led killLed;
        private Led statusLed;
        private GPSIMU forDebugPrint;

        // Event Setup
        public delegate void DLCCreatedDelegate(DeadLedControl dlcToEnemy);
        public event DLCCreatedDelegate DLCCreated;

        // Initializer, used to give the Radio, PositionComputer, and FiringSolution classes each other's events.
        public void Initialize(PositionComputer positionComputer, Radio xbee, Trigger trigger, Led firingLed, Led statusLed, GPSIMU forDebugPrint)
        {
            positionComputer.AttNavCreated += new PositionComputer.AttNavDataReadyDelegate(positionComputer_AttNavCreated);
            xbee.ReceivedAttNavEnemy += new Radio.AttNavDelegate(xbee_ReceivedAttNavEnemy);

            this.forDebugPrint = forDebugPrint;

            this.killLed = firingLed;
            this.statusLed = statusLed;
            enemyLocker = new Object();
            meLocker = new Object();
        }

        void xbee_ReceivedAttNavEnemy(AttNav shitIn)
        {
            forDebugPrint.TerminalPrintOut("\r\nHere we are in xbee_ReceivedAttNavEnemy mutherfucker.");
            lock (enemyLocker)
            {
                positionEnemyReceived = shitIn;
            }
        } // Assigning positionEnemyReceived

        // Insert trigger event handler, that changes a trigger variable true or false on both edges of trigger.

        void positionComputer_AttNavCreated(AttNav shitIn)
        {
            lock (meLocker)
            {
                positionMeReceived = shitIn;
                forDebugPrint.TerminalPrintOut("\n\rDOP: " + positionMeReceived.PositionDOP_e2 + "  Time: " + positionMeReceived.GPSTimeInWeek_csec + "  Yaw: " + positionMeReceived.Yaw_mrad + "  Pitch: " + positionMeReceived.Pitch_mrad);
            } // Now this event handler, which is running on clocked cycle, has an uptodate positionMe

            checkKillShot();
        }

        private void checkKillShot()
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

            double latitudeMe_rad = (((double)positionMe.Latitude_e7) / 10000000) * exMath.PI / 180;
            double longitudeMe_rad = (((double)positionMe.Longitude_e7) / 10000000) * exMath.PI / 180;

            int delta_ECEF_X_cm = positionEnemy.X_cm - positionMe.X_cm;
            int delta_ECEF_Y_cm = positionEnemy.Y_cm - positionMe.Y_cm;
            int delta_ECEF_Z_cm = positionEnemy.Z_cm - positionMe.Z_cm;

            //see pic of matrix math: http://upload.wikimedia.org/wikipedia/en/math/6/c/5/6c5e10c1708acc1663d618c2f3fecc98.png
            double east_m = -exMath.Sin(longitudeMe_rad) * delta_ECEF_X_cm / 100 + exMath.Cos(longitudeMe_rad) * delta_ECEF_Y_cm / 100;
            double north_m = -exMath.Cos(longitudeMe_rad) * exMath.Cos(latitudeMe_rad) * delta_ECEF_X_cm / 100 - exMath.Sin(latitudeMe_rad) * exMath.Sin(longitudeMe_rad) * delta_ECEF_Y_cm / 100 + exMath.Cos(latitudeMe_rad) * delta_ECEF_Z_cm / 100;
            double up_m = exMath.Cos(latitudeMe_rad) * exMath.Cos(longitudeMe_rad) * delta_ECEF_X_cm / 100 + exMath.Cos(latitudeMe_rad) * exMath.Sin(longitudeMe_rad) * delta_ECEF_Y_cm / 100 + exMath.Sin(latitudeMe_rad) * delta_ECEF_Z_cm / 100;

            //distance = sqrt(dx^2 + dy^2 + dz^2)
            double distance = System.Math.Pow(System.Math.Pow(east_m, 2) + System.Math.Pow(north_m, 2) + System.Math.Pow(up_m, 2), 0.5);

            double toShootAzimuthOffset_rad;  // offset converts traditional polar math to compass heading.  See jeff's notebook drawings 3 Feb.
            north_m = north_m + 1.1; //for debug only: add a smidge to north_m so you're not dividing by zero

            // Determins how much offset to use:
            double toShootAzimuth_rad = exMath.Atan(east_m / north_m);
            if (east_m > 0 && north_m > 0) //pointing in quadrant I
                toShootAzimuthOffset_rad = 0;
            else if (north_m < 0) //pointing in quadrant II or III
                toShootAzimuthOffset_rad = exMath.PI;
            else if (east_m < 0 && north_m > 0)//pointing in quadrant IV
                toShootAzimuthOffset_rad = 2 * exMath.PI;
            else
                toShootAzimuthOffset_rad = 999;

            toShootAzimuth_rad = toShootAzimuth_rad + toShootAzimuthOffset_rad;
            forDebugPrint.TerminalPrintOut("\n\rToShootAzimuth_rad: " + toShootAzimuth_rad);

            double denominator = System.Math.Pow((System.Math.Pow(east_m, 2) + System.Math.Pow(north_m, 2)), 0.5);
            double toShootElevationAngle_rad = exMath.Atan(up_m / denominator);

            //this is a tolernace to be used when comparing caclulated angles for killshot. 
            double deadbandTolerance_mrad = 50; // 50mrad = ~2.5deg

            // Metal Detector Magic!!!!! (like finding a gold coin on the beach)
            double maxOfAzimuths = exMath.Max(toShootAzimuth_rad*1000, positionMe.Yaw_mrad);
            double minOfAzimuths = exMath.Min(toShootAzimuth_rad*1000, positionMe.Yaw_mrad);
            double azimuthDelta_mrad = exMath.Min((2*exMath.PI*1000-maxOfAzimuths)+minOfAzimuths, maxOfAzimuths-minOfAzimuths); // always pos by nature.
            double elevationDelta_mrad = toShootElevationAngle_rad * 1000 - positionMe.Pitch_mrad;
            double magnitudeOfDeltas_mrad = System.Math.Pow(azimuthDelta_mrad * azimuthDelta_mrad + elevationDelta_mrad * elevationDelta_mrad, 0.5);

            if (magnitudeOfDeltas_mrad < deadbandTolerance_mrad)
            {
                killLed.On();
                DeadLedControl dlcToEnemy = new DeadLedControl(1);

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

            // Create a dlcToEnemy here, and throw the event so that xbee will catch it and transmit.
            // (oooorrrrr, fuck the radio stream method, and just re-compute the kill shot math here)
        }

        // Handler to the trigger interupt, and change TriggerBool to Yes or No.
        // Change Trigger Bool and keep the value here.


    }
}

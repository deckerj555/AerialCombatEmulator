using System;
using Microsoft.SPOT;

namespace DogFighter
{
    class PositionComputer
    {
        #region Declarations
        private LineGPS gpsSampleReceived;
        private LineGPS gpsSampleToUse;
        private LineIMU imuSampleReceived;
        private LineIMU imuSampleToUse;
        private Object imuLocker;
        private Object gpsLocker;
        private bool gpsBool = false; // These will be tripped by the event copying something into gpsSampleReceived, and Compute won't run until that's happened at least 1 time.
        private bool imuBool = false;
        private Led imuLed;
        private Led gpsLed;
        GPSIMU forDebugPrintout;
        private int gpsTimeLast = 0;
        #endregion

        #region Event Setup
        public delegate void AttNavDataReadyDelegate(AttNav positionMe);
        public event AttNavDataReadyDelegate AttNavCreated;
        #endregion

        //Initializer (in place of Constructor, becaue the constructor just did what this does)
        public void Initialize(GPSIMU dataSource, Led imuLed, Led gpsLed) // dataSource is just the razorVenus, as sent when this constructor is called in Main.
        {
            // Subcribing to ReceivedLineGPS event & IMU
            dataSource.ReceivedLineGPS += new GPSIMU.LineGPSDelegate(dataSource_ReceivedLineGPS);
            dataSource.ReceivedLineIMU += new GPSIMU.LineIMUDelegate(dataSource_ReceivedLineIMU);

            this.imuLed = imuLed;
            this.gpsLed = gpsLed;

            forDebugPrintout = dataSource;
            imuLocker = new Object();
            gpsLocker = new Object();
        }

        void dataSource_ReceivedLineGPS(LineGPS shitIn)
        { // Event Handler Catching
            lock (gpsLocker)
            {
                gpsSampleReceived = shitIn;
            }
            gpsBool = true;            
        } // Assigning GpsSampleReceived

        void dataSource_ReceivedLineIMU(LineIMU shitIn)
        {
            lock (imuLocker)
            {
                imuSampleReceived = shitIn;
            }
            imuBool = true;
        } // Assigning imuSampleReceived


        public void Compute() // Compute runs on clocked cycle, called from Mane.Run() 
        {

            // If statement avoids a race condition of Compute() being called before the event handlers have assigned gps and imu data to use
            if (gpsBool == false || imuBool == false)
            {
                gpsLed.BlinkyBlink();
            }
            else
            {
                lock (gpsLocker)
                {
                    gpsSampleToUse = gpsSampleReceived;
                }
                lock (imuLocker)
                {
                    imuSampleToUse = imuSampleReceived;
                }
                // insert very clever kalman filter here....
                // insert future vector math here....

                if (gpsSampleToUse.GPSTimeInWeek_csec > gpsTimeLast) // gpsTimeLast1
                {
                    AttNav positionMe = new AttNav(gpsSampleToUse.ECEF_X_cm, gpsSampleToUse.ECEF_Y_cm, gpsSampleToUse.ECEF_Z_cm, imuSampleToUse.Pitch_mrad, imuSampleToUse.Yaw_mrad, imuSampleToUse.Roll_mrad, gpsSampleToUse.Latitude_e7, gpsSampleToUse.Longitude_e7, gpsSampleToUse.MSLAlt_cm, gpsSampleToUse.PositionDOP_e2, gpsSampleToUse.GPSTimeInWeek_csec);
                    if (this.AttNavCreated != null) // Pitches AttNav to anyone who might subscribe (Radio and FiringSolution)
                    {
                        this.AttNavCreated(positionMe);
                    }
                }
                else
                {   // Creates an AttNav with GPS Time, and positionDOP zero'ed out if gps time has not updated...indicates stale attnav.
                    AttNav positionMe = new AttNav(gpsSampleToUse.ECEF_X_cm, gpsSampleToUse.ECEF_Y_cm, gpsSampleToUse.ECEF_Z_cm, imuSampleToUse.Pitch_mrad, imuSampleToUse.Yaw_mrad, imuSampleToUse.Roll_mrad, gpsSampleToUse.Latitude_e7, gpsSampleToUse.Longitude_e7, gpsSampleToUse.MSLAlt_cm, 0, 0);
                    if (this.AttNavCreated != null) // pitches AttNav to anyone who might subscribe (Radio)
                    {
                        this.AttNavCreated(positionMe);
                    }
                }
                gpsTimeLast = gpsSampleToUse.GPSTimeInWeek_csec;
            }
        }
    }
}

using System;
using Microsoft.SPOT;

namespace DogFighter
{
    class PositionComputer
    {
        // Declarations
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

        // Event Setup
        public delegate void AttNavDataReadyDelegate(AttNav positionMe);
        public event AttNavDataReadyDelegate AttNavCreated;

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

            // Blink(rate dependent on DOP quality), or On if No Solution
            if (gpsSampleReceived.PositionDOP_e2 < 6000 && gpsSampleReceived.PositionDOP_e2 > 0) // positionDOP, Ideal = 1000, good = 2000-5000, moderate = 6000-8000is, poor = up by 20
            {
                gpsLed.Off();
            }
            else
            {
                if (gpsSampleReceived.PositionDOP_e2 == 0)
                {
                    gpsLed.On();
                }
                else
                {
                    gpsLed.Blink(gpsSampleReceived.PositionDOP_e2 / 10 + 300); // This number was made up, need to change to something useful...but for now, it makes small DOP values blink slow, and larger ones blink fast, until it reaches 
                }
            }

        } // Assigning GpsSampleReceived

        void dataSource_ReceivedLineIMU(LineIMU shitIn)
        {
            lock (imuLocker)
            {
                imuSampleReceived = shitIn;
            }
            imuBool = true;
            // The below if is to blinky blink when the object is pointing North.
            //commented out this if-else block to troubleshoot blinkyBlink causing the main loop to stall out. 29May12 JPD
            //if (shitIn.Yaw_mrad_true > 6180 || shitIn.Yaw_mrad_true < 100)
            //{
            //    if (shitIn.Pitch_mrad > -100 && shitIn.Pitch_mrad < 100)
            //    {
            //        imuLed.BlinkyBlink();
            //    }
            //    else
            //    {
            //        imuLed.Off();
            //    }
            //}
            //else
            //{
            //    imuLed.Off();
            //}
        } // Assigning imuSampleReceived


        public int Compute(int gpsTimeLastUsed)  // Compute runs on clocked cycle, called from Mane.Run() 
        {

            // if statement to avoid a race condition where compute is called before the event handlers have assigned gps and imu data to use
            if (gpsBool == false || imuBool == false)
            {
                return gpsTimeLastUsed;
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

                if (gpsSampleToUse.GPSTimeInWeek_csec > gpsTimeLastUsed)
                {
                    AttNav positionMe = new AttNav(gpsSampleToUse.ECEF_X_cm, gpsSampleToUse.ECEF_Y_cm, gpsSampleToUse.ECEF_Z_cm, imuSampleToUse.Pitch_mrad, imuSampleToUse.Yaw_mrad_true, imuSampleToUse.Roll_mrad, gpsSampleToUse.Latitude_e7, gpsSampleToUse.Longitude_e7, gpsSampleToUse.MSLAlt_cm, gpsSampleToUse.PositionDOP_e2, gpsSampleToUse.GPSTimeInWeek_csec);
                    if (this.AttNavCreated != null) // pitches AttNav to anyone who might subscribe (Radio)
                    {
                        this.AttNavCreated(positionMe);
                    }
                }
                else
                {   // Creates an AttNav with GPS Time, and positionDOP zero'ed out if gps time has not updated...indicates stale attnav.
                    AttNav positionMe = new AttNav(gpsSampleToUse.ECEF_X_cm, gpsSampleToUse.ECEF_Y_cm, gpsSampleToUse.ECEF_Z_cm, imuSampleToUse.Pitch_mrad, imuSampleToUse.Yaw_mrad_true, imuSampleToUse.Roll_mrad, gpsSampleToUse.Latitude_e7, gpsSampleToUse.Longitude_e7, gpsSampleToUse.MSLAlt_cm, 0, 0);
                    if (this.AttNavCreated != null) // pitches AttNav to anyone who might subscribe (Radio)
                    {
                        this.AttNavCreated(positionMe);
                    }
                }
                return gpsSampleToUse.GPSTimeInWeek_csec;
            }
        }
    }
}

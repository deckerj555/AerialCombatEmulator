using System;
using Microsoft.SPOT;

// This class exists to define the type LineGPS (an object).  Instances are created by razorVenus whenever it throws the RecievedLineGPS event.

namespace DogFighter
{
    class LineGPS : Object
    {
        private byte[] line;
        private byte fixMode;
        private byte spaceVehicles;
        private int gpsWeek;
        private int gpsTimeInWeek_csec;
        private int latitude_e7;
        private int longitude_e7;
        private int ellipsoidAlt_cm;
        private int mslAlt_cm;
        private int geometricDOP_e2;
        private int positionDOP_e2;
        //according to the data sheet the ecefs are sint32...but the example provided was larger than 2^16...need to verify ecefs won't get bigger than 2^16; use previous octave code
        private int ecef_x_cm;
        private int ecef_y_cm;
        private int ecef_z_cm;
        private int ecef_vx_cmps; // centimetersPerSecond (from Venus datasheet)
        private int ecef_vy_cmps;
        private int ecef_vz_cmps;


        //Constructor
        public LineGPS(byte[] line)
        {
            this.line = line;


            try
            {
                //line is zero-indexed, & venus documentation is 1-indexed; subtract 1 from all the field indexes given in the data sheet
                fixMode = line[1];
                spaceVehicles = line[2];
                gpsWeek = (line[3] << 8) + line[4];
                gpsTimeInWeek_csec = (line[5] << 24) + (line[6] << 16) + (line[7] << 8) + line[8]; // _csec is 1/100 of a second

                latitude_e7 = (line[9] << 24) + (line[10] << 16) + (line[11] << 8) + line[12];
                longitude_e7 = (line[13] << 24) + (line[14] << 16) + (line[15] << 8) + line[16];
                ellipsoidAlt_cm = (line[17] << 24) + (line[18] << 16) + (line[19] << 8) + line[20];
                mslAlt_cm = (line[21] << 24) + (line[22] << 16) + (line[23] << 8) + line[24];
                geometricDOP_e2 = (line[26] << 8) + line[27];
                positionDOP_e2 = (line[28] << 8) + line[29];

                ecef_x_cm = (int)(((int)line[35] << 24) + ((int)line[36] << 16) + ((int)line[37] << 8) + (int)line[38]); // remove 4 longs, and rather than add, OR them together.
                ecef_y_cm = (int)(((int)line[39] << 24) + ((int)line[40] << 16) + ((int)line[41] << 8) + (int)line[42]);
                ecef_z_cm = (int)(((int)line[43] << 24) + ((int)line[44] << 16) + ((int)line[45] << 8) + (int)line[46]);
                ecef_vx_cmps = (int)(((int)line[47] << 24) + ((int)line[48] << 16) + ((int)line[49] << 8) + (int)line[50]);
                ecef_vy_cmps = (int)(((int)line[51] << 24) + ((int)line[52] << 16) + ((int)line[53] << 8) + (int)line[54]);
                ecef_vz_cmps = (int)(((int)line[55] << 24) + ((int)line[56] << 16) + ((int)line[57] << 8) + (int)line[58]);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public static long LineMaxChars()
        {
            //return 75; //this is copie from LineIMU; is this a meaningful value, or just a WAG?
            return 130;  //a venus gps message is 65 fields long...maybe this should be 65*3 = 195 to account for two hex characters and a dash?  <shrug>
        }
        public byte[] CurrentLine
        {
            get
            {
                return line;
            }
        }
        public int GPSWeek
        {
            get
            {
                return gpsWeek;
            }
        }
        public int GPSTimeInWeek_csec
        {
            get
            {
                return gpsTimeInWeek_csec;
            }
        }
        public int ECEF_X_cm
        {
            get
            {
                return ecef_x_cm;
            }
        }
        public int ECEF_Y_cm
        {
            get
            {
                return ecef_y_cm;
            }
        }
        public int ECEF_Z_cm
        {
            get
            {
                return ecef_z_cm;
            }
        }
        public int ECEF_VX_cmps
        {
            get
            {
                return ecef_vx_cmps;
            }
        }
        public int ECEF_VY_cmps
        {
            get
            {
                return ecef_vy_cmps;
            }
        }
        public int ECEF_VZ_cmps
        {
            get
            {
                return ecef_vz_cmps;
            }
        }
        public int Latitude_e7
        {
            get
            {
                return latitude_e7;
            }
        }
        public int Longitude_e7
        {
            get
            {
                return longitude_e7;
            }
        }
        public int MSLAlt_cm
        {
            get
            {
                return mslAlt_cm;
            }
        }
        public int PositionDOP_e2
        {
            get
            {
                return positionDOP_e2;
            }
        }

    }
}

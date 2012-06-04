using System;
using Microsoft.SPOT;



// This class exists to define the type LineIMU (an object).  Instances are created by razorVenus whenever it throws the RecievedLineIMU event.

namespace DogFighter
{
    class LineIMU : Object
    {
        private byte[] line;
        private int pitch_mrad;
        private int yaw_mrad;
        private int roll_mrad;

        //Constructor
        public LineIMU(byte[] line)
        {
            this.line = line;

            try
            {
                // b0 b1 30 75 00 00 10 27 00 00 50 c3 00 00 0d 0a -- little endian!
                pitch_mrad = (((int)line[7] << 24) + ((int)line[6] << 16) + ((int)line[5] << 8) + ((int)line[4])) / 10; //units of mrads
                // pitch_mrad = (int)(pitch_mrad - System.Math.PI / 2 * 1000 + 0.5);

                yaw_mrad = (((int)line[11] << 24) + ((int)line[10] << 16) + ((int)line[9] << 8) + ((int)line[8])) / 10; // units of mrads
                // yaw_mrad = (int)(yaw_mrad - System.Math.PI * 1000 + 0.5);

                roll_mrad = ((((int)line[3]) << 24) + (((int)line[2]) << 16) + (((int)line[1]) << 8) + ((int)line[0])) / 10; // units of mrads
                // roll_mrad = (int)(roll_mrad - System.Math.PI * 1000 + 0.5);


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
        public int Roll_mrad
        {
            get
            {
                return roll_mrad;
            }
        }
        public int Pitch_mrad
        {
            get
            {
                return pitch_mrad;
            }
        }
        public int Yaw_mrad_true
        {
            get
            {
                return yaw_mrad + 279; // about 16deg HARD CODED FOR LOCAL DECLINATION
            }
        }

    }
}

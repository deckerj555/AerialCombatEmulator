using System;
using Microsoft.SPOT;


namespace DogFighter
{

    class AttNav
    {
        // Declarations
        int x_cm;
        int y_cm;
        int z_cm;
        int pitch_mrad;
        int yaw_mrad;
        int roll_mrad;
        int latitude_e7;
        int longitude_e7;
        int mslAlt_cm;
        int positionDOP_e2;
        int gpsTimeInWeek_csec;
        int deadLedControl;
        
        // Constructor from PositionComputer
        public AttNav(int x_cm, int y_cm, int z_cm, int pitch_mrad, int yaw_mrad, int roll_mrad, int latitude_e7, int longitude_e7, int mslAlt_cm, int positionDOP_e2, int gpsTimeInWeek_csec)
        {
            this.x_cm = x_cm;
            this.y_cm = y_cm;
            this.z_cm = z_cm;
            this.pitch_mrad = pitch_mrad;
            this.yaw_mrad = yaw_mrad;
            this.roll_mrad = roll_mrad;
            this.latitude_e7 = latitude_e7;
            this.longitude_e7 = longitude_e7;
            this.mslAlt_cm = mslAlt_cm;
            this.positionDOP_e2 = positionDOP_e2;
            this.gpsTimeInWeek_csec = gpsTimeInWeek_csec;
        }

        // Overloaded constructor soley for the purpose of providing Radio with the size of an AttNav
        public AttNav()
        {
            // Nothing ever needs to go in here.
        }

        public static byte[] Serialize(AttNav attNav)
        {
            byte[] buffer = new byte[44]; // Size must be increased if AttNav is changed.

            Serialize(buffer, 0, attNav.x_cm);
            Serialize(buffer, 4, attNav.y_cm);  
            Serialize(buffer, 8, attNav.z_cm);  
            Serialize(buffer, 12, attNav.pitch_mrad); 
            Serialize(buffer, 16, attNav.yaw_mrad);
            Serialize(buffer, 20, attNav.roll_mrad);
            Serialize(buffer, 24, attNav.latitude_e7);
            Serialize(buffer, 28, attNav.longitude_e7);
            Serialize(buffer, 32, attNav.mslAlt_cm);
            Serialize(buffer, 36, attNav.positionDOP_e2);
            Serialize(buffer, 40, attNav.gpsTimeInWeek_csec);

            return buffer;
        }

        public static AttNav Deserialize(byte[] line)
        {
            int a = (line[0] << 24) | (line[1] << 16) | (line[2] << 8) | (line[3] << 0);
            int b = (line[4] << 24) | (line[5] << 16) | (line[6] << 8) | (line[7] << 0);
            int c = (line[8] << 24) | (line[9] << 16) | (line[10] << 8) | (line[11] << 0);
            int d = (line[12] << 24) | (line[13] << 16) | (line[14] << 8) | (line[15] << 0);
            int e = (line[16] << 24) | (line[17] << 16) | (line[18] << 8) | (line[19] << 0);
            int f = (line[20] << 24) | (line[21] << 16) | (line[22] << 8) | (line[23] << 0);
            int g = (line[24] << 24) | (line[25] << 16) | (line[26] << 8) | (line[27] << 0);
            int h = (line[28] << 24) | (line[29] << 16) | (line[30] << 8) | (line[31] << 0);
            int i = (line[32] << 24) | (line[33] << 16) | (line[34] << 8) | (line[35] << 0);
            int j = (line[36] << 24) | (line[37] << 16) | (line[38] << 8) | (line[39] << 0);
            int k = (line[40] << 24) | (line[41] << 16) | (line[42] << 8) | (line[43] << 0);

            AttNav deserialized = new AttNav(a, b, c, d, e, f, g, h, i, j, k);
            return deserialized;
        }

        private static void Serialize(byte[] buffer, int offset, int input) 
        {
            buffer[offset + 0] = (byte)((input >> 24) & 0xFF); //take the top 8bits, AND them with 0xFF
            buffer[offset + 1] = (byte)((input >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((input >> 8) & 0xFF);
            buffer[offset + 3] = (byte)((input >> 0) & 0xFF);

        }
        private static void Serialize(byte[] buffer, int offset, ulong input) // this was all me
        {
            buffer[offset] = (byte)(input >> 24);
            buffer[offset + 1] = (byte)(input >> 16);
            buffer[offset + 2] = (byte)(input >> 8);
            buffer[offset + 3] = (byte)(input);
        }

        // Properties
        public int X_cm
        {
            get
            {
                return x_cm;
            }
        }
        public int Y_cm
        {
            get
            {
                return y_cm;
            }
        }
        public int Z_cm
        {
            get
            {
                return z_cm;
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
        public int Yaw_mrad
        {
            get
            {
                return yaw_mrad;
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
        public int GPSTimeInWeek_csec
        {
            get
            {
                return gpsTimeInWeek_csec;
            }
        }

    }
}

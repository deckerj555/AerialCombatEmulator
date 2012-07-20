#include <stdio.h> 

unsigned char imuHeader[] = {0xb0, 0xb1};
unsigned char imuFooter[] = {0x0d, 0x0a};
unsigned char gpsHeader[] = {0xa0, 0xa1, 0x00, 0x3b, 0xa8};
unsigned char gpsFooter[] = {0x0d, 0x0a};

unsigned char imu[] = {0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a};

unsigned char gps[] = {0xa0, 0xa1, 0x00, 0x3b, 0xa8, 0x02, 0x07, 0x06,
                             0x6f, 0x03, 0x83, 0x9c, 0xc8, 0x1b, 0x41, 0xd3, 
                             0xba, 0xb7, 0x92, 0xa0, 0x8b, 0x00, 0x00, 0x08, 
                             0xa1, 0x00, 0x00, 0x10, 0x17, 0x00, 0x7b, 0x00, 
                             0x44, 0x00, 0xd4, 0x00, 0xf5, 0x00, 0xc5, 0xf2, 
                             0x1a, 0xfa, 0x7a, 0xe9, 0x56, 0x5f, 0xe8, 0x1b, 
                             0x16, 0x29, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 
                             0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x38, 
                             0x0d, 0x0a};

unsigned char combo[] = {0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xa0, 0xa1, 0x00, 0x3b, 0xa8, 0x02, 0x07, 0x06,
                       0x6f, 0x03, 0x83, 0x9c, 0xc8, 0x1b, 0x41, 0xd3, 
                       0xba, 0xb7, 0x92, 0xa0, 0x8b, 0x00, 0x00, 0x08, 
                       0xa1, 0x00, 0x00, 0x10, 0x17, 0x00, 0x7b, 0x00, 
                       0x44, 0x00, 0xd4, 0x00, 0xf5, 0x00, 0xc5, 0xf2, 
                       0x1a, 0xfa, 0x7a, 0xe9, 0x56, 0x5f, 0xe8, 0x1b, 
                       0x16, 0x29, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 
                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x38, 
                       0x0d, 0x0a,
                       0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xb0, 0xb1, 0x30, 0x75, 0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x50, 0xc3, 0x00, 0x00, 0x0d, 0x0a,
                       0xa0, 0xa1, 0x00, 0x3b, 0xa8, 0x02, 0x07, 0x06,
                       0x6f, 0x03, 0x83, 0x9c, 0xc8, 0x1b, 0x41, 0xd3, 
                       0xba, 0xb7, 0x92, 0xa0, 0x8b, 0x00, 0x00, 0x08, 
                       0xa1, 0x00, 0x00, 0x10, 0x17, 0x00, 0x7b, 0x00, 
                       0x44, 0x00, 0xd4, 0x00, 0xf5, 0x00, 0xc5, 0xf2, 
                       0x1a, 0xfa, 0x7a, 0xe9, 0x56, 0x5f, 0xe8, 0x1b, 
                       0x16, 0x29, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 
                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x38, 
                       0x0d, 0x0a};

int i;
 void main()
{
   for (i=0; i<sizeof(combo); i++)
      {

      if(combo[i]==imuHeader[0] && combo[i+1]==imuHeader[1] && combo[i+14]==imuFooter[0] && combo[i+15]==imuFooter[1])
         {
         printf("IMU header at %d, current combo value is %x\n", i, combo[i]);
         }

      if(combo[i]==gpsHeader[0] && combo[i+4]==gpsHeader[4] && combo[i+64]==gpsFooter[0] && combo[i+65]==gpsFooter[1])
         {
         printf("GPS Header at %d, current combo value is %x\n", i, combo[i]);
         }

      }
}
using System;
using Microsoft.SPOT;

namespace DogFighter
{
    class EnemyEmulator
    {
        // For testing purposes, the "enemy" Netduino will not run the usual code in Main.Run().
        // Instead, it will call one of this class's functions to output a dummy positionEnemy to the radio
        // All we HAVE to have from the enemy to check is his ECEF coords.  

        //LLA of Mount Defiance = 45.648727,-121.722451, 1503.6394m
        //ECEF: X = -2.3490e+06m, Y = -3.8000e+06m, Z =  4.5391e+06m
        //Scale like venus: X = -2.3490e+08m, Y = -3.8000e+08m, Z =  4.5391e+08m
        //then converted to Hex: -E004A20, -16A65700 ,1B0E1DF0
        private byte[] ecefXHex = { 0x0E, 0x00, 0x4A, 0x20 };  //has to be negative...how? 
        private byte[] ecefYHex = { 0x16, 0xA6, 0x57, 0x00 };  //has to be negative...how? 
        private byte[] ecefZHex = { 0x1B, 0x0E, 0x1D, 0xF0 };
        
        

        

    }
}

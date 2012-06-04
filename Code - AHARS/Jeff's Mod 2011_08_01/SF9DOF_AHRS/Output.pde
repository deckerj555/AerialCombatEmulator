
void printdata(void)
{  

#if WRITE_HEX == 1
  // make the angles unsigned such that 0 < roll, yaw < 2pi, and 0 < pitch < pi, then cast them into u_long  
  // (Note: This means a plane flying level...is at Pi/2 pitch angle) 
  //unsigned long roll_long = (unsigned long) ((roll + 3.14159265) * 10000); // units of 10,000 radians
  //unsigned long yaw_long = (unsigned long) ((yaw + 3.14159265) * 10000);
  //unsigned long pitch_long = (unsigned long) ((pitch + 1.570796325) * 10000);
  
  signed long roll_long = (signed long) (roll * 10000); // units of 10,000 radians
  signed long yaw_long = (((signed long)(yaw*10000))+62831)%62831;
  signed long pitch_long = (signed long) (pitch * 10000);

//  Serial.print("!");
//  Serial.print(roll_long);
//  Serial.print(",   ");
//  Serial.print(pitch_long);
//  Serial.print(",   ");
//  Serial.print(yaw_long);
//  Serial.println();
  Serial.write(0xB0);
  Serial.write(0xB1);
  Serial.write((uint8_t*)&roll_long, sizeof(roll_long));
  Serial.write((uint8_t*)&pitch_long, sizeof(pitch_long));
  Serial.write((uint8_t*)&yaw_long, sizeof(yaw_long));
  Serial.write(0x0D);
  Serial.write(0x0A);
#endif


#if PRINT_EULER == 1
  Serial.print("!");
  //      Serial.print(timer);   // JPD 23Dec2010
  //      Serial.print(",");
  //      Serial.print("ANG:"); // commented out to make serial to the arduino easier JPD 3Jan2011
  Serial.print((long)(ToDeg(roll)));  
  Serial.print(",");
  Serial.print((long)(ToDeg(pitch)));
  Serial.print(",");
  Serial.print((long)(ToDeg(yaw)));
  Serial.println();
#endif      
#if PRINT_ANALOGS==1
  Serial.print(",AN:");
  Serial.print(AN[sensors[0]]);  //(int)read_adc(0)
  Serial.print(",");
  Serial.print(AN[sensors[1]]);
  Serial.print(",");
  Serial.print(AN[sensors[2]]);  
  Serial.print(",");
  Serial.print(ACC[0]);
  Serial.print (",");
  Serial.print(ACC[1]);
  Serial.print (",");
  Serial.print(ACC[2]);
  Serial.print(",");
  Serial.print(magnetom_x);
  Serial.print (",");
  Serial.print(magnetom_y);
  Serial.print (",");
  Serial.print(magnetom_z);     
  Serial.println(); 
#endif
  /*#if PRINT_DCM == 1
   Serial.print (",DCM:");
   Serial.print(convert_to_dec(DCM_Matrix[0][0]));
   Serial.print (",");
   Serial.print(convert_to_dec(DCM_Matrix[0][1]));
   Serial.print (",");
   Serial.print(convert_to_dec(DCM_Matrix[0][2]));
   Serial.print (",");
   Serial.print(convert_to_dec(DCM_Matrix[1][0]));
   Serial.print (",");
   Serial.print(convert_to_dec(DCM_Matrix[1][1]));
   Serial.print (",");
   Serial.print(convert_to_dec(DCM_Matrix[1][2]));
   Serial.print (",");
   Serial.print(convert_to_dec(DCM_Matrix[2][0]));
   Serial.print (",");
   Serial.print(convert_to_dec(DCM_Matrix[2][1]));
   Serial.print (",");
   Serial.print(convert_to_dec(DCM_Matrix[2][2]));
   Serial.println();
   #endif*/


}

long convert_to_dec(float x)
{
  return x*10000000;
}




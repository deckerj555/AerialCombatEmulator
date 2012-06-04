#include <stdio.h>

//use gcc -mno-cygwin to complie without cygwin dependies

/* Qx:
 - why does buffer choke if you try to left shift more than 31 bits (as a uint8.  shouldn't it choke at more than 7-bits? (in jeff32.c i'm left shifting 24 bits, without complaint)
 

*/


int main() {
  
  int64_t buffer[8];
  int64_t n;
  int64_t n2;
  uint8_t i;
  
  uint32_t t1;
  
  n = -127;  

if(sizeof(n)==8)
{
   printf("n is int64\n\r"); 
   //gives the correct hex array e.g. -2 = ff ff ff ff ff ff ff fe
   
   //t1 = n >> 56;
   //printf("t1 = %d (0x%x)\r\n", t1, t1);
   
   buffer[0] = (int8_t)((n >> 56) & 0xFF);
   buffer[1] = (int8_t)((n >> 48) & 0xFF);
   buffer[2] = (int8_t)((n >> 40) & 0xFF);
   buffer[3] = (int8_t)((n >> 32) & 0xFF);
   buffer[4] = (int8_t)((n >> 24) & 0xFF);
   buffer[5] = (int8_t)((n >> 16) & 0xFF);
   buffer[6] = (int8_t)((n >>  8) & 0xFF);
   buffer[7] = (int8_t)((n >>  0) & 0xFF);
}


  
  i = 0;
  while(i < sizeof(buffer))
  {
    printf("buffer[%d] = %x\r\n", i, buffer[i]);
	i++;
  }

    t1 = (uint8_t)(buffer[0]<<56); 
    printf("t1 = %d (0x%x)\r\n", t1, t1);	 
	
	n2 = (int64_t)(((int8_t)(buffer[0]<<56))
	| ((int8_t)(buffer[1]<<48))
	| ((int8_t)(buffer[2]<<40))
	| ((int8_t)(buffer[3]<<32))
	| ((int8_t)(buffer[4]<<24))
	| ((int8_t)(buffer[5]<<16))
	| ((int8_t)(buffer[6]<<8))
	| ((int8_t)(buffer[7]<<0)));
	//n2 = buffer[0]<<24 | buffer[1]<<16 | buffer[2]<<8 | buffer[3]<<0;

	
	
	printf("re-constituted n = %d (0x%x)", n2, n2);

  return 0;
}

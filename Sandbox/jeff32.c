#include <stdio.h>

//use gcc -mno-cygwin to complie without cygwin dependies

/* if the width of the var is 32-bit, to serialize the var, you must shift the top 8-bits only by 24, otherwise the "shift count >= width of type".

*/


int main() {
  
  uint8_t buffer[4];
  int32_t n;
  int32_t n2;
  int32_t i;
  
  n = -234847615;



   printf("n is int32\n\r");
   buffer[0] = (int8_t)((n >> 24) & 0xFF);
   buffer[1] = (int8_t)((n >> 16) & 0xFF);
   buffer[2] = (int8_t)((n >>  8) & 0xFF);
   buffer[3] = (int8_t)((n >>  0) & 0xFF);

  
  i = 0;
  while(i < sizeof(buffer))
  {
    printf("buffer[%d] = %x\r\n", i, buffer[i]);
	i++;
  }
 

    n2 = buffer[0]<<24| buffer[1]<<16 | buffer[2]<<8 | buffer[3]<<0;

	
	printf("re-constituted n = %d (0x%x)", n2, n2);

  return 0;
}

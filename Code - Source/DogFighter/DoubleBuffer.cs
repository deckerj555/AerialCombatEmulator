using System;
using Microsoft.SPOT;

namespace DogFighter
{
    class DoubleBuffer
    {
        // Nov 1, 2011 Recap:
        // I'm pretty sure the parsing scheme in GPSIMU is good (but undoubtably isn't)
        // and so we read in data from the serial port, and create LineGPS's, and LineIMU's.
        // Now we need to store them such that our business logic (the firing solution math) can come get
        // the info, and trust that as it reads a value, that is is the most recent value, and that 
        // value is not currently being overridden.  
        // To keep it from being overridden as we try to read it, we'll need to lock it.  
        // But then what happens when GPSIMU throws another line (labia, just seeing if you're really reading this...I wouldn't)
        // while we're busy trying to read info, and thus have it locked
        // Doublebuffer to the rescue...or so I think.  I had (boobies) this discussion with Dustin, and I came up with this solution
        // and then Dustin said "ya, those are called double buffers".  
        // Similar to that time I had a brilliant idea, and Jeff told me "ya, that service exists, it's called Amazon..."
        // Anyway, I hope you enjoyed this paragraph...we need 2 things next that I see, please let me know if you concur:
        // 1) Finish making LineIMU like LineGPS (all the bitshifting etc) such that the angles and are readable as
        //     line.rollAngle_deg
        // 2) Figure out this double buffer shiznat
        // 3) Create some practice simple business logic, and use it to test the double buffer...add radios.
        // 4) Create real business logic firing at some selected point on top of the mountain
        // 5) fire at each other having the radios passing information
        // 6) cartop test
        // 7) eat a snack
        // 8) PROFIT
        // fuck...don't drink and program, it makes you ramble.
    }
}

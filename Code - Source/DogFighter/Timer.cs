using System;
using Microsoft.SPOT;

namespace DogFighter
{
    class Timer
    {
        long startMarker = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;

        public void Start()
        {
            startMarker = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
        }

        /// <returns>seconds</returns>
        public double Stop()
        {
            long stopMarker = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
            long delta_ticks;

            if (stopMarker > startMarker)
            {
                delta_ticks = stopMarker - startMarker;
            }
            else // the hardware tick clock must have wrapped during our measurement time
            {
                delta_ticks = stopMarker + (long.MaxValue - startMarker);
            }

            return (double)delta_ticks/System.TimeSpan.TicksPerSecond;
        }
        
        

    }
}

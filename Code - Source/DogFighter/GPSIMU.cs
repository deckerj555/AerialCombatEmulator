using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;
using System.Text;

// Input/Setup: Controls Serial Port COM1.  Only 1 instance is created, "razorVenus" created by Mane.
// Operation:   Handles parsing GPS or IMU information coming into COM1
// Output:      Throws the events ReceivedLineGPS or ReceivedLineIMU

namespace DogFighter
{
    class GPSIMU
    {
        // Declarations
        private SerialPort physicalSerialPort;
        private const int rate = 38400; //default for venus gps = 9600, //when venus set for 10hz, 38400 is used.
        private byte[] bigBuffer = new byte[512];   //OG line
        private int tailIndex = 0;
        private bool gpsHeaderFound = false;
        private bool imuHeaderFound = false;
        private byte[] completeGPSLine = new byte[59];
        private byte[] completeIMULine = new byte[15];
        private byte[] gpsHeader = { 0xA0, 0xA1, 0x00, 0x3B, 0xA8 };
        private byte[] gpsFooter = { 0x0D, 0x0A };
        private byte[] imuHeader = { 0xB0, 0xB1 };
        private byte[] imuFooter = { 0x0D, 0x0A };

        

        // Constructor
        public GPSIMU()
        {
            physicalSerialPort = new SerialPort("COM1", rate, Parity.None, 8, StopBits.One);
            physicalSerialPort.DataReceived += new SerialDataReceivedEventHandler(physicalSerialPort_DataReceived);
            physicalSerialPort.Open();
        }

        // Events Setup
        public delegate void LineGPSDelegate(LineGPS shitIn);
        public event LineGPSDelegate ReceivedLineGPS;

        public delegate void LineIMUDelegate(LineIMU shitIn);
        public event LineIMUDelegate ReceivedLineIMU;
       

        // DataRecieved Event Handler "Method"
        void physicalSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesPutIn = physicalSerialPort.Read(bigBuffer, tailIndex, bigBuffer.Length - tailIndex);
            tailIndex += bytesPutIn;

            // Parse baby, parse
            if (gpsHeaderFound == false && imuHeaderFound == false)
            {
                for (int i = 0; i < tailIndex; i++)
                {
                    if (i <= tailIndex - gpsHeader.Length)
                    {
                        if (bigBuffer[i] == gpsHeader[0] && bigBuffer[i + gpsHeader.Length-1] == gpsHeader[gpsHeader.Length-1])
                        {
                            tailIndex = tailIndex - i;
                            Array.Copy(bigBuffer, i, bigBuffer, 0, tailIndex);
                            gpsHeaderFound = true;
                            break;
                        }
                        if (bigBuffer[i] == imuHeader[0] && bigBuffer[i + imuHeader.Length - 1] == imuHeader[imuHeader.Length - 1])
                        {
                            tailIndex = tailIndex - i;
                            Array.Copy(bigBuffer, i, bigBuffer, 0, tailIndex);
                            imuHeaderFound = true;
                            break;
                        }
                    }
                    else
                    {
                        if (i > 0)
                        {//if no headers found, copy what's left back to the start of bigbuffer, and wait for more data...
                            tailIndex = tailIndex - i;
                            Array.Copy(bigBuffer, i, bigBuffer, 0, tailIndex);
                            break;
                        }
                    }
                }
            }

            if (gpsHeaderFound == true && tailIndex > 65) // Note: tailIndex is Litterally, the number of bytes we have in bigBuffer...which means the largest index possible is 64... but by waiting for 1 more byte to come in (because of the >), we make the indexing easier to look at in the below lines as well.
            { 
                if (bigBuffer[64] == gpsFooter[0] && bigBuffer[65] == gpsFooter[1])
                {
                    Array.Copy(bigBuffer, 4, completeGPSLine, 0, 59); //this is an extra aray.copy, that should be replaced by just throwing the line from bigbuffer[4:59], but it's late, and i'm unsure of the syntax
                    LineGPS line = new LineGPS(completeGPSLine);

                    if (this.ReceivedLineGPS != null) //if someone has subscribed to this event, throw it.
                    {
                        this.ReceivedLineGPS(line);
                    }
                }
                tailIndex = tailIndex - 65;
                Array.Copy(bigBuffer, 65, bigBuffer, 0, tailIndex);
                gpsHeaderFound = false;
            }

            if (imuHeaderFound == true && tailIndex > 15)
            {
                if ((bigBuffer[14] == imuFooter[0] && bigBuffer[15] == imuFooter[1])) //POSSIBLE OFF BY 1 ERROR, CHECK THE 15 INDEX ABOVE
                {
                    Array.Copy(bigBuffer, 2, completeIMULine, 0, 14); 
                    LineIMU line = new LineIMU(completeIMULine);

                    if (this.ReceivedLineIMU != null)
                    {
                        this.ReceivedLineIMU(line);
                    }
                }
                tailIndex = tailIndex - 15;
                Array.Copy(bigBuffer, 15, bigBuffer, 0, tailIndex);
                imuHeaderFound = false;
            }
        }


        public void TerminalPrintOut(string toPrint)
        {
            // Print data out the Tx line to the terminal
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            byte[] bytesToSend = encoder.GetBytes(toPrint);
            physicalSerialPort.Write(bytesToSend, 0, bytesToSend.Length);
        }

    }
}
